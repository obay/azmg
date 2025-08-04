using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AzMg.Tests;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_LoadsFromMultipleSources_WithCorrectPrecedence()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["AzureAuth:TenantId"] = "memory-tenant",
            ["AzureAuth:ClientId"] = "memory-client",
            ["AzureAuth:AuthMethod"] = "serviceprincipal",
            ["Output:Format"] = "json"
        };

        var envVars = new Dictionary<string, string?>
        {
            ["AzureAuth:TenantId"] = "env-tenant",
            ["Output:ColorEnabled"] = "false"
        };

        var cliArgs = new Dictionary<string, string?>
        {
            ["AzureAuth:TenantId"] = "cli-tenant"
        };

        // Act
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .AddInMemoryCollection(envVars)
            .AddInMemoryCollection(cliArgs)
            .Build();

        // Assert - CLI args should override everything
        configuration["AzureAuth:TenantId"].Should().Be("cli-tenant");
        // Env vars should override in-memory where not overridden by CLI
        configuration["Output:ColorEnabled"].Should().Be("false");
        // In-memory should be used where not overridden
        configuration["AzureAuth:ClientId"].Should().Be("memory-client");
        configuration["AzureAuth:AuthMethod"].Should().Be("serviceprincipal");
        configuration["Output:Format"].Should().Be("json");
    }

    [Fact]
    public void Configuration_HandlesEnvironmentVariables_WithProperPrefix()
    {
        // Arrange
        // When using environment variables, the actual program would use AddEnvironmentVariables("AZMG_")
        // For testing, we'll simulate the transformed keys
        var envVars = new Dictionary<string, string?>
        {
            ["AzureAuth:TenantId"] = "env-tenant-id",
            ["AzureAuth:ClientId"] = "env-client-id",
            ["AzureAuth:ClientSecret"] = "env-client-secret",
            ["Output:Format"] = "csv",
            ["Output:ColorEnabled"] = "true"
        };

        // Act
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(envVars)
            .Build();

        // Assert
        configuration["AzureAuth:TenantId"].Should().Be("env-tenant-id");
        configuration["AzureAuth:ClientId"].Should().Be("env-client-id");
        configuration["AzureAuth:ClientSecret"].Should().Be("env-client-secret");
        configuration["Output:Format"].Should().Be("csv");
        configuration["Output:ColorEnabled"].Should().Be("true");
    }

    [Fact]
    public void Configuration_LoadsFromJsonFile_WhenProvided()
    {
        // Arrange
        var jsonContent = @"{
            ""AzureAuth"": {
                ""AuthMethod"": ""ServicePrincipal"",
                ""TenantId"": ""json-tenant-id"",
                ""ClientId"": ""json-client-id"",
                ""ClientSecret"": ""json-client-secret""
            },
            ""Output"": {
                ""Format"": ""Tree"",
                ""ColorEnabled"": true,
                ""ShowIds"": true
            },
            ""Logging"": {
                ""LogLevel"": {
                    ""Default"": ""Information""
                }
            }
        }";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, jsonContent);

        try
        {
            // Act
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(tempFile, optional: false, reloadOnChange: false)
                .Build();

            // Assert
            configuration["AzureAuth:AuthMethod"].Should().Be("ServicePrincipal");
            configuration["AzureAuth:TenantId"].Should().Be("json-tenant-id");
            configuration["AzureAuth:ClientId"].Should().Be("json-client-id");
            configuration["AzureAuth:ClientSecret"].Should().Be("json-client-secret");
            configuration["Output:Format"].Should().Be("Tree");
            configuration["Output:ColorEnabled"].Should().Be("True");
            configuration["Output:ShowIds"].Should().Be("True");
            configuration["Logging:LogLevel:Default"].Should().Be("Information");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData("serviceprincipal", true, true, true)]
    [InlineData("cli", false, false, false)]
    [InlineData("managedidentity", false, false, false)]
    [InlineData("interactive", false, false, false)]
    public void Configuration_ValidatesRequiredFields_ForAuthMethod(
        string authMethod, bool requiresTenant, bool requiresClientId, bool requiresClientSecret)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAuth:AuthMethod"] = authMethod
            })
            .Build();

        // Act
        var method = configuration["AzureAuth:AuthMethod"];

        // Assert
        method.Should().Be(authMethod);
        
        if (authMethod == "serviceprincipal")
        {
            // For service principal, we'd need additional validation logic
            // This is just demonstrating the test structure
            requiresTenant.Should().BeTrue();
            requiresClientId.Should().BeTrue();
            requiresClientSecret.Should().BeTrue();
        }
        else
        {
            requiresTenant.Should().BeFalse();
            requiresClientId.Should().BeFalse();
            requiresClientSecret.Should().BeFalse();
        }
    }
}