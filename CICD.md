# CI/CD Pipeline Documentation

This document explains how to use the CI/CD pipeline for the FbaTemplates project.

## Pipeline Overview

This project uses 4 GitHub Actions workflows:

### 1. Build and Test (`ci-cd.yml`)

- **Triggers**: Push to `main`, `develop` branches, PR to `main` branch
- **Features**:
  - Code build and test
  - NuGet package creation (artifacts only)
  - Template installation testing
  - Upload packages as GitHub Actions artifacts

### 2. Prerelease (`prerelease.yml`)

- **Triggers**: Tag creation in `preview/v*.*.*` format
- **Features**:
  - Prerelease NuGet package build
  - GitHub Prerelease creation
  - Package deployment to NuGet.org as prerelease
  - All template validation

### 3. Release (`release.yml`)

- **Triggers**: Tag creation in `release/v*.*.*` format or manual execution
- **Features**:
  - Production NuGet package build
  - GitHub Release creation
  - Package deployment to NuGet.org as stable release
  - All template validation

### 4. Code Quality (`code-quality.yml`)

- **Triggers**: Push to `main`, `develop` branches, PR to `main` branch
- **Features**:
  - Code formatting check
  - Security scan (CodeQL)
  - Template structure validation
  - Dependency security check

## Setup Requirements

### GitHub Secrets Configuration

Set the following secrets in Repository Settings > Secrets and variables > Actions:

1. **NUGET_API_KEY**: NuGet.org API key
   - Create API key from NuGet.org account
   - Package push permission required

### Permission Settings

In Repository Settings > Actions > General:

- Workflow permissions: Select "Read and write permissions"
- Check "Allow GitHub Actions to create and approve pull requests"

## Usage

### Development Workflow

1. **Local development and testing**:

   ```bash
   git checkout -b feature/new-template
   
   # Make changes to templates...
   
   # Test locally (uses version 0.0.1)
   dotnet build
   dotnet pack --output ./local-test
   dotnet new install ./local-test/*.nupkg
   
   # Test your templates
   mkdir test-dir && cd test-dir
   dotnet new console-fba --name TestApp
   # Verify the template works...
   cd .. && rm -rf test-dir
   
   # Clean up
   dotnet new uninstall FbaTemplates
   ```

2. **Push and create Pull Request**:

   ```bash
   git commit -m "Add new template"
   git push origin feature/new-template
   ```

   - Create PR to `main` branch
   - Build and Test pipeline runs automatically
   - Code quality checks must pass
   - NuGet package artifacts are created (not deployed)

3. **Merge to main branch**:

   ```bash
   git checkout main
   git merge feature/new-template
   git push origin main
   ```

   - Build artifacts are created but not deployed to NuGet
   - Ready for tagging when release is needed

### Release Process

#### Method 1: Prerelease (Testing)

1. **Create prerelease tag**:

   ```bash
   git checkout main
   git tag preview/v1.0.0
   git push origin preview/v1.0.0
   ```

2. **Automatic execution**:
   - Prerelease workflow runs automatically
   - GitHub Prerelease created
   - NuGet prerelease package deployed (1.0.0-preview)

#### Method 2: Stable Release

1. **Create release tag**:

   ```bash
   git checkout main
   git tag release/v1.0.0
   git push origin release/v1.0.0
   ```

2. **Automatic execution**:
   - Release workflow runs automatically
   - GitHub Release created
   - NuGet stable package deployed

#### Method 3: Manual Release

1. **Select "Release to NuGet" workflow from GitHub Actions tab**
2. **Click "Run workflow"**
3. **Enter version** (e.g., 1.0.0)
4. **Execute**

### Version Management

- **Major version**: 1.x.x - Major changes, breaking compatibility
- **Minor version**: x.1.x - New feature additions
- **Patch version**: x.x.1 - Bug fixes
- **Prerelease**: 1.0.0-preview (from `preview/v1.0.0` tag)
- **Stable**: 1.0.0 (from `release/v1.0.0` tag)

### Tag Naming Convention

- **Development builds**: No deployment, artifacts only
- **Prerelease testing**: `preview/v1.0.0` → `1.0.0-preview` on NuGet
- **Stable release**: `release/v1.0.0` → `1.0.0` on NuGet

### Local Development

The project file (`FbaTemplates.csproj`) contains a development version `0.0.1` to avoid conflicts with released packages:

- **Local builds**: Use version `0.0.1` (safe for local testing)
- **CI/CD builds**: Version is dynamically set based on tags
- **Package testing**: Install local package with `dotnet new install ./artifacts/*.nupkg`
- **Cleanup**: Uninstall with `dotnet new uninstall FbaTemplates`

```bash
# Local development workflow
dotnet build                                    # Builds with version 0.0.1
dotnet pack --output ./local-artifacts         # Creates local package
dotnet new install ./local-artifacts/*.nupkg   # Install locally
dotnet new list | grep fba                     # Verify installation
# Test templates...
dotnet new uninstall FbaTemplates              # Clean up
```

### Template Addition Checklist

When adding a new template:

1. ✅ Create `content/{template-name}-fba/` directory
2. ✅ Add `Program.cs` file
3. ✅ Add `.template.config/template.json` configuration file
4. ✅ Include additional files if needed
5. ✅ Add new template test to both Prerelease and Release workflow test sections
6. ✅ Test with prerelease tag first: `preview/v1.0.0`
7. ✅ Deploy stable version with release tag: `release/v1.0.0`

### Troubleshooting

#### Build Failure

- Check project file syntax errors
- Verify .NET version compatibility
- Validate template JSON files

#### Deployment Failure

- Check NUGET_API_KEY secret
- Verify NuGet.org API key permissions
- Check for duplicate package versions

#### Test Failure

- Check template file structure
- Validate template.json configuration
- Check file permissions

#### Version Conflicts

- **Local testing**: Always uses version `0.0.1` (safe)
- **Template conflicts**: Uninstall existing templates with `dotnet new uninstall FbaTemplates`
- **Cache issues**: Clear NuGet cache with `dotnet nuget locals all --clear`
- **Multiple versions**: List installed templates with `dotnet new list`

#### Common Local Development Issues

```bash
# If template installation fails
dotnet new uninstall FbaTemplates   # Remove existing
rm -rf ./local-test                 # Clean artifacts
dotnet clean && dotnet build       # Rebuild
dotnet pack --output ./local-test  # Repack
dotnet new install ./local-test/*.nupkg  # Reinstall

# If templates don't appear
dotnet new list | grep -i fba       # Check installation
dotnet new --debug:reinit           # Reset template cache
```

## Monitoring

### Deployment Status Check

1. **GitHub Actions tab** - Check workflow execution status
2. **Releases tab** - Check created releases
3. **NuGet.org** - Check package deployment status

### Package Usage Statistics

- Check download statistics on NuGet.org package page
- Monitor repository activity in GitHub Insights

## Support

If issues occur:

1. Report issues in GitHub Issues
2. Check Actions logs
3. Refer to the troubleshooting section in this document
