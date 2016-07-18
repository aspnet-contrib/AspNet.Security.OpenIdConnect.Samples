﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Client.Controllers {
    public class AuthenticationController : Controller {
        [HttpGet("~/signin")]
        public ActionResult SignIn() {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public ActionResult SignOut() {
            // Instruct the cookies middleware to delete the local cookie created when the user agent
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut("ClientCookie", OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}