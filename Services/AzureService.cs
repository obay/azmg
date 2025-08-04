using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.ManagementGroups.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace AzMg.Services;

public class AzureService : IAzureService
{
    private readonly ILogger<AzureService> _logger;

    public AzureService(ILogger<AzureService> logger)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, ManagementGroupResource>> GetManagementGroupsAsync(ArmClient armClient)
    {
        ArgumentNullException.ThrowIfNull(armClient);
        
        var managementGroups = new Dictionary<string, ManagementGroupResource>();
        var mgCollection = armClient.GetManagementGroups();
        
        await foreach (var mg in mgCollection.GetAllAsync())
        {
            _logger.LogDebug("Processing management group: {Name}", mg.Data.Name);
            
            // Get the full management group details with expand parameter
            var fullMg = await armClient.GetManagementGroupResource(mg.Id).GetAsync(expand: ManagementGroupExpandType.Children);
            managementGroups[mg.Data.Name] = fullMg.Value;
        }
        
        return managementGroups;
    }

    public async Task<Dictionary<string, SubscriptionResource>> GetSubscriptionsAsync(ArmClient armClient)
    {
        ArgumentNullException.ThrowIfNull(armClient);
        
        var subscriptions = new Dictionary<string, SubscriptionResource>();
        var subCollection = armClient.GetSubscriptions();
        
        await foreach (var sub in subCollection.GetAllAsync())
        {
            _logger.LogDebug("Processing subscription: {Name}", sub.Data.DisplayName);
            subscriptions[sub.Data.SubscriptionId] = sub;
        }
        
        return subscriptions;
    }

    public async Task<Dictionary<string, List<string>>> BuildManagementGroupHierarchyAsync(
        Dictionary<string, ManagementGroupResource> managementGroups)
    {
        var mgHierarchy = new Dictionary<string, List<string>>();
        
        foreach (var mg in managementGroups.Values)
        {
            if (mg.Data.Details?.Parent?.Id != null)
            {
                string parentId = mg.Data.Details.Parent.Id.Split('/').Last();
                if (!mgHierarchy.ContainsKey(parentId))
                    mgHierarchy[parentId] = new List<string>();
                mgHierarchy[parentId].Add(mg.Data.Name);
            }
        }
        
        return await Task.FromResult(mgHierarchy);
    }

    public async Task<Dictionary<string, List<string>>> MapSubscriptionsToManagementGroupsAsync(
        Dictionary<string, ManagementGroupResource> managementGroups,
        Dictionary<string, SubscriptionResource> subscriptions,
        Dictionary<string, List<string>> mgHierarchy)
    {
        var mgToDirectSubs = new Dictionary<string, List<string>>();
        foreach (var mg in managementGroups.Values)
        {
            mgToDirectSubs[mg.Data.Name] = new List<string>();
        }
        
        // Build a complete hierarchy map including all descendants
        var mgDescendants = new Dictionary<string, HashSet<string>>();
        foreach (var mg in managementGroups.Values)
        {
            mgDescendants[mg.Data.Name] = new HashSet<string>();
            await foreach (var descendant in mg.GetDescendantsAsync())
            {
                if (descendant.ResourceType.ToString().ToLower().Contains("subscription"))
                {
                    mgDescendants[mg.Data.Name].Add(descendant.Name);
                }
            }
        }
        
        // For each subscription, find its immediate parent
        foreach (var sub in subscriptions.Values)
        {
            string? immediateParent = null;
            
            // Find all management groups that contain this subscription
            var containingMgs = new List<string>();
            foreach (var kvp in mgDescendants)
            {
                if (kvp.Value.Contains(sub.Data.SubscriptionId))
                {
                    containingMgs.Add(kvp.Key);
                }
            }
            
            // Find the most specific (deepest) management group
            if (containingMgs.Any())
            {
                immediateParent = containingMgs[0];
                
                // Check which MG is the most specific by checking parent relationships
                foreach (var mg in containingMgs)
                {
                    bool isMoreSpecific = true;
                    foreach (var otherMg in containingMgs.Where(m => m != mg))
                    {
                        // Check if mg is a descendant of otherMg
                        if (IsDescendantOf(mg, otherMg, mgHierarchy, managementGroups))
                        {
                            // mg is more specific than otherMg, keep checking
                        }
                        else if (IsDescendantOf(otherMg, mg, mgHierarchy, managementGroups))
                        {
                            // otherMg is more specific than mg
                            isMoreSpecific = false;
                            break;
                        }
                    }
                    if (isMoreSpecific)
                    {
                        immediateParent = mg;
                    }
                }
            }
            
            if (immediateParent != null)
            {
                mgToDirectSubs[immediateParent].Add(sub.Data.SubscriptionId);
            }
        }

        return mgToDirectSubs;
    }

    public List<string> GetRootManagementGroups(Dictionary<string, ManagementGroupResource> managementGroups)
    {
        var rootMgs = new List<string>();
        
        foreach (var mg in managementGroups.Values)
        {
            if (mg.Data.Details?.Parent?.Id == null)
            {
                rootMgs.Add(mg.Data.Name);
            }
        }
        
        return rootMgs;
    }

    public TokenCredential GetAzureCredential(string authMethod, string? tenantId, string? clientId, string? clientSecret)
    {
        _logger.LogDebug("Using authentication method: {AuthMethod}", authMethod);

        return authMethod.ToLowerInvariant() switch
        {
            "serviceprincipal" => GetServicePrincipalCredential(tenantId, clientId, clientSecret),
            "cli" => new AzureCliCredential(),
            "managedidentity" => new ManagedIdentityCredential(clientId),
            "interactive" => new InteractiveBrowserCredential(),
            _ => new DefaultAzureCredential()
        };
    }

    private ClientSecretCredential GetServicePrincipalCredential(string? tenantId, string? clientId, string? clientSecret)
    {
        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException(
                "Service principal authentication requires TenantId, ClientId, and ClientSecret. " +
                "Set these via configuration file, environment variables (AZMG_AzureAuth__TenantId, etc.), or command line arguments.");
        }

        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    private static bool IsDescendantOf(string childMg, string parentMg, Dictionary<string, List<string>> mgHierarchy, Dictionary<string, ManagementGroupResource> managementGroups)
    {
        // Check if childMg is a descendant of parentMg
        if (!mgHierarchy.ContainsKey(parentMg))
            return false;
            
        if (mgHierarchy[parentMg].Contains(childMg))
            return true;
            
        // Check recursively
        foreach (var directChild in mgHierarchy[parentMg])
        {
            if (IsDescendantOf(childMg, directChild, mgHierarchy, managementGroups))
                return true;
        }
        
        return false;
    }
}