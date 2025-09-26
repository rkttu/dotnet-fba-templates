# GitHub Copilot Instructions — WPF File-based App (FBA) Template

> Pragmatic, production-ready guidance for building **.NET 10** WPF apps as a **single C# file** with **MVVM (CommunityToolkit.Mvvm)**.
> **Apartment model:** Per field validation, WPF startup **must** perform the two-step STA initialization: **`Unknown → STA`**.

---

## 1) Project Overview

Author a modern WPF desktop app using the **File-based App (FBA)** model: keep build/runtime directives at the very top of one `.cs` file, place top-level bootstrapping first, and define all types (App, Window, ViewModels, etc.) after it.

---

## 2) FBA Directives (Windows-only GUI)

```csharp
// Shebang optional for Windows-only apps

#:sdk Microsoft.NET.Sdk
#:package CommunityToolkit.Mvvm@8.4.0
#:package LinqUI.WPF@1.0.6
#:property OutputType=WinExe
#:property TargetFramework=net10.0-windows
#:property UseWPF=True
#:property UseWindowsForms=False
#:property PublishAot=False        // WPF is not AOT-compatible
#:property Nullable=enable
```

**Notes**

* Use `net10.0-windows` to enable Windows APIs.
* Keep every directive at the top of the file.
* If you add more packages, **pin exact versions**.

---

## 3) Quick Start (single file, DI + required STA init, XAML-free UI)

