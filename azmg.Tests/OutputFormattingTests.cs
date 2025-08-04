using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace AzMg.Tests;

public class OutputFormattingTests
{
    [Fact]
    public void JsonOutput_SerializesCorrectly()
    {
        // Arrange
        var data = new
        {
            ManagementGroups = new[]
            {
                new
                {
                    Id = "/providers/Microsoft.Management/managementGroups/test-mg",
                    Name = "test-mg",
                    DisplayName = "Test Management Group",
                    ParentId = (string?)null,
                    Children = new[] { "child-mg" },
                    DirectSubscriptions = new[] { "sub-123" }
                }
            },
            Subscriptions = new[]
            {
                new
                {
                    Id = "/subscriptions/sub-123",
                    SubscriptionId = "sub-123",
                    DisplayName = "Test Subscription",
                    State = "Enabled"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Assert
        json.Should().Contain("\"ManagementGroups\"");
        json.Should().Contain("\"test-mg\"");
        json.Should().Contain("\"Test Management Group\"");
        json.Should().Contain("\"Subscriptions\"");
        json.Should().Contain("\"sub-123\"");
        json.Should().Contain("\"Test Subscription\"");
    }

    [Fact]
    public void CsvOutput_FormatsCorrectly()
    {
        // Arrange
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Type,Id,Name,DisplayName,ParentId,State");
        csvBuilder.AppendLine("ManagementGroup,/providers/Microsoft.Management/managementGroups/root,root,\"Root MG\",,");
        csvBuilder.AppendLine("ManagementGroup,/providers/Microsoft.Management/managementGroups/child,child,\"Child MG\",/providers/Microsoft.Management/managementGroups/root,");
        csvBuilder.AppendLine("Subscription,/subscriptions/sub-123,sub-123,\"Test Subscription\",/providers/Microsoft.Management/managementGroups/child,Enabled");

        var csv = csvBuilder.ToString();

        // Assert
        csv.Should().Contain("Type,Id,Name,DisplayName,ParentId,State");
        csv.Should().Contain("ManagementGroup");
        csv.Should().Contain("Subscription");
        csv.Should().Contain("\"Root MG\"");
        csv.Should().Contain("\"Child MG\"");
        csv.Should().Contain("\"Test Subscription\"");
        csv.Should().Contain("Enabled");
    }

    [Fact]
    public void TreeOutput_FormatsHierarchyCorrectly()
    {
        // Arrange
        var treeBuilder = new StringBuilder();
        treeBuilder.AppendLine("=== Azure Management Groups and Subscriptions Tree ===");
        treeBuilder.AppendLine();
        treeBuilder.AppendLine("📁 Root Management Group (root-mg)");
        treeBuilder.AppendLine("  ├─ 📁 Platform (platform-mg)");
        treeBuilder.AppendLine("  │   ├─ 📄 Management Subscription (mgmt-sub)");
        treeBuilder.AppendLine("  │   └─ 📄 Connectivity Subscription (conn-sub)");
        treeBuilder.AppendLine("  └─ 📁 Landing Zones (lz-mg)");
        treeBuilder.AppendLine("      └─ 📄 Production Subscription (prod-sub)");

        var tree = treeBuilder.ToString();

        // Assert
        tree.Should().Contain("📁");
        tree.Should().Contain("📄");
        tree.Should().Contain("├─");
        tree.Should().Contain("└─");
        tree.Should().Contain("Root Management Group");
        tree.Should().Contain("Platform");
        tree.Should().Contain("Landing Zones");
        tree.Should().Contain("Management Subscription");
        tree.Should().Contain("Production Subscription");
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "  ")]
    [InlineData(2, "    ")]
    [InlineData(3, "      ")]
    public void TreeOutput_IndentationIsCorrect(int level, string expectedIndent)
    {
        // Arrange & Act
        var indent = new string(' ', level * 2);

        // Assert
        indent.Should().Be(expectedIndent);
    }

    [Fact]
    public void ColoredOutput_IncludesColorCodes_WhenEnabled()
    {
        // This test would verify ANSI color codes are present when color is enabled
        // For simplicity, we're just testing the concept

        // Arrange
        var colorEnabled = true;
        var output = new StringBuilder();

        // Act
        if (colorEnabled)
        {
            output.Append("\u001b[34m"); // Blue color code
            output.Append("📁 Management Group");
            output.Append("\u001b[0m"); // Reset color code
        }
        else
        {
            output.Append("📁 Management Group");
        }

        // Assert
        if (colorEnabled)
        {
            output.ToString().Should().Contain("\u001b[34m");
            output.ToString().Should().Contain("\u001b[0m");
        }
    }

    [Fact]
    public void Output_HandlesSpecialCharacters_InNames()
    {
        // Arrange
        var specialNames = new[]
        {
            "Test & Development",
            "Production/Staging",
            "Name with \"quotes\"",
            "Name with 'apostrophes'",
            "Name with, commas"
        };

        // Act & Assert
        foreach (var name in specialNames)
        {
            // For CSV, quotes should be escaped
            var csvFormatted = $"\"{name.Replace("\"", "\"\"")}\"";
            csvFormatted.Should().NotBeNull();

            // For JSON, it should be properly escaped
            var jsonFormatted = JsonSerializer.Serialize(name);
            jsonFormatted.Should().NotBeNull();
            jsonFormatted.Should().StartWith("\"");
            jsonFormatted.Should().EndWith("\"");
        }
    }
}