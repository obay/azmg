using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzMg.Services
{
    public class OutputService
    {
        public async Task OutputAsync(HierarchyData hierarchyData, string outputFormat, bool colorEnabled)
        {
            switch (outputFormat.ToLowerInvariant())
            {
                case "json":
                    await OutputAsJsonAsync(hierarchyData);
                    break;
                case "csv":
                    await OutputAsCsvAsync(hierarchyData);
                    break;
                case "tree":
                default:
                    await OutputAsTreeAsync(hierarchyData, colorEnabled);
                    break;
            }
        }

        private async Task OutputAsTreeAsync(HierarchyData hierarchyData, bool colorEnabled)
        {
            Console.WriteLine("=== Azure Management Groups and Subscriptions Tree ===\n");

            // Start with root management groups
            foreach (var rootMg in hierarchyData.RootManagementGroups)
            {
                await PrintManagementGroupTreeAsync(
                    rootMg,
                    hierarchyData.ManagementGroups,
                    hierarchyData.Hierarchy,
                    hierarchyData.Subscriptions,
                    hierarchyData.ManagementGroupToDirectSubscriptions,
                    0,
                    colorEnabled);
            }
        }

        private async Task PrintManagementGroupTreeAsync(
            string mgName,
            Dictionary<string, ManagementGroupResource> managementGroups,
            Dictionary<string, List<string>> mgHierarchy,
            Dictionary<string, SubscriptionResource> subscriptions,
            Dictionary<string, List<string>> mgToDirectSubs,
            int indent,
            bool colorEnabled)
        {
            var mg = managementGroups[mgName];
            string prefix = new string(' ', indent * 2) + (indent == 0 ? "" : "├─ ");

            if (colorEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            Console.Write($"{prefix}📁 ");
            if (colorEnabled)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine($"{mg.Data.DisplayName} ({mg.Data.Name})");
            if (colorEnabled)
            {
                Console.ResetColor();
            }

            // Print direct subscriptions (sorted alphabetically by display name)
            if (mgToDirectSubs.ContainsKey(mgName) && mgToDirectSubs[mgName].Any())
            {
                var sortedSubs = mgToDirectSubs[mgName]
                    .Select(subId => subscriptions[subId])
                    .OrderBy(sub => sub.Data.DisplayName)
                    .ToList();

                foreach (var sub in sortedSubs)
                {
                    string subPrefix = new string(' ', (indent + 1) * 2) + "├─ ";
                    if (colorEnabled)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write($"{subPrefix}📄 ");
                    if (colorEnabled)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.WriteLine($"{sub.Data.DisplayName} ({sub.Data.SubscriptionId})");
                    if (colorEnabled)
                    {
                        Console.ResetColor();
                    }
                }
            }

            // Print child management groups (sorted alphabetically by display name)
            if (mgHierarchy.ContainsKey(mgName))
            {
                var sortedChildren = mgHierarchy[mgName]
                    .OrderBy(childName => managementGroups[childName].Data.DisplayName)
                    .ToList();

                foreach (var childMg in sortedChildren)
                {
                    await PrintManagementGroupTreeAsync(childMg, managementGroups, mgHierarchy, subscriptions, mgToDirectSubs, indent + 1, colorEnabled);
                }
            }
        }

        private Task OutputAsJsonAsync(HierarchyData hierarchyData)
        {
            var result = new
            {
                ManagementGroups = hierarchyData.ManagementGroups.Values.Select(mg => new
                {
                    Id = mg.Data.Id,
                    Name = mg.Data.Name,
                    DisplayName = mg.Data.DisplayName,
                    ParentId = mg.Data.Details?.Parent?.Id,
                    Children = hierarchyData.Hierarchy.ContainsKey(mg.Data.Name) ? hierarchyData.Hierarchy[mg.Data.Name] : new List<string>(),
                    DirectSubscriptions = hierarchyData.ManagementGroupToDirectSubscriptions[mg.Data.Name]
                }),
                Subscriptions = hierarchyData.Subscriptions.Values.Select(sub => new
                {
                    Id = sub.Data.Id,
                    SubscriptionId = sub.Data.SubscriptionId,
                    DisplayName = sub.Data.DisplayName,
                    State = sub.Data.State?.ToString()
                })
            };

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine(json);
            return Task.CompletedTask;
        }

        private Task OutputAsCsvAsync(HierarchyData hierarchyData)
        {
            Console.WriteLine("Type,Id,Name,DisplayName,ParentId,State");

            // Output management groups
            foreach (var mg in hierarchyData.ManagementGroups.Values.OrderBy(m => m.Data.DisplayName))
            {
                var parentId = mg.Data.Details?.Parent?.Id ?? "";
                Console.WriteLine($"ManagementGroup,{mg.Data.Id},{mg.Data.Name},\"{mg.Data.DisplayName}\",{parentId},");
            }

            // Output subscriptions with their parent management group
            foreach (var sub in hierarchyData.Subscriptions.Values.OrderBy(s => s.Data.DisplayName))
            {
                var parentMg = hierarchyData.ManagementGroupToDirectSubscriptions.FirstOrDefault(kvp => kvp.Value.Contains(sub.Data.SubscriptionId)).Key;
                var parentId = parentMg != null ? hierarchyData.ManagementGroups[parentMg].Data.Id : "";
                Console.WriteLine($"Subscription,{sub.Data.Id},{sub.Data.SubscriptionId},\"{sub.Data.DisplayName}\",{parentId},{sub.Data.State}");
            }

            return Task.CompletedTask;
        }
    }
}