```csharp
// wpf.cs
// FBA directives appear above (see section 2)

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinqUI.WPF;
// --- Top-level bootstrap must precede type declarations ---

// **CRITICAL**: Two-step STA initialization (empirically required)
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Host for DI/config/logging
using var host = Host.CreateApplicationBuilder(args)
    .ConfigureServices(s =>
    {
        s.AddSingleton<App>();
        s.AddSingleton<MainWindow>();
        s.AddSingleton<CounterViewModel>();
    })
    .Build();

// Create WPF Application and wire global behavior
var app = host.Services.GetRequiredService<App>();
app.ShutdownMode(ShutdownMode.OnMainWindowClose)
    .OnDispatcherUnhandledException((_, e) =>
    {
        MessageBox.Show(e.Exception.ToString(), "Unhandled", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    });

// Compose main window & VM
var main = host.Services.GetRequiredService<MainWindow>();
main.DataContext = host.Services.GetRequiredService<CounterViewModel>();

// Run (blocks until closed). Call Application.Current.Shutdown() for scripted/agent flows.
app.Run(main);

// ---- Types follow here ----

public sealed class App : Application { }

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        this.Title()"WPF FBA (.NET 10) — MVVM (Toolkit)")
            .Size(480, 280)
            .WindowStartupLocation(WindowStartupLocation.CenterScreen)
            .Content(
                new StackPanel()
                        .VCenter()
                        .Margin(16)
                        .Children(
                            new TextBlock()
                                .FontSize(16)
                                .Margin(bottom: 12)
                                .Bind(TextBlock.TextProperty, new Binding(nameof(CounterViewModel.Greeting))),
                            new TextBlock()
                                .FontSize(32)
                                .Margin(bottom: 12)
                                .Bind(TextBlock.TextProperty, new Binding(nameof(CounterViewModel.Count))),
                            new StackPanel()                                
                                .Orientation(Orientation.Horizontal)
                                .Children(
                                    new Button()
                                        .Content("Increment")
                                        .MinWidth(120)
                                        .Margin(right: 8)
                                        .Bind(Button.CommandProperty, new Binding(nameof(CounterViewModel.IncrementCountCommand))),
                                    new Button()
                                        .Content("Decrement")
                                        .MinWidth(120)
                                        .Bind(Button.CommandProperty, new Binding(nameof(CounterViewModel.DecrementCountCommand)))
                                )
                        )
            )
    }
}

// ViewModel with Toolkit generators
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

## 4) Application Initialization

* **STA Apartment (CRITICAL):**
  Execute **both**:

  ```csharp
  Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
  Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
  ```

  This two-step init prevents intermittent COM initialization issues and ensures stable WPF UI thread behavior.
* **App lifecycle:** single `Application` instance; set `ShutdownMode` as needed.
* **Unhandled exceptions:** handle `DispatcherUnhandledException` to fail gracefully.
* **Termination for agents:** call `Application.Current.Shutdown()` when objectives are achieved.

---

## 5) Declarative UI in Code

* Use **object/collection initializers** to compose the visual tree.
* Bind with `SetBinding` and `Binding`; prefer `nameof(ViewModel.Property)` to avoid string typos.
* Keep layout simple (`Grid`, `StackPanel`, `DockPanel`); avoid absolute coordinates.

---

## 6) MVVM with CommunityToolkit.Mvvm

* Base view models on `ObservableObject`.
* Use `[ObservableProperty]` for bindable state and `[RelayCommand]`/`AsyncRelayCommand` for actions.
* Inject dependencies via DI; keep VMs UI-agnostic and testable.

---

## 7) Data Binding & Validation

* Use OneWay bindings by default; switch to TwoWay only where input is required.
* Validation: `INotifyDataErrorInfo` or data annotations; present errors clearly.
* Collections: `ObservableCollection<T>`; use `CollectionViewSource` for filter/sort.

---

## 8) Threading & Async

* UI access must occur on the **Dispatcher** (`Dispatcher.Invoke/BeginInvoke`).
* Offload long work: `await Task.Run(...)` with `CancellationToken`.
* Avoid blanket `ConfigureAwait(false)` in UI code (you typically need the captured context).

---

## 9) Styling, Theming, Resources

* Centralize styles/brushes in `Application.Resources` (build programmatically in FBA as needed).
* Respect system/high-contrast themes; avoid hard-coded colors.

---

## 10) Performance

* Reduce binding churn; compute heavy values lazily.
* Virtualize lists (`VirtualizingStackPanel`) and avoid retaining event handlers unintentionally.
* Monitor allocations/leaks in long-running sessions.

---

## 11) Testing

* **Unit:** ViewModels, commands, and validation (no WPF references).
* **Integration/UI:** automate critical flows (e.g., WinAppDriver/Playwright for Windows).
* **Perf/Leak checks:** open/close windows and watch for survivors/GC pressure.

---

## 12) Deployment

* Package with MSIX/installer; document Windows/.NET requirements.
* Store user settings under user scope; protect secrets (DPAPI/Credential Locker).
* Provide safe upgrade paths and migrations.

---

## 13) File-based Dev Standards

* Default to **single-file FBA**; only convert on explicit request.
* Keep directives (`#:sdk`, `#:property`, `#:package`) at the top.
* If the file grows, add `Directory.Build.props` for shared MSBuild—but the FBA file remains authoritative.

---

## 14) Project Conversion (on request)

* Convert when asked:

  ```bash
  dotnet project convert wpf.cs -o WpfProject
  ```
* Preserve the original FBA; mirror properties and pinned packages.

---

## 15) Agent Execution Compatibility

* Don’t block with `Console.ReadLine()`/`ReadKey()`.
* Terminate deterministically (`Application.Current.Shutdown()` or close MainWindow) so agents can capture outputs/exit codes.

---

## 16) Review Checklist

* [ ] `TargetFramework=net10.0-windows`, `UseWPF=True`, `PublishAot=False`, `Nullable=enable`
* [ ] **Apartment state set: `Unknown` → `STA` (two-step)**
* [ ] Top-level bootstrap before types; DI host configured
* [ ] Bindings use `nameof`; TwoWay only where necessary; validation strategy chosen
* [ ] Dispatcher marshaling respected; no blanket `ConfigureAwait(false)` in UI paths
* [ ] Resources/styles centralized; DPI/contrast respected
* [ ] Single-file FBA; exact package versions pinned for any dependencies

---

## 17) MCP Integration (NuGet & Docs)

* **Never guess versions.** Resolve with your **`nuget`** MCP server and pin with `#:package PackageId@X.Y.Z`.
* Use **`microsoft_learn`** MCP to confirm current WPF/MVVM guidance, DPI, accessibility, and deployment patterns.
