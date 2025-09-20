# GitHub Copilot Instructions — MVC File-based App (FBA) Template

> Opinionated, execution-ready guidance for building **ASP.NET Core MVC** (with optional Razor Pages) as a **single C# file** on **.NET 10**.

---

## 1) Project Overview

This template demonstrates a full MVC stack using the **File-based App (FBA)** model: keep build/runtime directives at the very top of one `.cs` file, place top-level program code first, then any type declarations. It supports Controllers/Views, static files, Razor Pages, DI, EF Core, and common production middleware.

---

## 2) Quick Start

```bash
# Create an MVC FBA file
cat > mvc.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=net10.0          # cross-platform; use net10.0-windows only if you truly need Windows-only APIs
#:property RootNamespace=MyApp
#:property PublishAot=False                  # Razor views/filters typically incompatible with AOT; keep off unless fully verified
#:property Nullable=enable

// --- Top-level program must precede any type declarations ---
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();                 // optional
builder.Services.AddResponseCompression();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware (prod-first order)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();      // if configured
app.UseAuthorization();

// Conventional routes + Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();          // optional
app.MapHealthChecks("/health/live");

await app.RunAsync();

// --- Minimal sample MVC artifacts (inline) ---
namespace MyApp.Controllers;

public sealed class HomeController() : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
CS

# Run (explicit invocation is agent-friendly)
dotnet run mvc.cs
```

> **Shebang** must be line 1 on Unix (`chmod +x mvc.cs && ./mvc.cs`).
> Keep all `#:sdk`, `#:property`, and `#:package` directives **at the top**.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet`
* **SDK**: `#:sdk Microsoft.NET.Sdk.Web`
* **Properties**:

  * `TargetFramework=net10.0` (prefer cross-platform; use `net10.0-windows` only for Windows-specific APIs)
  * `RootNamespace=MyApp`
  * `Nullable=enable`
  * `PublishAot=False` (MVC with Razor views is generally **not** AOT-friendly)
* **Packages**: pin with `#:package PackageId@ExactVersion` only when needed.

---

## 4) MVC Architecture & Pipeline

* **STANDARD**: `var builder = WebApplication.CreateBuilder(args);`
* **Top-level rule**: Executable statements **before** any type declarations.
* Services to consider:

  * `AddControllersWithViews()` (always for MVC)
  * `AddRazorPages()` (optional)
  * `AddAntiforgery()`, `AddDataProtection()`, `AddAuthentication()/AddAuthorization()` as needed
  * `AddProblemDetails()` for standardized errors
* Middleware order (prod-first):

  1. `UseExceptionHandler` / `UseDeveloperExceptionPage`
  2. `UseHsts` (prod)
  3. `UseHttpsRedirection`
  4. Static files: `UseStaticFiles`
  5. `UseRouting`
  6. `UseAuthentication` → `UseAuthorization`
  7. Endpoints: `MapControllerRoute`, `MapRazorPages`, health checks

Default route:
`{controller=Home}/{action=Index}/{id?}`

---

## 5) Modern C# (C# 12/13)

* Enable **nullable**; prefer **file-scoped namespaces** as the file grows.
* Use primary constructors/`required`/`init` when they clarify intent.
* Use raw string literals for large HTML snippets, policy JSON, etc.
* Pattern matching (switch/property/tuple/list) for concise validation and branching.

---

## 6) Async & I/O

* ASP.NET Core doesn’t run under a UI context; **don’t** add `.ConfigureAwait(false)` mechanically. Use it only in **library** code that might be consumed by contexts that capture.
* If there’s no `await`, return `Task.CompletedTask`/`ValueTask.CompletedTask`.
* Prefer `IAsyncEnumerable<T>` only for genuine streaming scenarios.

---

## 7) Controllers, Models, Views, Razor Pages

* **Controllers**: keep thin; inject services; return correct `ActionResult` types.
* **Models**: POCOs with validation attributes; use ViewModels for view-specific shaping; DTOs for API.
* **Views**: strongly-typed; partials for reuse; Layout for consistent shell; tag helpers for form/links.
* **Razor Pages**: pragmatic for CRUD; use PageModels for non-trivial logic.

---

## 8) Security

