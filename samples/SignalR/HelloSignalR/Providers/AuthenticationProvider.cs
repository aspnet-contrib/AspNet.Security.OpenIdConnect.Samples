using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;

namespace HelloSignalR.Providers {
    public class AuthenticationProvider : OpenIdConnectServerProvider {
        public override Task ValidateTokenRequest(ValidateTokenRequestContext context) {
            // Reject token requests that don't use grant_type=password or grant_type=refresh_token.
            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType()) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only grant_type=password and refresh_token " +
                                 "requests are accepted by this server.");

                return Task.FromResult(0);
            }

            if (string.Equals(context.ClientId, "AspNetContribSample", StringComparison.Ordinal)) {
                // Note: the context is marked as skipped instead of validated because the client
                // is not trusted (JavaScript applications cannot keep their credentials secret).
                context.Skip();
            }

            else {
                // If the client_id doesn't correspond to the
                // intended identifier, reject the request.
                context.Reject(OpenIdConnectConstants.Errors.InvalidClient);
            }

            return Task.FromResult(0);
        }

        public override Task HandleTokenRequest(HandleTokenRequestContext context) {
            // Only handle grant_type=password token requests and let the
            // OpenID Connect server middleware handle the other grant types.
            if (context.Request.IsPasswordGrantType()) {
                var user = new { Id = "users-123", UserName = "AspNet", Password = "contrib" };

                if (!string.Equals(context.Request.Username, user.UserName, StringComparison.Ordinal) ||
                    !string.Equals(context.Request.Password, user.Password, StringComparison.Ordinal)) {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid username or password.");

                    return Task.FromResult(0);
                }

                var identity = new ClaimsIdentity(context.Options.AuthenticationScheme);

                identity.AddClaim(ClaimTypes.NameIdentifier, user.Id,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                identity.AddClaim(ClaimTypes.Name, user.UserName,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                context.Validate(new ClaimsPrincipal(identity));
            }

            return Task.FromResult(0);
        }
    }
}
