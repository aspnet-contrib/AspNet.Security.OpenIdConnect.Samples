using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Hosting;
using HelloSignalR.Connections;
using Microsoft.Extensions.Logging;
using HelloSignalR.Providers;
using Microsoft.AspNet.Http;
using System;
using Microsoft.AspNet.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace HelloSignalR {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddAuthentication();
            services.AddSignalR();
            services.AddCaching();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger) {
            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            if (env.IsDevelopment()) {
                // In case any errors occur.
                app.UseDeveloperExceptionPage();

                loggerFactory.MinimumLevel = LogLevel.Information;
                loggerFactory.AddConsole();
                loggerFactory.AddDebug();
            }

            // Configure the HTTP request pipeline for our front-end files.
            app.UseDefaultFiles();
            app.UseStaticFiles();

            var hostname = "http://localhost:5000/";

            app.UseJwtBearerAuthentication(options => {
                // We need this to enable authentication for all requests.
                options.AutomaticAuthenticate = true;

                // Your audience that has to be allowed by your Authority server.
                options.Audience = hostname;

                // Authority that issued your token (i.e. your identity server).
                options.Authority = hostname;

                options.Events = new JwtBearerEvents {
                    // For SignalR connections default Authorization header does not work,
                    // Because there are no Headers in WebSockets specification
                    // This is why we have to implement our own way to retrieve token
                    // Which can be as easy as adding it to query string
                    OnReceivingToken = context => {
                        // Set token to '?access_token={token}' query string value
                        // If the token is not there, context.Request.Query["access_token"] will return null
                        // And OIDC server will try to extract it from Headers["Authorization"]
                        context.Token = context.Request.Query["access_token"];
                        return Task.FromResult(true);
                    }
                };

                if (env.IsDevelopment()) {
                    // For not requiring HTTPS in localhost.
                    options.RequireHttpsMetadata = false;
                }
            });

            // Add WebSockets handling for SignalR to support it.
            app.UseWebSockets();

            app.UseSignalR<SimpleConnection>("/signalr");

            // Add token issuing middleware.
            app.UseOpenIdConnectServer(config => {
                // Your Auhentication server.
                config.Provider = new AuthenticationProvider();
                config.Issuer = new Uri(hostname);
            });

            app.Use(async (context, next) => {
                var identity = context.User.Identity;
                if (!identity.IsAuthenticated) {
                    await next();
                } else {
                    await context.Response.WriteAsync($"Username: {identity.Name}");
                }
            });
        }
    }
}
