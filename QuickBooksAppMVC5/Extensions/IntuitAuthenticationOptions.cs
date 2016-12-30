using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Owin.Security.Providers.OpenIDBase;
using System;
using System.Net.Http;

namespace QuickBooksAppMVC5.Extensions
{
    public sealed class IntuitAuthenticationOptions : OpenIDAuthenticationOptions
    {
        public const string FirstName = "intuit.firstname";
        public const string LastName = "intuit.lastname";
        public const string Email = "intuit.email";

        public IntuitAuthenticationOptions()
        {
            ProviderDiscoveryUri = "https://openid.intuit.com/openid/xrds";
            Caption = "Intuit";
            AuthenticationType = "Intuit";
            CallbackPath = new PathString("/signin-intuit");
        }
    }

    public static class IntuitAuthenticationExtensions
    {
        public static IAppBuilder UseIntuitAuthentication(this IAppBuilder app, IntuitAuthenticationOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");
            return app.Use(typeof(IntuitAuthenticationMiddleware), app, options);
        }

        public static IAppBuilder UseIntuitAuthentication(this IAppBuilder app)
        {
            return UseIntuitAuthentication(app, new IntuitAuthenticationOptions());
        }
    }

    internal sealed class IntuitAuthenticationHandler : OpenIDAuthenticationHandlerBase<IntuitAuthenticationOptions>
    {
        public IntuitAuthenticationHandler(HttpClient httpClient, ILogger logger)
            : base(httpClient, logger)
        {
        }
    }

    public sealed class IntuitAuthenticationMiddleware : OpenIDAuthenticationMiddlewareBase<IntuitAuthenticationOptions>
    {
        public IntuitAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, IntuitAuthenticationOptions options)
           : base(next, app, options)
        { }

        protected override AuthenticationHandler<IntuitAuthenticationOptions> CreateSpecificHandler()
        {
            return new IntuitAuthenticationHandler(HTTPClient, Logger);
        }
    }
}
