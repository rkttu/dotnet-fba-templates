# dotnet-fba-templates

[![NuGet](https://img.shields.io/nuget/v/FbaTemplates.svg)](https://www.nuget.org/packages/FbaTemplates/)

A collection of .NET project templates for creating **File-based Applications (FBA)** - simple C# console applications that can be executed directly without traditional project files using the `dotnet` command-line interface.

## What are File-based Applications?

File-based Applications (FBA) are a modern approach to creating simple C# console applications that leverage the power of .NET's top-level programs and the `dotnet` CLI. These applications consist of a single `.cs` file that can be executed directly using `dotnet run` or `dotnet <filename>.cs`, without the need for a traditional `.csproj` project file.

### Key Benefits

- **Simplicity**: Single file applications perfect for scripts, utilities, and prototypes
- **Quick Start**: No project setup required - just create a `.cs` file and run
- **Portable**: Easy to share and distribute as a single file
- **Modern C#**: Support for latest C# features including top-level programs
- **AOT Ready**: Templates include AOT (Ahead-of-Time) compilation support options

## Installation

Install the template package globally using the .NET CLI:

```bash
dotnet new install FbaTemplates
```

## Available Templates

### FbaConsole

A basic file-based console application template.

**Usage:**

```bash
dotnet new fbaconsole -n MyApp
```

**Features:**

- Top-level program structure
- Shebang line for cross-platform execution
- Basic "Hello, World!" implementation

### FbaConsoleAot

A file-based console application template optimized for AOT compilation.

**Usage:**

```bash
dotnet new fbaconsolaot -n MyAotApp
```

**Features:**

- AOT-ready implementation
- Optimized for native compilation
- Minimal runtime dependencies

## Usage Examples

### Creating a New File-based Application

1. **Create a new FBA console application:**

   ```bash
   dotnet new fbaconsole -n calculator
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

Each template creates a minimal C# file with:

```csharp
#!/usr/bin/env dotnet

// Optional properties for build configuration
#:property PublishAot=False

// Your application code here
Console.WriteLine("Hello, World!");
```

### Special Directives

- **Shebang line** (`#!/usr/bin/env dotnet`): Enables direct execution on Unix systems
- **Property directives** (`#:property`): Configure build settings inline

## Requirements

- .NET 10.0 or later
- dotnet CLI

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

### Adding New Templates

To contribute a new File-based Application template:

1. **Create template content**: Add your template files under the `content/` directory following the existing structure
2. **Template naming**: Use descriptive names that start with "Fba" (e.g., `FbaWebApi`, `FbaWorker`)
3. **Include documentation**: Ensure your template includes appropriate comments and examples
4. **Test locally**: Verify your template works by building and testing the package locally
5. **Submit a pull request**: Include a description of what your template does and its intended use case

All new templates should follow these guidelines:

- Include a shebang line for cross-platform compatibility
- Use top-level program structure when appropriate
- Provide clear, commented example code
- Consider AOT compatibility where relevant

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Resources

- [.NET CLI documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Creating custom templates](https://docs.microsoft.com/en-us/dotnet/core/tools/custom-templates)
- [Top-level programs in C#](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements)
