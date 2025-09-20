# GitHub Copilot Instructions — WPF File-based App (FBA) Template

> Opinionated, execution-ready guidance for building **.NET 10** WPF apps as a **single C# file** using **MVVM** (CommunityToolkit.Mvvm).

---

## 1) Project Overview

Author a modern WPF desktop app with the **File-based App (FBA)** model: keep build/runtime directives at the top of one `.cs` file; put top-level bootstrapping first; types follow (App, Window, ViewModels, etc.). No `.csproj` is required.

---

## 2) FBA Directives (Windows-only GUI)

```csharp
// Shebang optional for Windows-only GUI apps

#:sdk Microsoft.NET.Sdk
#:package CommunityToolkit.Mvvm@8.4.0
#:property OutputType=WinExe
#:property TargetFramework=net10.0-windows
#:property UseWPF=True
#:property UseWindowsForms=False
#:property PublishAot=False        // WPF is not AOT-compatible
#:property Nullable=enable
```

**Notes**

* Prefer `net10.0-windows` to light up Windows-specific APIs.
* Keep all directives **at the very top**.
* If you add more packages, **pin exact versions** (see MCP section).

---

## 3) Quick Start (single file, DI + MVVM, no XAML)

```csharp
// wpf.cs
// FBA directives go above (see section 2)

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// --- Top-level bootstrap (must precede type declarations) ---

// WPF requires STA thread.
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Host for DI/config/logging (modern .NET pattern)
using var host = Host.CreateApplicationBuilder(args)
    .ConfigureServices(s =>
    {
        s.AddSingleton<App>();           // Application (created below)
        s.AddSingleton<MainWindow>();
        s.AddSingleton<CounterViewModel>();
    })
    .Build();

// Create and configure WPF Application
var app = host.Services.GetRequiredService<App>();
app.ShutdownMode = ShutdownMode.OnMainWindowClose;

app.DispatcherUnhandledException += (_, e) =>
{
    MessageBox.Show(e.Exception.Message, "Unhandled", MessageBoxButton.OK, MessageBoxImage.Error);
    e.Handled = true;
};

var main = host.Services.GetRequiredService<MainWindow>();
main.DataContext = host.Services.GetRequiredService<CounterViewModel>();

app.Run(main);   // blocks until window closes

// --- Types follow ---

public sealed class App : Application { }

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "WPF FBA (.NET 10) — MVVM (Toolkit)";
        Width = 480; Height = 280; WindowStartupLocation = WindowStartupLocation.CenterScreen;

        // UI tree (StackPanel)
        var root = new StackPanel { Margin = new Thickness(16), VerticalAlignment = VerticalAlignment.Center };

        var txt = new TextBlock { FontSize = 16, Margin = new Thickness(0, 0, 0, 12) };
        txt.SetBinding(TextBlock.TextProperty, new Binding(nameof(CounterViewModel.Greeting)));

        var count = new TextBlock { FontSize = 32, Margin = new Thickness(0, 0, 0, 12) };
        count.SetBinding(TextBlock.TextProperty, new Binding(nameof(CounterViewModel.Count)));

        var inc = new Button { Content = "Increment", MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        inc.SetBinding(Button.CommandProperty, new Binding(nameof(CounterViewModel.IncrementCommand)));

        var dec = new Button { Content = "Decrement", MinWidth = 120 };
        dec.SetBinding(Button.CommandProperty, new Binding(nameof(CounterViewModel.DecrementCommand)));

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(inc);
        row.Children.Add(dec);

        root.Children.Add(txt);
        root.Children.Add(count);
        root.Children.Add(row);

        Content = root;
    }
}

// ViewModel (Toolkit source generators simplify INotifyPropertyChanged/RelayCommand)
public sealed partial class CounterViewModel : ObservableObject
{
    [ObservableProperty] private int _count = 0;

    public string Greeting => $"Hello, today is {DateTime.Now:yyyy-MM-dd}";

    [RelayCommand] private void Increment() => Count++;
    [RelayCommand] private void Decrement() => Count--;
}
```

**Run**

```powershell
dotnet run wpf.cs
```

---

## 4) Initialization (must-haves)

* **STA thread**: `Thread.CurrentThread.SetApartmentState(ApartmentState.STA)`.
* **Application**: create a single `Application` instance; set `ShutdownMode`.
* **Unhandled exceptions**: hook `DispatcherUnhandledException` and fail gracefully.
* **Termination**: call `Application.Current.Shutdown()` when your objectives complete (for agent workflows or scripted runs).

---

## 5) Declarative UI in Code (XAML-free)

