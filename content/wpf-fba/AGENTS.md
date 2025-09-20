# GitHub Copilot Instructions for WPF File-based App Template

## Project Overview

This is a WPF (Windows Presentation Foundation) File-based Application template using .NET 10 syntax for building modern desktop GUI applications with XAML-like declarative UI patterns using C# code.

## .NET 10 File-based App Syntax Guidelines

### File-based App Directives

- Shebang (`#!/usr/bin/env dotnet`) is optional for Windows GUI applications as they are Windows-specific
- Use `#:sdk Microsoft.NET.Sdk` for Windows desktop applications
- Include `#:package CommunityToolkit.Mvvm@8.4.0` for MVVM support
- Set `#:property OutputType=WinExe` for Windows executable
- Use `#:property TargetFramework=net10.0-windows` for Windows-specific framework
- Enable WPF with `#:property UseWPF=True`
- Disable Windows Forms with `#:property UseWindowsForms=False`
- Set `#:property PublishAot=False` (WPF cannot use AOT)

### Example File-based App Structure

```csharp
// Optional: #!/usr/bin/env dotnet (not needed for Windows-only GUI apps)

#:sdk Microsoft.NET.Sdk
#:package CommunityToolkit.Mvvm@8.4.0
#:property OutputType=WinExe
#:property TargetFramework=net10.0-windows
#:property UseWPF=True
#:property UseWindowsForms=False
#:property PublishAot=False

// Your WPF application code here
```

## WPF Specific Guidelines

### Application Initialization

- **STANDARD**: Use `Host.CreateApplicationBuilder(args)` for dependency injection in WPF apps (modern .NET 10+ pattern)
- **CRITICAL**: In File-based Apps with Top-Level Programs, Main method code must come first, followed by type definitions (classes, interfaces, delegates, structs) to avoid syntax errors
- Set apartment state to STA: `Thread.CurrentThread.SetApartmentState(ApartmentState.STA)`
- Create `Application` instance with proper configuration
- Set `ShutdownMode` appropriately
- Handle unhandled exceptions with `DispatcherUnhandledException`
- **IMPORTANT**: When application objectives are achieved, call `Application.Current.Shutdown()` to terminate autonomously

### Declarative UI in Code

- Create UI elements using object initializers
- Set properties using initialization syntax
- Use `SetBinding()` for data binding
- Organize UI hierarchy with proper parent-child relationships

### Data Binding

- Use `Binding` class for property bindings
- Bind to ViewModel properties using property names
- Use `SetBinding()` method for programmatic binding
- Implement proper binding contexts with `DataContext`

### MVVM Pattern Implementation

- Use CommunityToolkit.Mvvm for MVVM support
- Inherit ViewModels from `ObservableObject`
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for command implementations

## Modern C# Syntax Standards

### Language Features (C# 12/13)

- Use file-scoped namespaces where applicable
- Prefer primary constructors for simple classes
- Use collection expressions where appropriate
- Utilize raw string literals for multi-line text
- Apply required members and init-only properties

### Object Initialization

- Use object initializers for WPF controls
- Apply collection initializers for child collections
- Use target-typed new expressions where appropriate
- Implement proper property initialization patterns

### Null Safety

- Enable nullable reference types
- Use null-conditional operators (`?.`, `??`, `??=`)
- Handle nullable control references appropriately
- Validate binding targets before operations

### Pattern Matching

- Use switch expressions for UI state management
- Utilize property patterns for control state checking
- Apply list patterns for collection operations
- Use pattern matching with `is` expressions for type checking

## MVVM Architecture Best Practices

### ViewModel Design

- Use `ObservableObject` base class from CommunityToolkit.Mvvm
- Implement properties with `[ObservableProperty]` attribute
- Create commands with `[RelayCommand]` attribute
- Keep ViewModels testable and UI-agnostic

### Data Binding Patterns

- Bind UI elements to ViewModel properties
- Use two-way binding for input controls
- Implement validation with `INotifyDataErrorInfo`
- Handle collection binding with `ObservableCollection<T>`

### Command Implementation

- Use `RelayCommand` for simple commands
- Use `AsyncRelayCommand` for async operations
- Implement `CanExecute` logic when appropriate
- Handle command parameter passing

### Example MVVM Pattern

```csharp
public sealed partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count = 0;

    [RelayCommand]
    private void IncrementCount() => Count++;
}
```

## WPF UI Design Patterns

### Layout Management

- Use appropriate layout panels (StackPanel, Grid, DockPanel)
- Set proper margins and padding
- Implement responsive design with proper sizing
- Handle different screen resolutions and DPI settings

### Control Creation

- Create controls with object initializers
- Set appropriate properties for functionality
- Configure styling and appearance
- Implement proper event handling

### Window Management

- Set appropriate window properties (Title, Size, StartupLocation)
- Handle window lifecycle events
- Implement proper window positioning
- Configure window behavior appropriately

## Data Binding and Validation

### Binding Syntax

- Use `Binding` class for programmatic binding
- Specify property names as strings (consider using nameof())
- Configure binding modes appropriately
- Handle binding failures gracefully

### Validation Implementation

- Implement `INotifyDataErrorInfo` for validation
- Use data annotations for simple validation
- Provide user-friendly error messages
- Handle validation in ViewModels

### Collection Binding

- Use `ObservableCollection<T>` for dynamic collections
- Implement proper collection change notifications
- Handle item selection and editing
- Use collection views for filtering and sorting

