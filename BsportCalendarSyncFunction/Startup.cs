﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BsportCalendarSyncFunction.Startup))]
namespace BsportCalendarSyncFunction
{
    using Azure.Core;
    using Azure.Identity;
    using BSportCalendarSyncCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.IO;

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddScoped((serviceProvider) =>
            {
                var config = serviceProvider.GetService<IConfiguration>();
                AppConfiguration appConfig = new();
                config
                    .GetRequiredSection("BSportCalendarSyncSettings")
                    .Bind(appConfig);
                return appConfig;
            });
            builder.Services.AddTransient<TokenCredential>(serviceCollection =>
            {
                var hostingEnvironment = serviceCollection.GetService<IHostEnvironment>();
                var config = serviceCollection.GetService<AppConfiguration>();
                if (hostingEnvironment.EnvironmentName == "Development")
                {
                    return new ClientSecretCredential(config.TenantId, config.AppId, config.AppSecret);
                }
                return new DefaultAzureCredential();
            });
            builder.Services.AddTransient<SyncConfigStorage>();
            builder.Services.AddTransient<CoreEngine>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);
            builder.ConfigurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}
