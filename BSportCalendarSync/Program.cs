namespace BSportCalendarSync
{
    using Azure.Core;
    using Azure.Identity;
    using BSportCalendarSyncCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration((configBuilder) =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    AppConfiguration appConfig = new();
                    context.Configuration
                        .GetRequiredSection("BSportCalendarSyncSettings")
                        .Bind(appConfig);
                    services.AddSingleton(appConfig);

                    services.AddTransient<TokenCredential>(
                        _ => new ClientSecretCredential(appConfig.TenantId, appConfig.AppId, appConfig.AppSecret));
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddTransient<SyncConfigStorage>();
                    services.AddTransient<CoreEngine>();
                })
                .Build();

            var engine = host.Services.GetService<CoreEngine>();
            engine.SyncCalendarsForAllUsers();
        }
    }
}