using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;

namespace Postman.Providers
{
    public sealed class AuthorizationProvider : OpenIdConnectServerProvider
    {
        public override Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this authorization server.");

                return Task.FromResult(0);
            }

            // Note: to support custom response modes, the OpenID Connect server middleware doesn't
            // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
            // To ensure invalid modes are rejected early enough, a check is made here.
            if (!string.IsNullOrEmpty(context.Request.ResponseMode) && !context.Request.IsFormPostResponseMode() &&
                                                                       !context.Request.IsFragmentResponseMode() &&
                                                                       !context.Request.IsQueryResponseMode())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified response_mode is unsupported.");

                return Task.FromResult(0);
            }

            // Ensure the client_id parameter corresponds to the Postman client.
            if (!string.Equals(context.Request.ClientId, "postman", StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client_id is unknown.");

                return Task.FromResult(0);
            }

            // Ensure the redirect_uri parameter corresponds to the Postman client.
            if (!string.Equals(context.Request.RedirectUri, "https://www.getpostman.com/oauth2/callback", StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified redirect_uri is invalid.");

                return Task.FromResult(0);
            }

            context.Validate();

            return Task.FromResult(0);
        }

        public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            // Reject the token request if it doesn't specify grant_type=authorization_code,
            // grant_type=password or grant_type=refresh_token.
            if (!context.Request.IsAuthorizationCodeGrantType() &&
                !context.Request.IsPasswordGrantType() &&
                !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only grant_type=authorization_code, grant_type=password or " +
                                 "grant_type=refresh_token are accepted by this server.");

                return Task.FromResult(0);
            }

            // Since there's only one application and since it's a public client
            // (i.e a client that cannot keep its credentials private), call Skip()
            // to inform the server the request should be accepted without
            // enforcing client authentication.
            context.Skip();

            return Task.FromResult(0);
        }

        public override Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            // Only handle grant_type=password token requests and let the
            // OpenID Connect server middleware handle the other grant types.
            if (context.Request.IsPasswordGrantType())
            {
                // Using password derivation and a time-constant comparer is STRONGLY recommended.
                if (!string.Equals(context.Request.Username, "Bob", StringComparison.Ordinal) ||
                    !string.Equals(context.Request.Password, "P@ssw0rd", StringComparison.Ordinal))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid user credentials.");

                    return Task.FromResult(0);
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token and/or an access token.
                var identity = new ClaimsIdentity(
                    OpenIdConnectServerDefaults.AuthenticationScheme,
                    OpenIdConnectConstants.Claims.Name,
                    OpenIdConnectConstants.Claims.Role);

                identity.AddClaim(OpenIdConnectConstants.Claims.Subject, Guid.NewGuid().ToString(),
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                identity.AddClaim(OpenIdConnectConstants.Claims.Name, "Bob le Bricoleur",
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties(),
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Set the list of scopes granted to the client application.
                ticket.SetScopes(new[] {
                    /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                    /* email: */ OpenIdConnectConstants.Scopes.Email,
                    /* profile: */ OpenIdConnectConstants.Scopes.Profile,
                    /* offline_access: */ OpenIdConnectConstants.Scopes.OfflineAccess
                }.Intersect(context.Request.GetScopes()));

                context.Validate(ticket);
            }

            return Task.FromResult(0);
        }
    }
}