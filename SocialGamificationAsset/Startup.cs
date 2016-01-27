﻿using Boilerplate.Web.Mvc;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace SocialGamificationAsset
{
	/// <summary>
	/// The main start-up class for the application.
	/// </summary>
	public partial class Startup
	{
		#region Fields

		/// <summary>
		/// The location the application is running in.
		/// </summary>
		private readonly IApplicationEnvironment applicationEnvironment;

		/// <summary>
		/// Gets or sets the application configuration, where key value pair settings are stored. See
		/// http://docs.asp.net/en/latest/fundamentals/configuration.html
		/// http://weblog.west-wind.com/posts/2015/Jun/03/Strongly-typed-AppSettings-Configuration-in-ASPNET-5
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// The environment the application is running under. This can be Development, Staging or Production by default.
		/// To set the hosting environment on Windows:
		/// 1. On your server, right click 'Computer' or 'My Computer' and click on 'Properties'.
		/// 2. Go to 'Advanced System Settings'.
		/// 3. Click on 'Environment Variables' in the Advanced tab.
		/// 4. Add a new System Variable with the name 'ASPNET_ENV' and a value of Production, Staging or
		/// whatever you want.
		/// See http://docs.asp.net/en/latest/fundamentals/environments.html
		/// </summary>
		private readonly IHostingEnvironment hostingEnvironment;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Startup"/> class.
		/// </summary>
		/// <param name="applicationEnvironment">The location the application is running in.</param>
		/// <param name="hostingEnvironment">The environment the application is running under. This can be Development,
		/// Staging or Production by default.</param>
		public Startup(
			IApplicationEnvironment applicationEnvironment,
			IHostingEnvironment hostingEnvironment)
		{
			this.applicationEnvironment = applicationEnvironment;
			this.hostingEnvironment = hostingEnvironment;
			this.configuration = ConfigureConfiguration(hostingEnvironment);
		}

		#endregion Constructors

		/// <summary>
		/// Entry point for the application.
		/// </summary>
		public static void Main(string[] args) => WebApplication.Run<Startup>(args);

		#region Public Methods

		/// <summary>
		/// Configures the services to add to the ASP.NET MVC 6 Injection of Control (IoC) container. This method gets
		/// called by the ASP.NET runtime. See:
		/// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
		/// </summary>
		/// <param name="services">The services collection or IoC container.</param>
		public void ConfigureServices(IServiceCollection services)
		{
			ConfigureDebuggingServices(services, this.hostingEnvironment);

			ConfigureOptionsServices(services, this.configuration);

			ConfigureApplicationServices(services, this.configuration);

			ConfigureCachingServices(services);

			// Configure MVC routing. We store the route options for use by ConfigureSearchEngineOptimizationFilters.
			RouteOptions routeOptions = null;

			services.ConfigureRouting(
				x =>
				{
					routeOptions = x;

					ConfigureRouting(services, x);
				});

			// Add many MVC services to the services container.
			IMvcBuilder mvcBuilder = services.AddMvc(
				mvcOptions =>
				{
					ConfigureCacheProfiles(mvcOptions.CacheProfiles, this.configuration);

					ConfigureSearchEngineOptimizationFilters(mvcOptions.Filters, routeOptions);

					ConfigureSecurityFilters(this.hostingEnvironment, mvcOptions.Filters);
				});

			ConfigureFormatters(mvcBuilder);

			ConfigureSessionServices(services);

			ConfigureDocumentationGeneratorServices(services, this.applicationEnvironment);
		}

		/// <summary>
		/// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is
		/// called by the ASP.NET runtime.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="loggerfactory">The logger factory.</param>
		public void Configure(IApplicationBuilder application, ILoggerFactory loggerfactory)
		{
			// Give the ASP.NET MVC Boilerplate NuGet package assembly access to the HttpContext, so it can generate
			// absolute URL's and get the current request path.
			application.UseBoilerplate();

			// Add the IIS platform handler to the request pipeline.
			application.UseIISPlatformHandler();

			// Add static files to the request pipeline e.g. hello.html or world.css.
			application.UseStaticFiles();

			ConfigureDebugging(application, this.hostingEnvironment);

			ConfigureLogging(application, this.hostingEnvironment, loggerfactory, this.configuration);

			ConfigureErrorPages(application, this.hostingEnvironment);

			ConfigureHeadersOverride(application);

			ConfigureSecurity(application, this.hostingEnvironment);

			ConfigureSession(application);

			ConfigureCors(application);

			// Add MVC to the request pipeline.
			application.UseMvcWithDefaultRoute();

			// Add a 404 Not Found error page for visiting /this-resource-does-not-exist.
			Configure404NotFoundErrorPage(application, this.hostingEnvironment);

			ConfigureDocumentationGenerator(application);

			// Seed the database with Test values
			ConfigureDatabaseInitialization(application);
		}

		#endregion Public Methods
	}
}
