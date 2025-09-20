# GitHub Copilot Instructions — Windows Forms File-based App (FBA) Template

> Production-ready guidance for building **.NET 10** WinForms apps in a **single C# file**.
> **Note:** Per on-device verification, this template **requires the two-step apartment state init (`Unknown` → `STA`)**.

---

## 1) Overview

Build a full WinForms desktop app with the **File-based App (FBA)** model: directives at the top of one `.cs` file, top-level bootstrapping first, types below.

---

## 2) FBA Directives (Windows-only GUI)

```csharp
// Shebang optional for Windows-only apps

#:sdk Microsoft.NET.Sdk
#:property OutputType=WinExe
#:property TargetFramework=net10.0-windows
#:property UseWindowsForms=True
#:property UseWPF=False
#:property PublishAot=False
#:property Nullable=enable
```

**Notes**

* Use `net10.0-windows` to light up Windows APIs.
* Keep all directives at the very top.
* Pin any added packages with exact versions.

---

## 3) Quick Start (single file, DI + required WinForms init)

```csharp
// winforms.cs  (FBA directives above)

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// --- Top-level program must precede type declarations ---

// **CRITICAL (empirically required)**: two-step apartment initialization
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Minimal host for DI/logging/config
using var host = Host.CreateApplicationBuilder(args)
    .ConfigureServices(s =>
    {
        s.AddLogging(b => b.AddSimpleConsole());
        s.AddSingleton<MainForm>();
        s.AddSingleton<IBannerService, BannerService>();
    })
    .Build();

// WinForms bootstrap
Application.OleRequired(); // enables OLE features (Clipboard, DragDrop, etc.)
Application.EnableVisualStyles();
Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
Application.SetCompatibleTextRenderingDefault(false);

// Run via ApplicationContext for deterministic lifetime
using var scope = host.Services.CreateScope();
var context = new SingleFormContext(scope.ServiceProvider.GetRequiredService<MainForm>());
Application.Run(context);

// --- Types follow ---

sealed class SingleFormContext(Form main) : ApplicationContext(main)
{
    protected override void OnMainFormClosed(object? sender, EventArgs e)
    {
        base.OnMainFormClosed(sender, e);
        Application.ExitThread(); // deterministic shutdown
    }
}

public sealed class MainForm(IBannerService svc) : Form
{
    private readonly Button _btn = new() { Text = "Click me", AutoSize = true, Dock = DockStyle.Top };
    private readonly TextBox _log = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };

    public MainForm(IBannerService banner) : this()
    {
        Text = "WinForms FBA (.NET 10)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(640, 360);

        Controls.AddRange([ _log, _btn ]);
        _btn.Click += (_, __) => Append(banner.Banner());
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

## 4) Initialization (must-haves)

* **Apartment state (CRITICAL):**

  * `Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);`
  * `Thread.CurrentThread.SetApartmentState(ApartmentState.STA);`
  * Rationale: matches real-world behavior you validated; avoids intermittent COM issues.
* **OLE**: `Application.OleRequired()` when using Clipboard/Drag-Drop/Embedding (safe to call once).
* **Styles/DPI**: `EnableVisualStyles()` and `SetHighDpiMode(PerMonitorV2)`.
* **Text rendering**: `SetCompatibleTextRenderingDefault(false)`.

Use `ApplicationContext` to coordinate multi-form lifecycles.

---

## 5) Layout & Controls

* Prefer **Dock/Anchor** and layout panels (**TableLayoutPanel**, **FlowLayoutPanel**) over fixed sizes.
* Use object initializers and collection expressions (`Controls.AddRange([ … ])`).
* Center the main window (`StartPosition = CenterScreen`).

---

## 6) Events & Async

* Keep UI work on the UI thread; background work via `await Task.Run(...)` with `CancellationToken`.
* Marshal back with `Control.BeginInvoke/Invoke`.
* Don’t blanket-apply `ConfigureAwait(false)` in UI code.

---

## 7) Data Binding

* `Control.DataBindings` for simple cases; validation via `IDataErrorInfo` or data annotations.
* For larger apps, prefer MVP/MVVM-style separation for testability.

---

## 8) High DPI

* `HighDpiMode.PerMonitorV2`, avoid pixel constants, prefer autosizing.
* Provide multi-size `.ico` or vector assets.

---

## 9) Threading & Resilience

* Make long operations async; show progress (`IProgress<T>` or UI updates).
* Catch exceptions in background tasks; present friendly errors (no dialog storms).

---

## 10) Security & Reliability

* Validate inputs.
* Keep secrets out of config; use DPAPI/Windows credential stores.
* Dispose `IDisposable` controls/resources (timers, streams, etc.).

---

## 11) Testing

* **Unit**: isolate business logic; inject services.
* **UI**: optional automation (WinAppDriver/Playwright); verify keyboard navigation & accessibility.
* **Integration**: full flows (startup → action → shutdown).

---

## 12) Deployment

* ClickOnce/MSIX/installer—document Windows and .NET requirements.
* Graceful updates; preserve user settings.

---

## 13) Accessibility & UX

* Set `AccessibleName/Description`; correct tab order.
* Keyboard shortcuts (`&File`, `AcceptButton`, `CancelButton`).
* Clear, actionable validation errors.

---

## 14) File-based Dev Standards

* Stay **single-file** unless explicitly asked to convert.
* If the file grows, add `Directory.Build.props` (FBA file remains authoritative).
* Conversion on request: `dotnet project convert winforms.cs` (preserve original FBA).

---

## 15) Agent Execution Compatibility

* Don’t add `Console.ReadLine()`/`ReadKey()` blockers.
* Exit deterministically when done (`Application.ExitThread()` / `Application.Exit()`).

---

## 16) Review Checklist

* [ ] `TargetFramework=net10.0-windows`, `UseWindowsForms=True`, `PublishAot=False`
* [ ] **Apartment state set: `Unknown` → `STA`**
* [ ] Top-level bootstrapping before types; DI via `Host.CreateApplicationBuilder`
* [ ] `ApplicationContext` for lifetime; centered window; Dock/Anchor layout
* [ ] Long work off UI thread; safe marshaling back
* [ ] Accessibility and clear error feedback
* [ ] Single-file FBA; exact package versions pinned if any are added

---

### Callout

Your field result matters. This template **codifies** the two-step apartment initialization you confirmed so others get the same stable behavior.
