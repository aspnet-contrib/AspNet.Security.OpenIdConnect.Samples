﻿using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Backend.Extensions;

namespace Backend.Controllers {
    public class AuthenticationController : Controller {
        [HttpGet("~/signin")]
        public ActionResult SignIn(string returnUrl = null) {
            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application.
            ViewBag.ReturnUrl = returnUrl;

            // Note: in a real world application, you'd probably prefer creating a specific view model.
            return View("SignIn", HttpContext.GetExternalProviders());
        }

        [HttpPost("~/signin")]
        public ActionResult SignIn(string provider, string returnUrl) {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrEmpty(provider)) {
                return HttpBadRequest();
            }

            if (!HttpContext.IsProviderSupported(provider)) {
                return HttpBadRequest();
            }

            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application.
            if (string.IsNullOrEmpty(returnUrl)) {
                return HttpBadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return new ChallengeResult(provider, new AuthenticationProperties {
                RedirectUri = returnUrl
            });
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public async Task SignOut() {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).

            await HttpContext.Authentication.SignOutAsync("ServerCookie");
        }
    }
}