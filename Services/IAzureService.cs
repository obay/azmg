using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;

namespace AzMg.Services;

public interface IAzureService
{
    Task<Dictionary<string, ManagementGroupResource>> GetManagementGroupsAsync(ArmClient armClient);
    Task<Dictionary<string, SubscriptionResource>> GetSubscriptionsAsync(ArmClient armClient);
    Task<Dictionary<string, List<string>>> BuildManagementGroupHierarchyAsync(
        Dictionary<string, ManagementGroupResource> managementGroups);
    Task<Dictionary<string, List<string>>> MapSubscriptionsToManagementGroupsAsync(
        Dictionary<string, ManagementGroupResource> managementGroups,
        Dictionary<string, SubscriptionResource> subscriptions,
        Dictionary<string, List<string>> mgHierarchy);
    List<string> GetRootManagementGroups(Dictionary<string, ManagementGroupResource> managementGroups);
    TokenCredential GetAzureCredential(string authMethod, string? tenantId, string? clientId, string? clientSecret);
}