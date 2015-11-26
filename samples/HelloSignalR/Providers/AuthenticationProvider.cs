using AspNet.Security.OpenIdConnect.Server;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using AspNet.Security.OpenIdConnect.Extensions;

namespace HelloSignalR.Providers {
    public class AuthenticationProvider : OpenIdConnectServerProvider {
        public override Task ValidateClientAuthentication(ValidateClientAuthenticationContext context) {
            if (context.ClientId == "AspNetContribSample") {
                // Context is marked as Skipped instead of Validated because the client is not trusted
                context.Skipped();
            } else {
                context.Rejected();
            }
            return Task.FromResult(0);
        }
        public override Task GrantResourceOwnerCredentials(GrantResourceOwnerCredentialsContext context) {
            var user = new { Id = "users-123", UserName = "AspNet", Password = "contrib" };
            if (context.UserName != user.UserName || context.Password != user.Password) {
                context.Rejected("Invalid username or password.");
                return Task.FromResult(0);
            }

            var identity = new ClaimsIdentity(OpenIdConnectDefaults.AuthenticationScheme);
            identity.AddClaim(ClaimTypes.NameIdentifier, user.Id, "id_token token");
            identity.AddClaim(ClaimTypes.Name, user.UserName, "id_token token");

            var principal = new ClaimsPrincipal(identity);
            context.Validated(principal);

            return Task.FromResult(0);
        }
    }
}
