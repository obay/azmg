using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;

namespace AzMg.Services;

public interface IOutputService
{
    Task OutputAsync(
        Dictionary<string, ManagementGroupResource> managementGroups,
        Dictionary<string, List<string>> mgHierarchy,
        Dictionary<string, SubscriptionResource> subscriptions,
        Dictionary<string, List<string>> mgToDirectSubs,
        List<string> rootMgs,
        string outputFormat,
        bool colorEnabled);
}