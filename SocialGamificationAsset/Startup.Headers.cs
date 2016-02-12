﻿using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace SocialGamificationAsset
{
    public partial class Startup
    {
        private static void ConfigureHeadersOverride(IApplicationBuilder application)
        {
            application.UseMiddleware<XHttpHeaderOverride>();
        }
    }

    public class XHttpHeaderOverride
    {
        private readonly RequestDelegate _next;

        public XHttpHeaderOverride(RequestDelegate next)
        {
            _next = next;
        }

        /// <exception cref="Exception">
        ///     A <see langword="delegate" /> callback throws an exception.
        /// </exception>
        public Task Invoke(HttpContext httpContext)
        {
            var headerValue = httpContext.Request.Headers["X-HTTP-Method-Override"];
            var queryValue = httpContext.Request.Query["X-HTTP-Method-Override"];

            if (!string.IsNullOrEmpty(headerValue))
            {
                httpContext.Request.Method = headerValue;
            }
            else if (!string.IsNullOrEmpty(queryValue))
            {
                httpContext.Request.Method = queryValue;
            }

            return _next.Invoke(httpContext);
        }
    }
}