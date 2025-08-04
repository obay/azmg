# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
This is a .NET 8.0 console application (azmg) that demonstrates Azure Management Groups and Subscriptions operations using the Azure SDK for .NET.

## Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

## Architecture

The application uses Azure SDK libraries to:
1. Authenticate to Azure using service principal credentials
2. Query Azure Management Groups hierarchy
3. List all Azure Subscriptions
4. Map relationships between Management Groups and their associated Subscriptions

### Key Dependencies
- **Azure.Identity**: Handles Azure authentication via ClientSecretCredential
- **Azure.ResourceManager**: Core Azure Resource Manager functionality
- **Azure.ResourceManager.Resources**: Management Groups and Subscriptions operations

### Authentication Flow
The application requires service principal credentials configured in Program.cs:
- `tenantId`: Azure AD tenant ID
- `clientId`: Service principal application ID  
- `clientSecret`: Service principal secret

These must be replaced with actual values before running the application.

### Core Operations in Program.cs
1. **Authentication**: Creates ArmClient using ClientSecretCredential
2. **Management Groups**: Retrieves all management groups via `GetManagementGroups()`
3. **Subscriptions**: Lists all subscriptions via `GetSubscriptions()`
4. **Relationship Mapping**: Associates subscriptions with their parent management groups using the management group hierarchy