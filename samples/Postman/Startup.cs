using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Postman.Providers;

namespace Postman
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // To test this sample with Postman, use the following settings:
            //
            // * Authorization URL: http://localhost:6500/connect/authorize
            // * Access token URL: http://localhost:6500/connect/token
            // * Client ID: postman
            // * Client secret: [blank] (not used with public clients)
            // * Scope: openid email profile roles
            // * Grant type: authorization code
            // * Request access token locally: yes

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseOAuthValidation();

            app.UseOpenIdConnectServer(options =>
            {
                options.Provider = new AuthorizationProvider();

                // Enable the authorization and token endpoints.
                options.AuthorizationEndpointPath = "/connect/authorize";
                options.TokenEndpointPath = "/connect/token";
                options.AllowInsecureHttp = true;

                // Note: to override the default access token format and use JWT, assign AccessTokenHandler:
                //
                // options.AccessTokenHandler = new JwtSecurityTokenHandler
                // {
                //     InboundClaimTypeMap = new Dictionary<string, string>(),
                //     OutboundClaimTypeMap = new Dictionary<string, string>()
                // };
                //
                // Note: when using JWT as the access token format, you have to register a signing key.
                //
                // You can register a new ephemeral key, that is discarded when the application shuts down.
                // Tokens signed using this key are automatically invalidated and thus this method
                // should only be used during development:
                //
                // options.SigningCredentials.AddEphemeralKey();
                //
                // On production, using a X.509 certificate stored in the machine store is recommended.
                // You can generate a self-signed certificate using Pluralsight's self-cert utility:
                // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
                //
                // options.SigningCredentials.AddCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
            });

            app.UseMvc();

            app.UseWelcomePage();
        }
    }
}