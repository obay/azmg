using System.CommandLine;

namespace AzMg
{
    public static class CommandLineOptions
    {
        public static RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand("Azure Management Groups and Subscriptions hierarchy visualization tool");

            rootCommand.AddOption(OutputOption);
            rootCommand.AddOption(ConfigOption);
            rootCommand.AddOption(TenantOption);
            rootCommand.AddOption(ClientIdOption);
            rootCommand.AddOption(ClientSecretOption);
            rootCommand.AddOption(AuthMethodOption);
            rootCommand.AddOption(NoColorOption);
            rootCommand.AddOption(VerboseOption);

            return rootCommand;
        }

        public static Option<string> OutputOption { get; } = new(
            aliases: new[] { "--output", "-o" },
            description: "Output format (tree, json, csv)",
            getDefaultValue: () => "tree");

        public static Option<string?> ConfigOption { get; } = new(
            aliases: new[] { "--config", "-c" },
            description: "Path to configuration file");

        public static Option<string?> TenantOption { get; } = new(
            aliases: new[] { "--tenant", "-t" },
            description: "Azure AD tenant ID");

        public static Option<string?> ClientIdOption { get; } = new(
            aliases: new[] { "--client-id" },
            description: "Service principal client ID");

        public static Option<string?> ClientSecretOption { get; } = new(
            aliases: new[] { "--client-secret" },
            description: "Service principal client secret");

        public static Option<string> AuthMethodOption { get; } = new(
            aliases: new[] { "--auth-method" },
            description: "Authentication method (serviceprincipal, cli, managedidentity, interactive)",
            getDefaultValue: () => "cli");

        public static Option<bool> NoColorOption { get; } = new(
            aliases: new[] { "--no-color" },
            description: "Disable colored output");

        public static Option<bool> VerboseOption { get; } = new(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose logging");
    }
}