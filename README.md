# dotnet-fba-templates

[![NuGet](https://img.shields.io/nuget/v/FbaTemplates.svg)](https://www.nuget.org/packages/FbaTemplates/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FbaTemplates.svg)](https://www.nuget.org/packages/FbaTemplates/)
[![Build and Test](https://github.com/rkttu/dotnet-fba-templates/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/rkttu/dotnet-fba-templates/actions/workflows/ci-cd.yml)
[![Release](https://github.com/rkttu/dotnet-fba-templates/actions/workflows/release.yml/badge.svg)](https://github.com/rkttu/dotnet-fba-templates/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub Stars](https://img.shields.io/github/stars/rkttu/dotnet-fba-templates?style=social)](https://github.com/rkttu/dotnet-fba-templates/stargazers)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/rkttu/dotnet-fba-templates/blob/main/README.md#contributing)

A collection of .NET project templates for creating **File-based Applications (FBA)** - simple C# console applications that can be executed directly without traditional project files using the `dotnet` command-line interface.

> 💡 **Love this project?** Give it a ⭐ and consider [contributing](https://github.com/rkttu/dotnet-fba-templates/blob/main/README.md#contributing) to help make File-based Applications even better!

## What are File-based Applications?

File-based Applications (FBA) are a modern approach to creating simple C# console applications that leverage the power of .NET's top-level programs and the enhanced `dotnet` CLI capabilities in .NET 10.0+. These applications consist of a single `.cs` file that can be executed directly using `dotnet run` or `dotnet <filename>.cs`, without the need for a traditional `.csproj` project file.

> **⚠️ Important**: File-based Applications require .NET 10.0 or later for optimal functionality and feature support.

### Key Benefits

- **Simplicity**: Single file applications perfect for scripts, utilities, and prototypes
- **Quick Start**: No project setup required - just create a `.cs` file and run
- **Portable**: Easy to share and distribute as a single file
- **Modern C#**: Support for latest C# features including top-level programs
- **AOT Ready**: Templates include AOT (Ahead-of-Time) compilation support options
- **AI Agent Ready**: Built-in `AGENTS.md` files in each template provide ready-to-use configuration for AI agents and GitHub Copilot integration
- **MCP Configuration Ready**: Each template includes pre-configured `mcp.json` files for VS Code and Cursor with Microsoft Learn and NuGet MCP server settings built-in
- **Expandable**: Can be converted to full projects later using `dotnet project convert your_code.cs` when more complexity is needed

## Installation

**Prerequisites**: Ensure you have .NET 10.0 or later installed. Check with:

```bash
dotnet --version
```

Install the template package globally using the .NET CLI:

```bash
dotnet new install FbaTemplates
```

> **💡 Tip**: If you encounter compatibility issues, verify you're running .NET 10.0+ as this template package is specifically designed for the latest .NET features.

## Available Templates

| Template | Category | Description | Usage |
|----------|----------|-------------|-------|
| `console-fba` | Console | Basic console application | `dotnet new console-fba -n MyApp` |
| `minimal-api-fba` | Web | Lightweight web API using Minimal APIs | `dotnet new minimal-api-fba -n MyApi` |
| `mvc-fba` | Web | Full-featured MVC web application | `dotnet new mvc-fba -n MyMvcApp` |
| `aspire-fba` | Cloud | .NET Aspire cloud-native orchestrator | `dotnet new aspire-fba -n MyAspireApp` |
| `awscdk-fba` | Cloud | AWS CDK Infrastructure as Code | `dotnet new awscdk-fba -n MyInfra` |
| `winforms-fba` | Desktop | Windows Forms desktop application | `dotnet new winforms-fba -n MyWinApp` |
| `wpf-fba` | Desktop | WPF desktop application with XAML | `dotnet new wpf-fba -n MyWpfApp` |
| `wpf-linqui-fba` | Desktop | WPF desktop application with LinqUI declarative UI powered by [LinqUI.WPF](https://github.com/XAMLBaker/LinqUI) | `dotnet new wpf-linqui-fba -n MyLinqUiApp` |
| `mcpserver-stdio-fba` | AI/Integration | Model Context Protocol server implementation | `dotnet new mcpserver-stdio-fba -n MyMcp` |
| `pythonnet-fba` | Integration | Python.NET interoperability | `dotnet new pythonnet-fba -n MyPyApp` |
| `wasm-fba` | Web | WebAssembly browser application | `dotnet new wasm-fba -n MyWasmApp` |
| `win32dll-fba` | System | Win32 DLL creation and usage | `dotnet new win32dll-fba -n MyDllApp` |
| `win32rundll-fba` | System | RunDLL32 compatible DLL | `dotnet new win32rundll-fba -n MyRunDll` |

## Usage Examples

### Creating a New File-based Application

1. **Create a new FBA console application:**

   ```bash
   dotnet new console-fba -n calculator
   ```

2. **Run the application directly:**

   ```bash
   dotnet run calculator.cs
   ```

3. **Or execute with shebang (Linux/macOS):**

   ```bash
   chmod +x calculator.cs
   ./calculator.cs
   ```

### Creating Different Types of Applications

```bash
# Console application
dotnet new console-fba -n MyConsole

# Web API
dotnet new minimal-api-fba -n MyApi

# MVC web application
dotnet new mvc-fba -n MyWebApp

# Desktop application (Windows)
dotnet new winforms-fba -n MyDesktopApp

# WPF application with declarative UI (Windows)
dotnet new wpf-linqui-fba -n MyWpfApp

# Cloud-native application
dotnet new aspire-fba -n MyCloudApp
```

### Running Existing File-based Applications

File-based applications can be executed in several ways:

```bash
# Method 1: Using dotnet run
dotnet run Program.cs

# Method 2: Using dotnet with file path
dotnet Program.cs

# Method 3: Direct execution (with shebang on Unix systems)
./Program.cs
```

## Template Structure

Each template creates a minimal C# file with special directives for configuration:

### Console Application Example

```csharp
#!/usr/bin/env dotnet

#:property TargetFramework=net10.0
#:property PublishAot=false

Console.WriteLine("Hello, World!");
```

### Web Application Example

```csharp
#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=net10.0
#:property PublishAot=false

var builder = WebApplication.CreateBuilder(args);
using var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
```

### Special Directives

- **Shebang line** (`#!/usr/bin/env dotnet`): Enables direct execution on Unix systems
- **SDK directive** (`#:sdk`): Specifies the project SDK (e.g., Microsoft.NET.Sdk.Web)
- **Property directives** (`#:property`): Configure build settings inline
- **Framework targeting**: Supports .NET 8.0, 9.0, and 10.0
- **AOT compilation**: Optional ahead-of-time compilation support

## Requirements

### ⚠️ Important: .NET Version Requirement

**This template package requires .NET 10.0 or later.** File-based Applications leverage the latest .NET features and the enhanced `dotnet` CLI capabilities available only in .NET 10.0+.

### System Requirements

- **✅ .NET 10.0 or later** (Required)
- **✅ dotnet CLI** (Included with .NET SDK)
- Platform-specific requirements:
  - **Windows**: Windows 10 or later for desktop applications
  - **Linux/macOS**: Compatible with .NET 10.0+ runtime

### Verification

To check your .NET version:

```bash
dotnet --version
```

If you need to install .NET 10.0, visit: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

## Editor Support

To get the best development experience with IntelliSense support for File-based Applications, install the appropriate C# extension for your editor:

### Visual Studio Code

Install the official Microsoft C# extension:

- **Extension**: [C# for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- **Installation**: `code --install-extension ms-dotnettools.csharp`

### VS Code Forks (Cursor, Windsurf, Eclipse Theia, Amazon Kiro, Trae, etc.)

For VS Code-based editors that use Open VSX Registry, install the community-maintained extension:

- **Extension**: [C# for Open VSX](https://open-vsx.org/extension/dotnetdev-kr-custom/csharp)
- **Installation**: Use your editor's extension marketplace or install via command line

### Features Provided

Both extensions provide:

- **IntelliSense**: Code completion and syntax highlighting for `.cs` files
- **Error detection**: Real-time error checking and diagnostics
- **Debugging support**: Breakpoints and step-through debugging
- **File-based App recognition**: Proper handling of single-file C# applications
- **Framework targeting**: Support for different .NET versions

### Configuration

No additional configuration is required. The extensions automatically recognize File-based Applications and provide appropriate IntelliSense based on the framework directives in your `.cs` files.

## Template Parameters

Most templates support the following parameters:

- `--Framework` or `-F`: Target framework (net10.0 - default and recommended)
- `--EnableAot`: Enable AOT compilation (true/false)
- `--name` or `-n`: Name of the application

**Note**: All templates are optimized for .NET 10.0 and may not work correctly with earlier versions.

Example:

```bash
dotnet new console-fba -n MyApp --EnableAot true
```

## Template Documentation

For detailed information about specific templates, see our [template documentation](docs/):

- **[WPF LinqUI Template](docs/wpf-linqui-fba.md)** - Complete guide to building XAML-free WPF applications with declarative UI

## Contributing

We actively welcome contributions and feedback from the community! 🎉 Whether you're a seasoned developer or just getting started with File-based Applications, your input helps make this project better for everyone.

### How You Can Help

- 🐛 **Report bugs or issues** - Found something that doesn't work as expected?
- 💡 **Suggest new features** - Have an idea for improving existing templates or adding new ones?
- 📝 **Improve documentation** - Help us make the docs clearer and more comprehensive
- 🔧 **Submit code contributions** - Fix bugs, add features, or create new templates
- 💬 **Share feedback** - Tell us about your experience using File-based Applications
- ⭐ **Star the repository** - Show your support and help others discover the project

### Getting Started

1. **Check existing issues** - See if someone else has already reported the same issue or requested a similar feature
2. **Open a discussion** - Not sure if your idea fits? Start a discussion to get feedback from the community
3. **Fork and contribute** - Ready to code? Fork the repository and start contributing!

### Adding New Templates

To contribute a new File-based Application template:

1. **Create template structure**: Add your template under `content/{template-name}-fba/`
2. **Add configuration**: Create `.template.config/template.json` with template metadata
3. **Template naming**: Use descriptive names ending with "-fba" (e.g., `worker-fba`, `grpc-fba`)
4. **Include documentation**: Ensure your template includes appropriate comments and examples
5. **Test locally**: Build and test the package using version 0.0.1
6. **Submit a pull request**: Include description of the template's purpose and usage

### Template Guidelines

All new templates should follow these guidelines:

- Include a shebang line (`#!/usr/bin/env dotnet`) for cross-platform compatibility
- Use appropriate SDK directive (`#:sdk`) when needed
- Support framework targeting with parameters
- Provide AOT compilation option where applicable
- Include clear, commented example code
- Follow the existing naming convention ({category}-fba)

### Community & Support

- 💬 **Discussions**: Join our [GitHub Discussions](https://github.com/rkttu/dotnet-fba-templates/discussions) to ask questions, share ideas, or show off what you've built
- 📋 **Issues**: Report bugs or request features in our [Issue Tracker](https://github.com/rkttu/dotnet-fba-templates/issues)
- 🚀 **Showcase**: Built something cool with File-based Applications? We'd love to hear about it!

We believe that File-based Applications can revolutionize how developers approach simple C# projects, and your feedback helps us achieve that vision. Every contribution, no matter how small, makes a difference! 🌟

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Resources

- [.NET CLI documentation](https://learn.microsoft.com/en-us/dotnet/core/tools)
- [.NET projects SDKs](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [Creating custom templates](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates)
- [Top-level programs in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements)
- [File-based Applications documentation](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/file-based-programs)
- [AOT compilation in .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=windows%2Cnet8)
- [.NET Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Build an MCP server](https://modelcontextprotocol.io/docs/develop/build-server#c%23)
