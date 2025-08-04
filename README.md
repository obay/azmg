# azmg - Azure Management Groups Visualizer

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/obay/azmg/workflows/Build/badge.svg)](https://github.com/obay/azmg/actions)
[![NuGet Version](https://img.shields.io/nuget/v/azmg.svg)](https://www.nuget.org/packages/azmg/)
[![Platform](https://img.shields.io/badge/platform-windows%20%7C%20macos%20%7C%20linux-lightgrey)](https://github.com/obay/azmg)

A command-line tool for visualizing Azure Management Groups and Subscriptions hierarchy in tree, JSON, or CSV format.

## Features

- 🏗️ **Hierarchical Visualization** - Display your Azure organization structure in an easy-to-read tree format
- 🔐 **Multiple Authentication Methods** - Support for Azure CLI, Service Principal, Managed Identity, and Interactive authentication
- 📊 **Multiple Output Formats** - Tree view (with colors), JSON, and CSV
- ⚡ **Fast & Efficient** - Optimized API calls to minimize latency
- 🎨 **Colored Output** - Enhanced readability with color-coded management groups and subscriptions
- 🛠️ **Flexible Configuration** - Configure via files, environment variables, or command-line arguments

## Installation

### As a .NET Global Tool

```bash
dotnet tool install --global azmg
```

### Using Homebrew (macOS/Linux)

```bash
brew tap obay/tools
brew install azmg
```

### Using Scoop (Windows)

```bash
scoop bucket add obay https://github.com/obay/scoop-bucket
scoop install azmg
```

### From Source

```bash
git clone https://github.com/obay/azmg.git
cd azmg
dotnet build
dotnet run
```

## Usage

### Basic Usage

```bash
# Display hierarchy using Azure CLI authentication (default)
azmg

# Display as JSON
azmg --output json

# Display as CSV
azmg --output csv

# Disable colored output
azmg --no-color
```

### Authentication Options

#### Azure CLI (Default)
```bash
# Login to Azure CLI first
az login

# Run azmg
azmg
```

#### Service Principal
```bash
# Via command line
azmg --auth-method serviceprincipal \
     --tenant <tenant-id> \
     --client-id <client-id> \
     --client-secret <client-secret>

# Via environment variables
export AZMG_AzureAuth__TenantId=<tenant-id>
export AZMG_AzureAuth__ClientId=<client-id>
export AZMG_AzureAuth__ClientSecret=<client-secret>
azmg --auth-method serviceprincipal

# Via configuration file
azmg --config myconfig.json --auth-method serviceprincipal
```

#### Managed Identity
```bash
# For system-assigned managed identity
azmg --auth-method managedidentity

# For user-assigned managed identity (set client ID via config)
azmg --auth-method managedidentity --client-id <client-id>
```

#### Interactive Browser
```bash
azmg --auth-method interactive
```

### Configuration

Create an `appsettings.json` file:

```json
{
  "AzureAuth": {
    "AuthMethod": "ServicePrincipal",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "Output": {
    "Format": "Tree",
    "ColorEnabled": true,
    "ShowIds": true
  }
}
```

Or use environment variables:
```bash
export AZMG_AzureAuth__AuthMethod=ServicePrincipal
export AZMG_AzureAuth__TenantId=your-tenant-id
export AZMG_AzureAuth__ClientId=your-client-id
export AZMG_AzureAuth__ClientSecret=your-client-secret
```

### Command-Line Options

```
azmg [options]

Options:
  -o, --output <output>              Output format (tree, json, csv) [default: tree]
  -c, --config <config>              Path to configuration file
  -t, --tenant <tenant>              Azure AD tenant ID
  --client-id <client-id>            Service principal client ID
  --client-secret <client-secret>    Service principal client secret
  --auth-method <auth-method>        Authentication method (serviceprincipal, cli, managedidentity, interactive) [default: cli]
  --no-color                         Disable colored output
  -v, --verbose                      Enable verbose logging
  --version                          Show version information
  -?, -h, --help                     Show help and usage information
```

## Output Examples

### Tree View (Default)
```
=== Azure Management Groups and Subscriptions Tree ===

📁 Contoso (contoso-root)
  ├─ 📁 Platform (platform-mg)
  │   ├─ 📄 Management (00000000-0000-0000-0000-000000000001)
  │   ├─ 📄 Connectivity (00000000-0000-0000-0000-000000000002)
  │   └─ 📄 Identity (00000000-0000-0000-0000-000000000003)
  └─ 📁 Landing Zones (landingzones-mg)
      ├─ 📁 Corp (corp-mg)
      │   ├─ 📄 Production (00000000-0000-0000-0000-000000000004)
      │   └─ 📄 Development (00000000-0000-0000-0000-000000000005)
      └─ 📁 Online (online-mg)
          └─ 📄 E-Commerce (00000000-0000-0000-0000-000000000006)
```

### JSON Output
```json
{
  "ManagementGroups": [
    {
      "Id": "/providers/Microsoft.Management/managementGroups/contoso-root",
      "Name": "contoso-root",
      "DisplayName": "Contoso",
      "ParentId": null,
      "Children": ["platform-mg", "landingzones-mg"],
      "DirectSubscriptions": []
    }
  ],
  "Subscriptions": [
    {
      "Id": "/subscriptions/00000000-0000-0000-0000-000000000001",
      "SubscriptionId": "00000000-0000-0000-0000-000000000001",
      "DisplayName": "Management",
      "State": "Enabled"
    }
  ]
}
```

### CSV Output
```csv
Type,Id,Name,DisplayName,ParentId,State
ManagementGroup,/providers/Microsoft.Management/managementGroups/contoso-root,contoso-root,"Contoso",,
ManagementGroup,/providers/Microsoft.Management/managementGroups/platform-mg,platform-mg,"Platform",/providers/Microsoft.Management/managementGroups/contoso-root,
Subscription,/subscriptions/00000000-0000-0000-0000-000000000001,00000000-0000-0000-0000-000000000001,"Management",/providers/Microsoft.Management/managementGroups/platform-mg,Enabled
```

## Required Permissions

The authenticated principal needs the following permissions:
- `Microsoft.Management/managementGroups/read`
- `Microsoft.Management/managementGroups/descendants/read`
- `Microsoft.Resources/subscriptions/read`

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📖 [Documentation](https://github.com/obay/azmg/wiki)
- 🐛 [Issue Tracker](https://github.com/obay/azmg/issues)
- 💬 [Discussions](https://github.com/obay/azmg/discussions)

## Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- Uses [Azure SDK for .NET](https://github.com/Azure/azure-sdk-for-net)
- Command-line parsing by [System.CommandLine](https://github.com/dotnet/command-line-api)