## Threading and Async Operations

### Dispatcher Management

- Use `Dispatcher.Invoke()` for UI thread operations
- Implement proper async/await patterns
- Use `AsyncRelayCommand` for async commands
- Handle long-running operations appropriately

### Background Operations

- Use `Task.Run()` for CPU-intensive operations
- Implement proper cancellation with `CancellationToken`
- Provide progress feedback with `IProgress<T>`
- Handle exceptions from background threads

## Styling and Theming

### Programmatic Styling

- Set control properties for appearance
- Use system colors and fonts when appropriate
- Implement consistent styling across the application
- Handle high contrast and accessibility themes

### Resource Management

- Create and manage application resources
- Use resource dictionaries for shared resources
- Implement proper resource cleanup
- Handle resource loading failures

## .NET Version Requirements

- **MANDATORY**: This template targets .NET 10 and later versions
- **PROHIBITED**: Do not reference packages or features that require versions lower than .NET 10
- Always verify package compatibility with .NET 10+ before adding dependencies
- Use the latest language features and APIs available in .NET 10

## Testing Strategies

### Unit Testing ViewModels

- Test ViewModel logic independently from UI
- Mock external dependencies appropriately
- Test property change notifications
- Validate command execution logic

### UI Testing

- Use automated UI testing frameworks when appropriate
- Test data binding scenarios
- Validate user interaction workflows
- Test accessibility features

### Integration Testing

- Test complete MVVM interactions
- Validate data flow between layers
- Test error handling scenarios
- Verify performance characteristics

## Performance Optimization

### Binding Performance

- Use efficient binding modes
- Avoid unnecessary property change notifications
- Implement proper collection virtualization
- Cache frequently accessed data

### Memory Management

- Implement proper disposal patterns
- Avoid memory leaks with event handlers
- Use weak references where appropriate
- Monitor memory usage in long-running applications

## Deployment Considerations

### Application Packaging

- Use appropriate deployment strategies
- Include necessary WPF runtime dependencies
- Handle application updates gracefully
- Configure proper security permissions

### System Requirements

- Document Windows version requirements
- Specify .NET runtime requirements
- List any additional dependencies
- Provide installation instructions

### Configuration Management

- Use application settings for user preferences
- Implement proper configuration persistence
- Handle different user permission levels
- Store sensitive data securely

## MCP Server Integration Guidelines

### Package Version Management

- **CRITICAL**: Never guess or assume NuGet package versions
- **MANDATORY**: Always use the `nuget` MCP server to verify exact package versions before adding dependencies
- Query the nuget MCP server with package names to get the latest stable versions
- Use the exact version numbers returned by the MCP server in your `#:package` directives
- **WPF-SPECIFIC**: Verify WPF and MVVM package version compatibility using the MCP server

### Microsoft Technology Research

- **RECOMMENDED**: Use the `microsoft_learn` MCP server to research latest .NET WPF technologies and desktop application best practices
- Query for best practices, new features, and modern WPF patterns
- Use the MCP server to stay updated with the latest Microsoft documentation and tutorials
- Plan implementations based on official Microsoft guidance obtained through the MCP server

### File-based App Execution

- **EXECUTION**: File-based Apps must be run using `dotnet run filename.cs` command
- Ensure the shebang `#!/usr/bin/env dotnet` is present at the top of the file (optional for Windows-only apps)
- The file should be executable directly with dotnet run without requiring a project file
- Test execution with the exact `dotnet run filename.cs` syntax to verify functionality

## File-based App Development Standards

### Code Style Requirements

- **MANDATORY**: Always write File-based App style code unless explicitly requested otherwise by the user
- **PROHIBITED**: Never create project files arbitrarily or without explicit user request
- **CRITICAL**: Maintain the single-file approach with all dependencies declared using FBA syntax
- All code must be contained within a single C# source file with appropriate FBA directives

### File Organization Patterns

- **SEPARATION**: When file length becomes excessive and requires separation, create a `Directory.Build.props` file
- **MSBUILD INTEGRATION**: Use `Directory.Build.props` for MSBuild-style SDK tags and project-level configurations
- **FBA SYNTAX PRIORITY**: PropertyGroup settings, SDK configurations, and NuGet package references must use FBA syntax in the main C# source file
- **DIRECTIVE PLACEMENT**: Keep `#:property`, `#:sdk`, and `#:package` directives at the top of the main FBA file

### Project Conversion Guidelines

- **CONVERSION COMMAND**: Use `dotnet project convert fba.cs` to convert File-based App to traditional project structure when requested
- **OUTPUT DIRECTORY**: Specify `-o directoryname` option to create conversion output in a separate directory
- **USER REQUEST ONLY**: Only perform project conversion when explicitly requested by the user
- **PRESERVE ORIGINAL**: Always maintain the original FBA file during conversion process

## Agent Execution Compatibility

### Program Termination Guidelines

- **PROHIBITED**: Do not add program termination prevention functions like `Console.ReadLine()` or `Console.ReadKey()` at the end of programs unless explicitly requested by the user
- **CRITICAL**: Allow programs to terminate naturally so agents can collect STDOUT and STDERR output for analysis
- **PERFORMANCE**: Avoiding unnecessary input blocking reduces execution time and improves agent workflow efficiency
- **OUTPUT COLLECTION**: Natural program termination enables proper output collection and evaluation by automated agents
