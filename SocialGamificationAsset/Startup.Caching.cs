﻿using Microsoft.Extensions.DependencyInjection;

namespace SocialGamificationAsset
{
    public partial class Startup
    {
        /// <summary>
        ///     <para>
        ///         <para>
        ///             Configures caching for the application. Registers the
        ///             <see cref="IDistrbutedCache" /> and
        ///             <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" />
        ///         </para>
        ///         <para>
        ///             types with the <paramref name="services" /> collection or IoC
        ///             container. The <see cref="IDistrbutedCache" /> is intended to be
        ///             used in cloud hosted scenarios where there is a shared cache, which
        ///             is shared between multiple instances of the application. Use the
        ///             <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" />
        ///         </para>
        ///     </para>
        ///     <para>otherwise.</para>
        /// </summary>
        /// <param name="services">
        ///     The services collection or IoC container.
        /// </param>
        private static void ConfigureCachingServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache, which is very fast but the cache will not
            // be shared between instances of the application. Also adds IMemoryCache.
            services.AddCaching();

            // Uncomment the following line to use the Redis implementation of IDistributedCache. This will override
            // any previously registered IDistributedCache service.
            // Redis is a very fast cache provider and the recommended distributed cache provider.
            // services.AddTransient<IDistributedCache, RedisCache>();

            // Uncomment the following line to use the Microsoft SQL Server implementation of IDistributedCache.
            // Note that this would require setting up the session state database.
            // Redis is the preferred cache implementation but you can use SQL Server if you don't have an alternative.
            // services.AddSqlServerCache(
            //     x =>
            //     {
            //         x.ConnectionString = "Server=.;Database=ASPNET5SessionState;Trusted_Connection=True;";
            //         x.SchemaName = "dbo";
            //         x.TableName = "Sessions";
            //     });
        }
    }
}