using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;

namespace AzMg.Tests;

public class CommandLineTests
{
    [Fact]
    public void CommandLine_DefaultValues_AreCorrect()
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse("");

        // Assert
        parseResult.GetValueForOption<string>("--output").Should().Be("tree");
        parseResult.GetValueForOption<string>("--auth-method").Should().Be("cli");
        parseResult.GetValueForOption<bool>("--no-color").Should().BeFalse();
        parseResult.GetValueForOption<bool>("--verbose").Should().BeFalse();
    }

    [Theory]
    [InlineData("--output json", "json")]
    [InlineData("-o csv", "csv")]
    [InlineData("--output tree", "tree")]
    public void CommandLine_OutputOption_ParsesCorrectly(string args, string expected)
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse(args);

        // Assert
        parseResult.GetValueForOption<string>("--output").Should().Be(expected);
    }

    [Theory]
    [InlineData("--auth-method serviceprincipal", "serviceprincipal")]
    [InlineData("--auth-method cli", "cli")]
    [InlineData("--auth-method managedidentity", "managedidentity")]
    [InlineData("--auth-method interactive", "interactive")]
    public void CommandLine_AuthMethodOption_ParsesCorrectly(string args, string expected)
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse(args);

        // Assert
        parseResult.GetValueForOption<string>("--auth-method").Should().Be(expected);
    }

    [Fact]
    public void CommandLine_ServicePrincipalOptions_ParseCorrectly()
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var args = "--tenant test-tenant --client-id test-client --client-secret test-secret";
        var parseResult = rootCommand.Parse(args);

        // Assert
        parseResult.GetValueForOption<string>("--tenant").Should().Be("test-tenant");
        parseResult.GetValueForOption<string>("--client-id").Should().Be("test-client");
        parseResult.GetValueForOption<string>("--client-secret").Should().Be("test-secret");
    }

    [Theory]
    [InlineData("--no-color", true)]
    [InlineData("", false)]
    public void CommandLine_NoColorOption_ParsesCorrectly(string args, bool expected)
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse(args);

        // Assert
        parseResult.GetValueForOption<bool>("--no-color").Should().Be(expected);
    }

    [Theory]
    [InlineData("--verbose", true)]
    [InlineData("-v", true)]
    [InlineData("", false)]
    public void CommandLine_VerboseOption_ParsesCorrectly(string args, bool expected)
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse(args);

        // Assert
        parseResult.GetValueForOption<bool>("--verbose").Should().Be(expected);
    }

    [Fact]
    public void CommandLine_ConfigOption_ParsesCorrectly()
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse("--config /path/to/config.json");

        // Assert
        parseResult.GetValueForOption<string>("--config").Should().Be("/path/to/config.json");
    }

    [Fact]
    public void CommandLine_ShortAliases_Work()
    {
        // Arrange
        var rootCommand = CreateRootCommand();
        var parseResult = rootCommand.Parse("-o json -c config.json -t tenant-id -v");

        // Assert
        parseResult.GetValueForOption<string>("--output").Should().Be("json");
        parseResult.GetValueForOption<string>("--config").Should().Be("config.json");
        parseResult.GetValueForOption<string>("--tenant").Should().Be("tenant-id");
        parseResult.GetValueForOption<bool>("--verbose").Should().BeTrue();
    }

    private static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Azure Management Groups and Subscriptions hierarchy visualization tool");

        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output format (tree, json, csv)",
            getDefaultValue: () => "tree");

        var configOption = new Option<string?>(
            aliases: new[] { "--config", "-c" },
            description: "Path to configuration file");

        var tenantOption = new Option<string?>(
            aliases: new[] { "--tenant", "-t" },
            description: "Azure AD tenant ID");

        var clientIdOption = new Option<string?>(
            aliases: new[] { "--client-id" },
            description: "Service principal client ID");

        var clientSecretOption = new Option<string?>(
            aliases: new[] { "--client-secret" },
            description: "Service principal client secret");

        var authMethodOption = new Option<string>(
            aliases: new[] { "--auth-method" },
            description: "Authentication method (serviceprincipal, cli, managedidentity, interactive)",
            getDefaultValue: () => "cli");

        var noColorOption = new Option<bool>(
            aliases: new[] { "--no-color" },
            description: "Disable colored output");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose logging");

        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(tenantOption);
        rootCommand.AddOption(clientIdOption);
        rootCommand.AddOption(clientSecretOption);
        rootCommand.AddOption(authMethodOption);
        rootCommand.AddOption(noColorOption);
        rootCommand.AddOption(verboseOption);

        return rootCommand;
    }
}

internal static class ParseResultExtensions
{
    public static T? GetValueForOption<T>(this ParseResult parseResult, string optionName)
    {
        var option = parseResult.CommandResult.Command.Options
            .FirstOrDefault(o => o.Name == optionName || o.Aliases.Contains(optionName));

        return option != null ? parseResult.GetValueForOption((Option<T>)option) : default;
    }
}