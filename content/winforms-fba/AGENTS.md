# GitHub Copilot Instructions — Windows Forms File-based App (FBA) Template

> Opinionated, production-ready guidance for building **.NET 10** Windows Forms apps as a **single C# file**.

---

## 1) Project Overview

Author a full Windows Forms desktop app using the **File-based App (FBA)** model: keep build/runtime directives at the top of one `.cs` file; put top-level program code first; types follow.

---

## 2) FBA Directives (Windows-only GUI)

```csharp
// Shebang optional for Windows-only GUI apps

#:sdk Microsoft.NET.Sdk
#:property OutputType=WinExe
#:property TargetFramework=net10.0-windows
#:property UseWindowsForms=True
#:property UseWPF=False
#:property PublishAot=False        // WinForms is not AOT-compatible
#:property Nullable=enable
```

**Notes**

* Prefer `net10.0-windows` to light up Windows-only APIs.
* Keep directives at the very top.
* No packages are required to start; pin any added packages with exact versions.

---

## 3) Quick Start (single file, DI + proper WinForms init)

```csharp
// winforms.cs
// FBA directives go here (see section 2)

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// --- Top-level program must precede type declarations ---

// WinForms requires STA.
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Minimal host for DI/logging/config (modern .NET pattern)
using var host = Host.CreateApplicationBuilder(args)
    .ConfigureServices(s =>
    {
        s.AddLogging(b => b.AddDebug().AddEventLog().AddSimpleConsole());
        s.AddSingleton<MainForm>();                    // register forms in DI
        s.AddSingleton<IBannerService, BannerService>(); 
    })
    .Build();

// WinForms bootstrap
Application.OleRequired();
Application.EnableVisualStyles();
Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
Application.SetCompatibleTextRenderingDefault(false);

// Run via ApplicationContext for clean lifetime control
using var scope = host.Services.CreateScope();
var context = new SingleFormContext(scope.ServiceProvider.GetRequiredService<MainForm>());

Application.Run(context);

// --- Types follow here ---

sealed class SingleFormContext(Form main) : ApplicationContext(main)
{
    protected override void OnMainFormClosed(object? sender, EventArgs e)
    {
        base.OnMainFormClosed(sender, e);
        Application.ExitThread(); // deterministic shutdown
    }
}

public sealed class MainForm(IBannerService banner) : Form
{
    private readonly Button _btn = new() { Text = "Click me", AutoSize = true, Dock = DockStyle.Top };
    private readonly TextBox _log = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };

    public MainForm : this(null!) { } // designer compatibility (not used here)

    public MainForm(IBannerService? _ = null) : this(new BannerService()) { } // FBA convenience

    public MainForm(IBannerService svc) : this()
    {
        Text = "WinForms FBA (.NET 10)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(640, 360);

        Controls.AddRange([ _log, _btn ]);
        _btn.Click += (_, __) => Append(svc.Banner());
        Append("Ready.");
    }

    private void Append(string line) => _log.AppendText(line + Environment.NewLine);
}

public interface IBannerService { string Banner(); }
public sealed class BannerService : IBannerService
{
    public string Banner() => $"Hello from DI at {DateTime.Now:O}";
}
```

**Run**

```powershell
dotnet run winforms.cs
```

---

## 4) Application Initialization (must-haves)

* **STA**: `Thread.CurrentThread.SetApartmentState(ApartmentState.STA)`.
* **OLE**: `Application.OleRequired()` if you touch Clipboard/Drag-Drop/etc.
* **Styles/DPI**: `EnableVisualStyles()`, `SetHighDpiMode(PerMonitorV2)`.
* **Text rendering**: `SetCompatibleTextRenderingDefault(false)`.

Use `ApplicationContext` for multi-form or background-work lifecycles.

---

## 5) Layout & Controls

* Prefer **Dock/Anchor** and layout panels (**TableLayoutPanel**, **FlowLayoutPanel**) over absolute sizes.
* Initialize via **object initializers** and **collection expressions** (`Controls.AddRange([ … ])`).
* Center main window with `StartPosition = CenterScreen`.

---

## 6) Events & Async

* Keep UI work on the UI thread; use `BeginInvoke/Invoke` when crossing threads.
* For long work: `await Task.Run(...)` with `CancellationToken` and progress updates.
* **Do not** sprinkle `ConfigureAwait(false)` in UI code; you usually need the captured context.

---

## 7) Data Binding

* Use `Control.DataBindings` for simple scenarios; validate via `IDataErrorInfo`/attributes.
* For larger apps, adopt MVP/MVVM-like separation for testability.

---

## 8) High-DPI

* Use `HighDpiMode.PerMonitorV2`.
* Avoid pixel constants; prefer layout containers and autosizing.
* Provide scalable icons/images (ICO with multiple sizes or vector where possible).

---

## 9) Threading & Resilience

* Use `async` handlers for I/O; report progress via `IProgress<T>` or UI updates.
* Catch exceptions in background tasks and surface friendly messages (not MessageBox storms).

---

## 10) Security & Reliability

* Validate all user inputs.
* Avoid storing secrets in config; use Windows DPAPI/credential stores if needed.
* Dispose `IDisposable` controls/resources (e.g., timers, streams).

---

## 11) Testing

* **Unit**: isolate business logic from UI; inject services.
* **UI**: optional automation (WinAppDriver/Playwright); verify keyboard navigation & accessibility.
* **Integration**: end-to-end workflows (startup → action → shutdown).

---

## 12) Deployment

* **ClickOnce** or MSIX/installer for distribution.
* Document **Windows version** and **.NET** requirements.
* Handle updates gracefully; preserve user settings.

---

## 13) Accessibility & UX

* Set `AccessibleName/Description` and tab order.
* Keyboard shortcuts (e.g., `&File`, `AcceptButton`, `CancelButton`).
* Clear validation errors and status feedback.

---

## 14) File-based Dev Standards

* Stay **single-file** unless asked to convert.
* If the file grows, add a `Directory.Build.props` for shared MSBuild settings—**FBA stays authoritative**.
* For conversion: `dotnet project convert winforms.cs` (only on explicit request; keep original).

---

## 15) Agent Execution Compatibility

* Don’t add `Console.ReadLine()`/`ReadKey()` blockers.
* Exit deterministically (`Application.ExitThread()` / `Application.Exit()` when your scenario completes).

---

## 16) Review Checklist

* [ ] `TargetFramework=net10.0-windows`, `UseWindowsForms=True`, `PublishAot=False`
* [ ] STA thread, OLE, styles, **PerMonitorV2** DPI
* [ ] Top-level code **before** types; DI wired via `Host.CreateApplicationBuilder`
* [ ] `ApplicationContext` for lifetime; main form centered; controls laid out with Dock/Anchor
* [ ] Long operations off UI thread; safe marshaling back to UI
* [ ] Accessibility, keyboard navigation, and proper error feedback
* [ ] Single-file FBA; no unnecessary packages; pinned versions if any are added

---

## 17) MCP Integration (NuGet & Docs)

* **Never guess package versions.** If you add packages (logging, settings, etc.), resolve exact versions via your **`nuget` MCP server** and pin them with `#:package PackageId@X.Y.Z`.
* Use **`microsoft_learn` MCP** to verify current WinForms DPI, accessibility, and deployment guidance.
