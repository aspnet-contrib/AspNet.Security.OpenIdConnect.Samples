using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using HelloSignalR.Connections;
using HelloSignalR.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HelloSignalR
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();

            app.UseStaticFiles();

            // Add a new middleware validating access tokens.
            app.UseOAuthValidation(options =>
            {
                options.Events = new OAuthValidationEvents
                {
                    // Note: for SignalR connections, the default Authorization header does not work,
                    // because the WebSockets JS API doesn't allow setting custom parameters.
                    // To work around this limitation, the access token is retrieved from the query string.
                    OnRetrieveToken = context =>
                    {
                        context.Token = context.Request.Query["access_token"];

                        return Task.FromResult(0);
                    }
                };
            });

            app.UseSignalR<SimpleConnection>("/signalr");

            // Add a new middleware issuing access tokens.
            app.UseOpenIdConnectServer(options =>
            {
                options.Provider = new AuthorizationProvider();

                // Enable the token endpoint.
                options.TokenEndpointPath = "/connect/token";
                options.AllowInsecureHttp = true;

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                options.SigningCredentials.AddEphemeralKey();

                // On production, using a X.509 certificate stored in the machine store is recommended.
                // You can generate a self-signed certificate using Pluralsight's self-cert utility:
                // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
                //
                // options.SigningCredentials.AddCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
            });
        }
    }
}
