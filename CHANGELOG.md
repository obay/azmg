# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2024-01-XX

### Added
- Initial release of azmg
- Tree view visualization of Azure Management Groups and Subscriptions
- JSON output format for programmatic consumption
- CSV output format for data analysis
- Multiple authentication methods:
  - Azure CLI (default)
  - Service Principal
  - Managed Identity
  - Interactive Browser
- Configuration support via:
  - Configuration files (appsettings.json)
  - Environment variables
  - Command-line arguments
- Colored output for better readability
- Verbose logging mode
- Cross-platform support (Windows, macOS, Linux)
- Published as .NET global tool
- Scoop package for Windows
- Homebrew formula for macOS/Linux

### Security
- No hardcoded credentials
- Support for Azure Key Vault integration
- Secure credential handling

[Unreleased]: https://github.com/obay/azmg/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/obay/azmg/releases/tag/v1.0.0