# Contributing to azmg

First off, thank you for considering contributing to azmg! It's people like you that make azmg such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

* **Use a clear and descriptive title** for the issue to identify the problem
* **Describe the exact steps which reproduce the problem** in as many details as possible
* **Provide specific examples to demonstrate the steps**
* **Describe the behavior you observed after following the steps**
* **Explain which behavior you expected to see instead and why**
* **Include screenshots** if applicable
* **Include your environment details** (OS, .NET version, Azure CLI version if using CLI auth)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

* **Use a clear and descriptive title** for the issue to identify the suggestion
* **Provide a step-by-step description of the suggested enhancement**
* **Provide specific examples to demonstrate the steps**
* **Describe the current behavior** and **explain which behavior you expected to see instead**
* **Explain why this enhancement would be useful**

### Pull Requests

* Fill in the required template
* Do not include issue numbers in the PR title
* Follow the coding style used in the project
* Include thoughtfully-worded, well-structured tests
* Document new code
* End all files with a newline

## Development Setup

1. **Prerequisites**
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for testing)
   - An Azure subscription with appropriate permissions

2. **Clone the repository**
   ```bash
   git clone https://github.com/obay/azmg.git
   cd azmg
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Run the application locally**
   ```bash
   dotnet run -- --help
   ```

## Coding Style

* Use the default .NET coding conventions
* Use meaningful variable and method names
* Keep methods small and focused
* Write XML documentation for public APIs
* Use async/await for all Azure API calls
* Handle exceptions appropriately
* Use nullable reference types effectively

### Example Code Style

```csharp
/// <summary>
/// Retrieves all management groups from Azure.
/// </summary>
/// <param name="armClient">The authenticated ARM client.</param>
/// <returns>A dictionary of management groups keyed by name.</returns>
public async Task<Dictionary<string, ManagementGroupResource>> GetManagementGroupsAsync(
    ArmClient armClient)
{
    ArgumentNullException.ThrowIfNull(armClient);
    
    var managementGroups = new Dictionary<string, ManagementGroupResource>();
    
    await foreach (var mg in armClient.GetManagementGroups().GetAllAsync())
    {
        _logger.LogDebug("Processing management group: {Name}", mg.Data.Name);
        managementGroups[mg.Data.Name] = mg;
    }
    
    return managementGroups;
}
```

## Testing

* Write unit tests for new functionality
* Ensure all tests pass before submitting PR
* Include integration tests for Azure interactions (using mocked responses)
* Test with different authentication methods
* Test error scenarios

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

## Commit Messages

* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line

### Examples

```
Add CSV output format support

- Implement CSV formatter for management groups and subscriptions
- Add --output csv command line option
- Update documentation with CSV examples

Fixes #123
```

## Project Structure

```
azmg/
├── Program.cs              # Main entry point and CLI setup
├── Services/              # Business logic services
│   ├── AzureService.cs    # Azure API interactions
│   ├── OutputService.cs   # Output formatting
│   └── AuthService.cs     # Authentication handling
├── Models/                # Data models
├── Tests/                 # Unit and integration tests
├── azmg.csproj           # Project file
└── README.md             # Documentation
```

## Release Process

1. Update version in `azmg.csproj`
2. Update CHANGELOG.md
3. Create a PR with version bump
4. After merge, create a GitHub release
5. GitHub Actions will automatically:
   - Build and test
   - Publish to NuGet
   - Create release artifacts

## Questions?

Feel free to open an issue with your question or reach out to the maintainers directly.

Thank you for contributing! 🎉