﻿using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.SwaggerGen;
using System.Collections.Generic;

namespace SocialGamificationAsset
{
	public partial class Startup
	{
		private static void ConfigureDocumentationGeneratorServices(IServiceCollection services)
		{
			// SWASHBUCKLE SWAGGER API Documentation Generator
			services.AddSwaggerGen();
			services.ConfigureSwaggerDocument(options =>
			{
				options.IgnoreObsoleteActions = true;

				options.OrderActionGroupsBy(new DescendingAlphabeticComparer());

				options.SingleApiVersion(new Info
				{
					Version = "v1",
					Title = "Social Gamification API",
					Description = "This module allows to layer game mechanics affording game-inspired social relations and interactions on top a system to support engagement, collaboration, and learning. Two main forms of social interaction are supported: player-player interactions (such as matches) and group interactions (such as shared team goals or team vs. team competitions).",
					TermsOfService = ""
				});

				/*
				options.SecurityDefinitions.Add("apiKey", new ApiKeyScheme()
				{
					Type = "apiKey",
					Description = "API Key Authentication",
					Name = "apiKey",
					In = "header"
				});
				*/
			});
		}

		private static void ConfigureDocumentationGenerator(IApplicationBuilder application)
		{
			application.UseSwaggerGen();
			application.UseSwaggerUi();
		}
	}

	public class DescendingAlphabeticComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			return string.Compare(y, x, System.StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
