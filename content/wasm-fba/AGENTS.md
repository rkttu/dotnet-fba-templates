# GitHub Copilot Instructions — WebAssembly File-based App (FBA) Template

> Practical, execution-ready guidance for building **.NET 10 WebAssembly** (Blazor WebAssembly) apps as a **single C# file**, with modern JS interop.

---

## 1) Project Overview

This template targets **client-side .NET WebAssembly**. You keep build/runtime directives at the very top of a single `.cs` file (FBA), write top-level program code first, and place type declarations below. Use **JS interop** for DOM and browser APIs.

---

## 2) Quick Start (minimal, JS interop ready)

```bash
# Create the app
cat > wasm.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.BlazorWebAssembly
#:property TargetFramework=net10.0
#:property Nullable=enable
#:property RunAOTCompilation=False          # WASM uses its own AOT switch; enable when needed
#:property AllowUnsafeBlocks=False

// --- Top-level program must precede type declarations ---
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Runtime.InteropServices.JavaScript;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Optional: register services
builder.Services.AddLogging();

var app = builder.Build();

// Example: call into JS after startup using IJSRuntime
var js = app.Services.GetRequiredService<IJSRuntime>();
await js.InvokeVoidAsync("dom.setInnerText", "#app", "Hello from .NET WASM!");

await app.RunAsync();

// --- Types, exports, and imports follow ---

// Import a JS function from main.js ES module
partial class Dom
{
    [JSImport("dom.setInnerText", "main.js")]
    internal static partial void SetInnerText(string selector, string content);
}

// Export a .NET method callable from JS
public static partial class WasmExports
{
    [JSExport]
    public static int Add(int a, int b) => a + b;
}
CS
```

> You’ll also need a **`wwwroot/index.html`** and a **`wwwroot/main.js`** (ES module) beside your FBA file. Example scaffolds are below.

**`wwwroot/index.html` (example)**

```html
<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <title>WASM FBA</title>
    <base href="/" />
  </head>
  <body>
    <div id="app">loading…</div>
    <script type="module" src="main.js"></script>
    <script type="module">
      import { dotnet } from './_framework/dotnet.js';
      await dotnet.create().run();
    </script>
  </body>
</html>
```

**`wwwroot/main.js` (example)**

```js
// Exported API consumed by [JSImport]
export const dom = {
  setInnerText: (selector, content) => {
    const el = document.querySelector(selector);
    if (el) el.innerText = content;
  }
};

// Import a .NET export and use it
const { getAssemblyExports, getConfig } = await globalThis.getDotnetRuntime();
const cfg = getConfig();
const asm = await getAssemblyExports(cfg.mainAssemblyName);
console.log("Add(2,3) =", asm.WasmExports.Add(2, 3));
```

Run with:

```bash
dotnet run wasm.cs
```

---

## 3) FBA Directive Cheatsheet

* **Shebang** (Unix): `#!/usr/bin/env dotnet` (line 1 to allow `./wasm.cs`)
* **SDK**: `#:sdk Microsoft.NET.Sdk.BlazorWebAssembly`
* **Properties**:

  * `TargetFramework=net10.0`
  * `Nullable=enable`
  * `RunAOTCompilation=False` (turn **on** only after measuring benefits)
  * `AllowUnsafeBlocks` only when truly needed
* Keep all directives **at the top**.

---

## 4) JavaScript Interop (modern, ES modules)

* **Import from JS**: `[JSImport("symbol", "main.js")]` in a `partial` type.
* **Export to JS**: `[JSExport]` static method; load via `getAssemblyExports()` in JS.
* Prefer **ES modules**; keep your interop surface **small and explicit**.
* For DOM work, do it **in JS**, not C#, and call via interop to minimize marshaling.

**Pattern**

```csharp
[JSImport("mod.fn", "module.js")]
internal static partial int Fn(int x);

[JSExport]
public static string Echo(string s) => s;
```

---

## 5) Browser API Integration

* Use JS interop for: DOM manipulation, Clipboard, File System Access, Notifications, etc.
* Handle **permissions** and feature detection in JS; surface simple results to .NET.
* Wrap interop calls with minimal DTOs; avoid passing large graphs frequently.

---

## 6) Asset Management

* Place static assets under **`wwwroot/`** (HTML, JS, CSS, images).
* Use **cache-busting** filenames or a manifest when shipping to production.
* Keep the initial payload small (defer heavy modules; dynamic `import()` in JS).

---

## 7) Modern C# (C# 12/13)

* **Nullable** on; prefer file-scoped namespaces as the codebase grows.
* Use primary constructors / `required` / `init` members where they clarify intent.
* Raw string literals for HTML/JS snippets embedded in tooling or tests.

---

## 8) Async/Await Guidance (UI context caveat)

