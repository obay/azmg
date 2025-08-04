using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.ManagementGroups.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzMg.Services
{
    public class HierarchyService
    {
        private readonly ILogger<HierarchyService> _logger;

        public HierarchyService(ILogger<HierarchyService> logger)
        {
            _logger = logger;
        }

        public async Task<HierarchyData> FetchHierarchyAsync(ArmClient armClient)
        {
            _logger.LogInformation("Retrieving Management Groups and Subscriptions...");
            
            var managementGroups = new Dictionary<string, ManagementGroupResource>();
            var mgHierarchy = new Dictionary<string, List<string>>();
            var rootMgs = new List<string>();

            // Fetch all Management Groups
            var mgCollection = armClient.GetManagementGroups();
            await foreach (var mg in mgCollection.GetAllAsync())
            {
                _logger.LogDebug("Processing management group: {Name}", mg.Data.Name);
                
                // Get the full management group details with expand parameter
                var fullMg = await armClient.GetManagementGroupResource(mg.Id).GetAsync(expand: ManagementGroupExpandType.Children);
                managementGroups[mg.Data.Name] = fullMg.Value;
                
                // Build parent-child relationships
                if (fullMg.Value.Data.Details?.Parent?.Id != null)
                {
                    string parentId = fullMg.Value.Data.Details.Parent.Id.Split('/').Last();
                    if (!mgHierarchy.ContainsKey(parentId))
                        mgHierarchy[parentId] = new List<string>();
                    mgHierarchy[parentId].Add(mg.Data.Name);
                }
                else
                {
                    rootMgs.Add(mg.Data.Name);
                }
            }

            // Fetch all Subscriptions
            var subCollection = armClient.GetSubscriptions();
            var subscriptions = new Dictionary<string, SubscriptionResource>();
            await foreach (var sub in subCollection.GetAllAsync())
            {
                _logger.LogDebug("Processing subscription: {Name}", sub.Data.DisplayName);
                subscriptions[sub.Data.SubscriptionId] = sub;
            }

            // Map subscriptions to their parent management groups
            var mgToDirectSubs = await MapSubscriptionsToManagementGroupsAsync(managementGroups, subscriptions, mgHierarchy);

            return new HierarchyData
            {
                ManagementGroups = managementGroups,
                Hierarchy = mgHierarchy,
                Subscriptions = subscriptions,
                ManagementGroupToDirectSubscriptions = mgToDirectSubs,
                RootManagementGroups = rootMgs
            };
        }

        private async Task<Dictionary<string, List<string>>> MapSubscriptionsToManagementGroupsAsync(
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

        private bool IsDescendantOf(string childMg, string parentMg, Dictionary<string, List<string>> mgHierarchy, Dictionary<string, ManagementGroupResource> managementGroups)
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

    public class HierarchyData
    {
        public Dictionary<string, ManagementGroupResource> ManagementGroups { get; set; } = new();
        public Dictionary<string, List<string>> Hierarchy { get; set; } = new();
        public Dictionary<string, SubscriptionResource> Subscriptions { get; set; } = new();
        public Dictionary<string, List<string>> ManagementGroupToDirectSubscriptions { get; set; } = new();
        public List<string> RootManagementGroups { get; set; } = new();
    }
}