* **AuthN/Z**: configure schemes and policies; apply `[Authorize]` at controller/action level.
* **CSRF**: antiforgery tokens for unsafe verbs in forms.
* **Headers**: HSTS in prod; consider CSP, X-Content-Type-Options, X-Frame-Options; prefer HTTPS everywhere.
* **Input**: validate/sanitize; validate file uploads (size, type, path).

---

## 9) Performance

* View pre-compilation and response compression where beneficial.
* Cache static files (with cache-control), consider CDN.
* EF Core: async I/O, connection pooling, compiled queries for hot paths.
* Keep allocations low in hot controllers; use pooled `ArrayBufferWriter<T>`/`Utf8JsonWriter` when appropriate.

---

## 10) Configuration & Options

* Options pattern with `IOptions<T>` / `IOptionsMonitor<T>`.
* Environment overlays (Development/Staging/Production).
* **User Secrets** for local secrets; vault for non-dev.
* Structured logging; add OpenTelemetry if you need tracing/metrics.

---

## 11) Testing

* **Unit**: controller actions with mocked services; model validation tests.
* **Integration**: `WebApplicationFactory` for end-to-end pipeline (auth, filters, views).
* **View**: verify model binding and key HTML fragments; component/partial testing where it adds value.

---

## 12) Client-side & Assets

* Use bundling/minification (e.g., build pipeline) and cache busting.
* Progressive enhancement: unobtrusive JS; graceful degradation.
* AJAX for dynamic fragments; respect anti-forgery on POSTs.

---

## 13) Containerization & Ops

* Multi-stage Docker builds; small runtime images; health probes.
* Graceful shutdown (SIGTERM/SIGINT honored by Kestrel).
* Observability: health checks, structured logs, metrics, tracing.
* Static file volume/caching strategy for CDNs and proxies.

---

## 14) .NET Version Requirements

* **MANDATORY**: `TargetFramework=net10.0` (or `net10.0-windows` only when truly needed).
* **PROHIBITED**: Don’t add dependencies that require lower TFMs.
* Prefer latest C# features compatible with .NET 10.

---

## 15) MCP Integration (NuGet & Learn)

**Package Version Management**

* **CRITICAL**: Never guess versions.
* **MANDATORY**: Use the **`nuget` MCP server** to pin exact versions for any additional packages (e.g., EF Core, Identity, FluentValidation, Serilog).
* **MVC-SPECIFIC**: Ensure ASP.NET Core packages are compatible with .NET 10.

**Microsoft Guidance**

* Use **`microsoft_learn` MCP** to confirm current MVC/ASP.NET Core guidance, security and deployment best practices.

---

## 16) Running FBA

* Recommended: `dotnet run mvc.cs -- <args>`.
* Shebang allows `./mvc.cs` on Unix.
* FBA runs **without** a project file.

---

## 17) File-based Development Standards

* Default to **single-file FBA** unless a traditional project is explicitly requested.
* Keep directives (`#:sdk`, `#:property`, `#:package`) **at the top**.
* If the file grows large, you may introduce a **`Directory.Build.props`** for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 18) Project Conversion (On Request)

* Convert to a traditional project **only** when explicitly asked.
* Use the official CLI verb when available; emit to a separate directory and **preserve** the original FBA.
* Re-pin packages and mirror properties after conversion.

---

## 19) Agent Execution Compatibility

* Don’t add `Console.ReadLine()`/`ReadKey()` to block termination.
* MVC web apps **intentionally** run until stopped; keep outputs deterministic and logs structured for agents.

---

### Review Checklist

* [ ] `TargetFramework=net10.0` (or `net10.0-windows` only when required), `Nullable=enable`
* [ ] AOT **disabled**; Razor view pipeline works end-to-end
* [ ] Top-level code precedes types; MVC services + routing configured
* [ ] Production-first middleware order; HTTPS/HSTS/exception handling appropriate
* [ ] Conventional route and (optionally) Razor Pages mapped
* [ ] Security: antiforgery, authZ policies, headers as needed
* [ ] Packages (if any) pinned via MCP **`nuget`**; no TFM downgrades
* [ ] Deterministic outputs, health checks, graceful shutdown
