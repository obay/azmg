using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Azure.ResourceManager;
using AzMg.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzMg
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = CommandLineOptions.CreateRootCommand();

            rootCommand.SetHandler(async (context) =>
            {
                var output = context.ParseResult.GetValueForOption(CommandLineOptions.OutputOption)!;
                var configPath = context.ParseResult.GetValueForOption(CommandLineOptions.ConfigOption);
                var tenant = context.ParseResult.GetValueForOption(CommandLineOptions.TenantOption);
                var clientId = context.ParseResult.GetValueForOption(CommandLineOptions.ClientIdOption);
                var clientSecret = context.ParseResult.GetValueForOption(CommandLineOptions.ClientSecretOption);
                var authMethod = context.ParseResult.GetValueForOption(CommandLineOptions.AuthMethodOption)!;
                var noColor = context.ParseResult.GetValueForOption(CommandLineOptions.NoColorOption);
                var verbose = context.ParseResult.GetValueForOption(CommandLineOptions.VerboseOption);

                context.ExitCode = await RunAsync(output, configPath, tenant, clientId, clientSecret, authMethod, !noColor, verbose);
            });

            return await rootCommand.InvokeAsync(args);
        }

        static async Task<int> RunAsync(
            string outputFormat,
            string? configPath,
            string? tenantId,
            string? clientId,
            string? clientSecret,
            string authMethod,
            bool colorEnabled,
            bool verbose)
        {
            try
            {
                // Setup configuration
                var configuration = BuildConfiguration(configPath, tenantId, clientId, clientSecret, authMethod, outputFormat, colorEnabled);

                // Setup services
                var serviceProvider = ConfigureServices(configuration, verbose);

                // Get services
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                var authService = serviceProvider.GetRequiredService<AzureAuthenticationService>();
                var hierarchyService = serviceProvider.GetRequiredService<HierarchyService>();
                var outputService = serviceProvider.GetRequiredService<OutputService>();

                // Get Azure credentials and authenticate
                logger.LogDebug("Authenticating to Azure...");
                var credential = authService.GetAzureCredential();
                var armClient = new ArmClient(credential);

                // Fetch hierarchy data
                var hierarchyData = await hierarchyService.FetchHierarchyAsync(armClient);

                // Output results
                await outputService.OutputAsync(hierarchyData, outputFormat, colorEnabled);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private static IConfiguration BuildConfiguration(
            string? configPath,
            string? tenantId,
            string? clientId,
            string? clientSecret,
            string authMethod,
            string outputFormat,
            bool colorEnabled)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("AZMG_");

            if (!string.IsNullOrEmpty(configPath))
            {
                builder.AddJsonFile(configPath, optional: false, reloadOnChange: false);
            }

            // Command line arguments override everything
            var cliArgs = new Dictionary<string, string?>();
            if (!string.IsNullOrEmpty(tenantId)) cliArgs["AzureAuth:TenantId"] = tenantId;
            if (!string.IsNullOrEmpty(clientId)) cliArgs["AzureAuth:ClientId"] = clientId;
            if (!string.IsNullOrEmpty(clientSecret)) cliArgs["AzureAuth:ClientSecret"] = clientSecret;
            cliArgs["AzureAuth:AuthMethod"] = authMethod;
            cliArgs["Output:Format"] = outputFormat;
            cliArgs["Output:ColorEnabled"] = colorEnabled.ToString();

            builder.AddInMemoryCollection(cliArgs);
            return builder.Build();
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration, bool verbose)
        {
            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                if (verbose)
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    builder.SetMinimumLevel(LogLevel.Warning);
                }
            });

            // Add services
            services.AddSingleton<AzureAuthenticationService>();
            services.AddSingleton<HierarchyService>();
            services.AddSingleton<OutputService>();

            return services.BuildServiceProvider();
        }
    }
}