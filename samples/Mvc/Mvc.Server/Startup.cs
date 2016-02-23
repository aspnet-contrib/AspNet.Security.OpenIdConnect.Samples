using System;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mvc.Server.Extensions;
using Mvc.Server.Models;
using Mvc.Server.Providers;
using NWebsec.Middleware;

namespace Mvc.Server {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<ApplicationContext>(options => {
                    options.UseInMemoryDatabase();
                });

            services.Configure<SharedAuthenticationOptions>(options => {
                options.SignInScheme = "ServerCookie";
            });

            services.AddAuthentication();

            services.AddAuthorization(options => {
                // Add a new policy requiring a "scope" claim
                // containing the "api-resource-controller" value.
                options.AddPolicy("API", policy => {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(OpenIdConnectConstants.Claims.Scope, "api-resource-controller");
                });
            });

            services.AddCaching();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) {
            var factory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            factory.AddConsole();

            app.UseIISPlatformHandler(options => {
                options.AuthenticationDescriptions.Clear();
            });

            app.UseStaticFiles();

            // Create a new branch where the registered middleware will be executed only for API calls.
            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")), branch => {
                branch.UseJwtBearerAuthentication(options => {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.RequireHttpsMetadata = false;

                    options.Audience = "http://localhost:54540/";
                    options.Authority = "http://localhost:54540/";
                });
            });

            // Create a new branch where the registered middleware will be executed only for non API calls.
            app.UseWhen(context => !context.Request.Path.StartsWithSegments(new PathString("/api")), branch => {
                // Insert a new cookies middleware in the pipeline to store
                // the user identity returned by the external identity provider.
                branch.UseCookieAuthentication(options => {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.AuthenticationScheme = "ServerCookie";
                    options.CookieName = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.LoginPath = new PathString("/signin");
                });

                branch.UseGoogleAuthentication(options => {
                    options.ClientId = "560027070069-37ldt4kfuohhu3m495hk2j4pjp92d382.apps.googleusercontent.com";
                    options.ClientSecret = "n2Q-GEw9RQjzcRbU3qhfTj8f";
                });

                branch.UseTwitterAuthentication(options => {
                    options.ConsumerKey = "6XaCTaLbMqfj6ww3zvZ5g";
                    options.ConsumerSecret = "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI";
                });
            });

            // Note: visit https://docs.nwebsec.com/en/4.2/nwebsec/Configuring-csp.html for more information.
            app.UseCsp(options => options.DefaultSources(configuration => configuration.Self())
                                         .ImageSources(configuration => configuration.Self().CustomSources("data:"))
                                         .ScriptSources(configuration => configuration.UnsafeInline())
                                         .StyleSources(configuration => configuration.Self().UnsafeInline()));

            app.UseXContentTypeOptions();

            app.UseXfo(options => options.Deny());

            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.UseOpenIdConnectServer(options => {
                options.Provider = new AuthorizationProvider();

                // Note: see AuthorizationController.cs for more
                // information concerning ApplicationCanDisplayErrors.
                options.ApplicationCanDisplayErrors = true;
                options.AllowInsecureHttp = true;

                // Note: by default, tokens are signed using dynamically-generated
                // RSA keys but you can also use your own certificate:
                // options.SigningCredentials.AddCertificate(certificate);
            });

            app.UseMvc();

            app.UseWelcomePage();

            using (var database = app.ApplicationServices.GetService<ApplicationContext>()) {
                database.Applications.Add(new Application {
                    ApplicationID = "myClient",
                    DisplayName = "My client application",
                    RedirectUri = "http://localhost:53507/signin-oidc",
                    LogoutRedirectUri = "http://localhost:53507/",
                    Secret = "secret_secret_secret"
                });

                database.SaveChanges();
            }
        }
    }
}