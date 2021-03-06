﻿using Microsoft.AspNet.Builder;

using SocialGamificationAsset.Middlewares;

namespace SocialGamificationAsset
{
    public partial class Startup
    {
        private static void ConfigureHeadersOverride(IApplicationBuilder application)
        {
            application.UseMiddleware<XHttpHeaderOverrideMiddleware>();
        }
    }
}