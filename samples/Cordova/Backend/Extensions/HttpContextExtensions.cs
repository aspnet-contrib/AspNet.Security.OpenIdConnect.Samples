﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace Backend.Extensions {
    public static class HttpContextExtensions {
        public static IEnumerable<AuthenticationDescription> GetExternalProviders(this HttpContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            return from description in context.Authentication.GetAuthenticationSchemes()
                   where !string.IsNullOrEmpty(description.DisplayName)
                   select description;
        }

        public static bool IsProviderSupported(this HttpContext context, string provider) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            return (from description in context.GetExternalProviders()
                    where string.Equals(description.AuthenticationScheme, provider, StringComparison.OrdinalIgnoreCase)
                    select description).Any();
        }
    }
}