* Build visual trees via **object initializers** and **collection initializers**.
* Use `SetBinding` with `Binding` objects (prefer `nameof(ViewModel.Property)`).
* Keep layout simple (e.g., `Grid`, `StackPanel`, `DockPanel`); avoid absolute positions.
* Move complex UI into helper methods or partial classes to keep the file readable.

---

## 6) MVVM with CommunityToolkit.Mvvm

* Base VMs on `ObservableObject`.
* Use `[ObservableProperty]` for bindable properties and `[RelayCommand]`/`AsyncRelayCommand` for commands.
* Keep VMs **UI-agnostic** and testable; inject dependencies through the constructor (DI).

**Example**

```csharp
public sealed partial class ExampleViewModel : ObservableObject
{
    [ObservableProperty] private string _query = "";

    [RelayCommand]
    private async Task SearchAsync()
    {
        // async work; update properties; raise change notifications automatically
        await Task.Delay(100);
    }
}
```

---

## 7) Data Binding & Validation

* Prefer **TwoWay** binding only where needed; otherwise default OneWay reduces churn.
* Use `INotifyDataErrorInfo` or data annotations for validation; surface errors via styles or adorner layers.
* Bind `ItemsControl`/`ListBox` to `ObservableCollection<T>` and use `CollectionViewSource` for sorting/filtering.

---

## 8) Threading & Async

* UI work must run on the **Dispatcher**. Use `Dispatcher.Invoke/BeginInvoke` when crossing threads.
* For long operations: `await Task.Run(...)` with `CancellationToken` and progress reporting.
* **Do not** blanket-apply `ConfigureAwait(false)` in WPF UI code; continuations commonly need the captured context. Reserve it for **library** code.

---

## 9) Styling, Theming, Resources

* Centralize brushes, fonts, and styles in `Application.Resources`/`ResourceDictionary` objects (you can build these programmatically in FBA).
* Respect system/high-contrast themes; avoid hard-coded colors.
* For DPI, WPF is DPI-aware by default; test Per-Monitor-V2 scenarios (manifest-driven).

---

## 10) Performance

* Minimize binding chatter: avoid unnecessary property updates; compute heavy values lazily.
* Virtualize long lists (`VirtualizingStackPanel`, proper `ScrollViewer` usage).
* Avoid retaining event handlers that capture long-lived objects; prefer weak event patterns or explicit unsubscribe.

---

## 11) Testing

* **Unit**: ViewModels only—no WPF dependencies; assert property changes and command behavior.
* **Integration**: optional UI automation (WinAppDriver/Playwright for Windows) for key flows.
* **Perf**: open window, interact, close—watch allocations and leaks (Finalizer queue, Live Visual Tree).

---

## 12) Deployment

* Use MSIX or installer of your choice; document **Windows** and **.NET** runtime requirements.
* Store settings under user scope; secure secrets (Windows DPAPI/Credential Locker).
* Provide upgrade paths; keep user data in `%APPDATA%`.

---

## 13) MCP Integration (NuGet & Docs)

* **Never guess package versions.** Resolve exact versions with your **`nuget` MCP server** and pin via `#:package PackageId@X.Y.Z`.
* Use **`microsoft_learn` MCP** to verify current WPF/MVVM guidance, DPI, accessibility, and deployment recommendations.

---

## 14) File-based Development Standards

* Default to **single-file FBA** unless a classic project is explicitly requested.
* Keep directives (`#:sdk`, `#:property`, `#:package`) **at the top**.
* If the file grows, introduce a `Directory.Build.props` for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 15) Project Conversion (on request)

* Convert to a traditional project **only** when explicitly asked (`dotnet project convert wpf.cs -o <dir>`).
* Preserve the original FBA; re-pin packages; mirror properties.

---

## 16) Agent Execution Compatibility

* Do **not** add `Console.ReadLine()`/`ReadKey()` blockers.
* WPF apps run until the window closes; call `Application.Current.Shutdown()` when automation completes to allow reliable STDOUT/exit-code capture.

---

### Review Checklist

* [ ] `TargetFramework=net10.0-windows`, `UseWPF=True`, `PublishAot=False`, `Nullable=enable`
* [ ] STA thread set; `Application` created; unhandled exceptions handled
* [ ] DI host configured; VM injected; `DataContext` set
* [ ] Bindings use `nameof`; TwoWay only where needed; validation strategy chosen
* [ ] No blanket `ConfigureAwait(false)` in UI paths
* [ ] Resource dictionaries/styles centralized; DPI/contrast respected
* [ ] Single-file FBA; exact package versions pinned via MCP `nuget` if additional packages are used
