using System;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Backend.Models;

namespace Backend.Providers {
    public sealed class AuthorizationProvider : OpenIdConnectServerProvider {
        public override Task MatchEndpoint(MatchEndpointContext context) {
            // Note: by default, OpenIdConnectServerHandler only handles authorization requests made to the authorization endpoint.
            // This context handler uses a more relaxed policy that allows extracting authorization requests received at
            // /connect/authorize/accept and /connect/authorize/deny (see AuthorizationController.cs for more information).
            if (context.Options.AuthorizationEndpointPath.HasValue &&
                context.Request.Path.StartsWithSegments(context.Options.AuthorizationEndpointPath)) {
                context.MatchesAuthorizationEndpoint();
            }

            return Task.FromResult(0);
        }

        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context) {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow()) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this authorization server");

                return;
            }

            // Note: to support custom response modes, the OpenID Connect server middleware doesn't
            // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
            // To ensure invalid modes are rejected early enough, a check is made here.
            if (!string.IsNullOrEmpty(context.Request.ResponseMode) && !context.Request.IsFormPostResponseMode() &&
                                                                       !context.Request.IsFragmentResponseMode() &&
                                                                       !context.Request.IsQueryResponseMode()) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified response_mode is unsupported.");

                return;
            }

            var database = context.HttpContext.RequestServices.GetRequiredService<ApplicationContext>();

            // Retrieve the application details corresponding to the requested client_id.
            var application = await (from entity in database.Applications
                                     where entity.ApplicationID == context.ClientId
                                     select entity).SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (application == null) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Application not found in the database: ensure that your client_id is correct");

                return;
            }

            if (!string.IsNullOrEmpty(context.RedirectUri) &&
                !string.Equals(context.RedirectUri, application.RedirectUri, StringComparison.Ordinal)) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid redirect_uri");

                return;
            }

            context.Validate(application.RedirectUri);
        }

        public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports authorization code, refresh token, client credentials
            // and resource owner password credentials grant types but this authorization provider uses a safer policy
            // rejecting the last two ones. You may consider relaxing it to support the ROPC or client credentials grant types.
            if (!context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only authorization code and refresh token grant types " +
                                 "are accepted by this authorization server");

                return Task.FromResult(0);
            }

            // Note: we use a relaxed policy here as the client credentials cannot be safely stored in the Cordova Javascript application.
            // In this case, we call context.Skip() to inform the server middleware the client is not trusted.
            context.Skip();
            return Task.FromResult(0);            
        }

        public override async Task ValidateLogoutRequest(ValidateLogoutRequestContext context) {
            var database = context.HttpContext.RequestServices.GetRequiredService<ApplicationContext>();

            // Skip validation if the post_logout_redirect_uri parameter was missing.
            if (string.IsNullOrEmpty(context.PostLogoutRedirectUri)) {
                context.Skip();

                return;
            }

            // When provided, post_logout_redirect_uri must exactly match the address registered by the client application.
            if (!await database.Applications.AnyAsync(application => application.LogoutRedirectUri == context.PostLogoutRedirectUri)) {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid post_logout_redirect_uri");

                return;
            }

            context.Validate();
        }
    }
}