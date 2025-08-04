using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AzMg.Services
{
    public class AzureAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAuthenticationService> _logger;

        public AzureAuthenticationService(IConfiguration configuration, ILogger<AzureAuthenticationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public TokenCredential GetAzureCredential()
        {
            var authMethod = _configuration["AzureAuth:AuthMethod"] ?? "cli";
            _logger.LogDebug("Using authentication method: {AuthMethod}", authMethod);

            return authMethod.ToLowerInvariant() switch
            {
                "serviceprincipal" => GetServicePrincipalCredential(),
                "cli" => new AzureCliCredential(),
                "managedidentity" => new ManagedIdentityCredential(),
                "interactive" => new InteractiveBrowserCredential(),
                _ => new DefaultAzureCredential()
            };
        }

        private TokenCredential GetServicePrincipalCredential()
        {
            var tenantId = _configuration["AzureAuth:TenantId"];
            var clientId = _configuration["AzureAuth:ClientId"];
            var clientSecret = _configuration["AzureAuth:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException(
                    "Service principal authentication requires TenantId, ClientId, and ClientSecret. " +
                    "Set these via configuration file, environment variables (AZMG_AzureAuth__TenantId, etc.), or command line arguments.");
            }

            return new ClientSecretCredential(tenantId, clientId, clientSecret);
        }
    }
}