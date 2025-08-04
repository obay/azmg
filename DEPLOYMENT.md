# Deployment Guide

This guide explains how to publish azmg to various package managers.

## Prerequisites

- GitHub repository with proper secrets configured:
  - `NUGET_API_KEY` - Your NuGet.org API key
- .NET 8.0 SDK installed locally
- Git and GitHub CLI installed

## Publishing Process

### 1. Prepare Release

1. Update version in `azmg.csproj`
2. Update `CHANGELOG.md` with release notes
3. Commit changes: `git commit -am "Release v1.0.0"`
4. Create and push tag: `git tag v1.0.0 && git push origin v1.0.0`

### 2. Create GitHub Release

```bash
gh release create v1.0.0 \
  --title "Release v1.0.0" \
  --notes "See CHANGELOG.md for details" \
  --draft
```

### 3. Publish to NuGet

The GitHub Actions workflow will automatically:
1. Build and test the project
2. Create NuGet package
3. Publish to NuGet.org
4. Create platform-specific binaries
5. Upload binaries to GitHub release

### 4. Update Package Managers

#### Scoop (Windows)

1. Fork/clone your Scoop bucket repository
2. Update `azmg.json` with new version and SHA256 hash
3. Submit PR to the bucket repository

```bash
# Get SHA256 hash of the Windows binary
certutil -hashfile azmg-win-x64.exe.zip SHA256
```

#### Homebrew (macOS/Linux)

1. Fork/clone your Homebrew tap repository
2. Update formula with new version and SHA256 hashes
3. Submit PR to the tap repository

```bash
# Get SHA256 hash of the macOS/Linux binaries
shasum -a 256 azmg-osx-x64.tar.gz
shasum -a 256 azmg-linux-x64.tar.gz
```

## Manual Publishing

If automatic publishing fails:

### NuGet
```bash
dotnet pack -c Release /p:Version=1.0.0
dotnet nuget push ./bin/Release/azmg.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Creating Binaries
```bash
# Windows
dotnet publish -c Release -r win-x64 \
  /p:PublishSingleFile=true \
  /p:SelfContained=true \
  /p:PublishTrimmed=true \
  /p:Version=1.0.0

# macOS
dotnet publish -c Release -r osx-x64 \
  /p:PublishSingleFile=true \
  /p:SelfContained=true \
  /p:PublishTrimmed=true \
  /p:Version=1.0.0

# Linux
dotnet publish -c Release -r linux-x64 \
  /p:PublishSingleFile=true \
  /p:SelfContained=true \
  /p:PublishTrimmed=true \
  /p:Version=1.0.0
```

## Post-Release

1. Verify NuGet package: https://www.nuget.org/packages/azmg/
2. Test installation methods:
   ```bash
   # .NET tool
   dotnet tool install --global azmg
   
   # Scoop
   scoop install azmg
   
   # Homebrew
   brew install obay/tools/azmg
   ```
3. Update documentation if needed
4. Announce release on social media/blog