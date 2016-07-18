using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Postman.Providers;

namespace Postman {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) {
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

            app.UseOpenIdConnectServer(options => {
                options.Provider = new AuthorizationProvider();

                // Enable the authorization and token endpoints.
                options.AuthorizationEndpointPath = "/connect/authorize";
                options.TokenEndpointPath = "/connect/token";
                options.AllowInsecureHttp = true;
            });

            app.UseMvc();

            app.UseWelcomePage();
        }
    }
}