* **Do not blanket use** `ConfigureAwait(false)` in Blazor WASM UI paths—continuations often need the **captured context** to update UI.
* Use `ConfigureAwait(false)` only in **library** code that does not interact with UI and benefits from context-free continuations.
* If no `await`, return `Task.CompletedTask`/`ValueTask.CompletedTask`.

---

## 9) State & Components

* Keep **state** immutable where feasible; centralize updates (simple store pattern).
* Coalesce interop calls—prefer **batched** updates over chattier per-element changes.
* Extract reusable JS and C# helpers; prefer **partial classes** for interop separation.

---

## 10) Performance

* Minimize **interop frequency** and payload size.
* Use **AOT** (`RunAOTCompilation=True`) only when it improves perf for your workload (measure size vs. speed).
* Avoid frequent DOM updates; batch in JS (e.g., use `DocumentFragment`).
* Trim ICU/data sets if not needed; enable **compression** on hosting.

---

## 11) Memory Management

* Avoid long-lived references across interop boundaries.
* Clean up event handlers in JS; use **AbortController** and remove listeners on dispose.
* For large data, prefer **binary** or **SharedArrayBuffer** strategies over JSON blobs.

---

## 12) Compatibility

* Test on modern **Chromium**, **Firefox**, **Safari** (desktop & mobile).
* Feature-detect; provide **progressive enhancement** or fallbacks.
* Keep CSP strict; allow only what you need (see Security below).

---

## 13) Security

* Validate all user input in JS and .NET.
* Set a **Content Security Policy** that blocks inline script unless hashed.
* Use HTTPS; set appropriate security headers at the host/CDN.
* Never embed secrets client-side; use backend flows for sensitive operations.

---

## 14) Testing

* **Unit**: isolate business logic from interop; mock `IJSRuntime`.
* **Browser**: run Playwright/WebDriver for UI paths; test across browsers.
* **Integration**: verify startup, asset loading, module init, and error paths.

---

## 15) Deployment

* Enable **bundling/minification** of JS/CSS.
* Serve **`.wasm`** with correct MIME (`application/wasm`) and long-cache headers.
* Use **HTTP/2+** and a **CDN**; precompress (`br`, `gzip`) large artifacts.
* Consider **service worker** + manifest for PWA (offline & installable).

---

## 16) .NET Version Requirements

* **MANDATORY**: Target **.NET 10**.
* **PROHIBITED**: No dependencies requiring lower TFMs.
* Prefer latest C# features compatible with .NET 10.

---

## 17) MCP Integration (NuGet & Learn)

**Package Version Management**

* **Never guess versions.** Resolve exact versions with your **`nuget` MCP server** before adding any packages and pin them in `#:package`.

**Microsoft Learn**

* Use **`microsoft_learn` MCP** to confirm current Blazor WASM / interop guidance and performance recommendations.

---

## 18) Running FBA Apps

* Recommended: `dotnet run wasm.cs -- <args>`.
* Shebang enables `./wasm.cs` on Unix.
* FBA runs **without** a traditional project file.

---

## 19) File-based Development Standards

* Default to **single-file FBA** unless you explicitly need a traditional project.
* Keep directives (`#:sdk`, `#:property`, `#:package`) **at the top**.
* If the file grows, you may add **`Directory.Build.props`** for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 20) Project Conversion (on request)

* Convert to a traditional project **only** when explicitly requested.
* Emit to a separate directory and **preserve** the original FBA.
* Re-pin packages and port build properties.

---

## 21) Agent Execution Compatibility

* Don’t add `Console.ReadLine()`/`ReadKey()` to block termination.
* Web apps intentionally run until stopped; keep logs deterministic.

---

## 22) Scaffolds (copy-paste)

**Interop call from .NET to JS**

```csharp
[JSImport("dom.setInnerText", "main.js")]
internal static partial void SetInnerText(string selector, string content);

// usage:
// Dom.SetInnerText("#status", "Ready");
```

**Interop call from JS to .NET**

```csharp
public static partial class Api
{
    [JSExport] public static string Version() => "1.0.0";
}

// JS:
// const { getAssemblyExports, getConfig } = await getDotnetRuntime();
// const asm = await getAssemblyExports(getConfig().mainAssemblyName);
// console.log("Version:", asm.Api.Version());
```

---

### Review Checklist

* [ ] `Microsoft.NET.Sdk.BlazorWebAssembly`, `TargetFramework=net10.0`, `Nullable=enable`
* [ ] Top-level program **before** any type declarations
* [ ] JS interop via `[JSImport]` / `[JSExport]` with ES modules
* [ ] Minimal interop frequency; DOM updates batched in JS
* [ ] **No blanket** `ConfigureAwait(false)` in UI code
* [ ] Security headers/CSP considered; no client-side secrets
* [ ] Proper MIME + caching for `.wasm` and static assets
* [ ] Packages (if any) pinned via MCP **`nuget`**; no TFM downgrades
