# .NET File-based App Templates

A template NuGet package for creating new file-based app projects in .NET 10+. This template helps you quickly write file-based application code.

## Installation

Install the template package from NuGet:

```bash
dotnet new install DotNet.FBA.Templates
```

## Usage

Create a new file-based app project:

```bash
# Create in current directory
dotnet new fba

# Create in a new directory
dotnet new fba -n MyFileApp

# Specify target framework
dotnet new fba -n MyFileApp --TargetFramework net8.0
```

### Available Parameters

- `--TargetFramework` or `-f`: The target framework for the project
  - `net10.0` (default): Target .NET 10
  - `net9.0`: Target .NET 9
  - `net8.0`: Target .NET 8
- `--SkipRestore`: Skip automatic package restore after project creation

## Template Features

The file-based app template includes:

- ✅ Basic console application structure
- ✅ File I/O operations examples
- ✅ Async/await patterns for file operations
- ✅ Error handling for file operations
- ✅ Modern C# features (nullable reference types, implicit usings)
- ✅ Ready for .NET 10+ when available

## Example

The generated project includes a sample that demonstrates:

- Creating files
- Reading file content
- Checking file existence
- Exception handling for file operations
- Proper cleanup

Replace the template code with your specific file-based application logic.

## Requirements

- .NET SDK 8.0 or later (ready for .NET 10)

## Uninstallation

To remove the template:

```bash
dotnet new uninstall DotNet.FBA.Templates
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
