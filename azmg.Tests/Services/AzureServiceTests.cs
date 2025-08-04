using AzMg.Services;
using Azure.Core;
using Azure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AzMg.Tests.Services;

public class AzureServiceTests
{
    private readonly Mock<ILogger<AzureService>> _loggerMock;
    private readonly AzureService _azureService;

    public AzureServiceTests()
    {
        _loggerMock = new Mock<ILogger<AzureService>>();
        _azureService = new AzureService(_loggerMock.Object);
    }

    [Fact]
    public void GetAzureCredential_WithCliAuth_ReturnsAzureCliCredential()
    {
        // Act
        var credential = _azureService.GetAzureCredential("cli", null, null, null);

        // Assert
        credential.Should().BeOfType<AzureCliCredential>();
    }

    [Fact]
    public void GetAzureCredential_WithManagedIdentity_ReturnsManagedIdentityCredential()
    {
        // Act
        var credential = _azureService.GetAzureCredential("managedidentity", null, null, null);

        // Assert
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void GetAzureCredential_WithInteractive_ReturnsInteractiveBrowserCredential()
    {
        // Act
        var credential = _azureService.GetAzureCredential("interactive", null, null, null);

        // Assert
        credential.Should().BeOfType<InteractiveBrowserCredential>();
    }

    [Fact]
    public void GetAzureCredential_WithServicePrincipal_ValidCredentials_ReturnsClientSecretCredential()
    {
        // Arrange
        var tenantId = "test-tenant-id";
        var clientId = "test-client-id";
        var clientSecret = "test-client-secret";

        // Act
        var credential = _azureService.GetAzureCredential("serviceprincipal", tenantId, clientId, clientSecret);

        // Assert
        credential.Should().BeOfType<ClientSecretCredential>();
    }

    [Theory]
    [InlineData(null, "client-id", "client-secret")]
    [InlineData("tenant-id", null, "client-secret")]
    [InlineData("tenant-id", "client-id", null)]
    [InlineData("", "client-id", "client-secret")]
    [InlineData("tenant-id", "", "client-secret")]
    [InlineData("tenant-id", "client-id", "")]
    public void GetAzureCredential_WithServicePrincipal_MissingCredentials_ThrowsException(
        string? tenantId, string? clientId, string? clientSecret)
    {
        // Act & Assert
        var act = () => _azureService.GetAzureCredential("serviceprincipal", tenantId, clientId, clientSecret);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Service principal authentication requires TenantId, ClientId, and ClientSecret.*");
    }

    [Fact]
    public void GetAzureCredential_WithUnknownAuthMethod_ReturnsDefaultAzureCredential()
    {
        // Act
        var credential = _azureService.GetAzureCredential("unknown", null, null, null);

        // Assert
        credential.Should().BeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public async Task GetManagementGroupsAsync_WithNullArmClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _azureService.GetManagementGroupsAsync(null!));
    }

    [Fact]
    public async Task GetSubscriptionsAsync_WithNullArmClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _azureService.GetSubscriptionsAsync(null!));
    }

    [Fact]
    public async Task BuildManagementGroupHierarchyAsync_ReturnsEmptyHierarchy_WhenNoParentRelationships()
    {
        // Arrange
        var managementGroups = new Dictionary<string, Azure.ResourceManager.ManagementGroups.ManagementGroupResource>();

        // Act
        var result = await _azureService.BuildManagementGroupHierarchyAsync(managementGroups);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRootManagementGroups_ReturnsEmptyList_WhenNoManagementGroups()
    {
        // Arrange
        var managementGroups = new Dictionary<string, Azure.ResourceManager.ManagementGroups.ManagementGroupResource>();

        // Act
        var result = _azureService.GetRootManagementGroups(managementGroups);

        // Assert
        result.Should().BeEmpty();
    }
}