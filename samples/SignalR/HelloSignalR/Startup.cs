using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using HelloSignalR.Connections;
using HelloSignalR.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HelloSignalR {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddAuthentication();

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app) {
            app.UseDefaultFiles();

            app.UseStaticFiles();

            // Add a new middleware validating access tokens.
            app.UseOAuthValidation(options => {
                options.Events = new OAuthValidationEvents {
                    // Note: for SignalR connections, the default Authorization header does not work,
                    // because the WebSockets JS API doesn't allow setting custom parameters.
                    // To work around this limitation, the access token is retrieved from the query string.
                    OnRetrieveToken = context => {
                        context.Token = context.Request.Query["access_token"];

                        return Task.FromResult(0);
                    }
                };
            });

            app.UseSignalR<SimpleConnection>("/signalr");

            // Add a new middleware issuing access tokens.
            app.UseOpenIdConnectServer(options => {
                options.Provider = new AuthenticationProvider();

                // Enable the token endpoint.
                options.TokenEndpointPath = "/connect/token";
                options.AllowInsecureHttp = true;
            });
        }
    }
}
