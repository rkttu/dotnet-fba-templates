# GitHub Copilot Instructions — Minimal API File-based App (FBA) Template

> Opinionated, execution-ready guidance for building **.NET 10** Minimal APIs as a **single C# file**.

---

## 1) Project Overview

This template shows how to author a lightweight **ASP.NET Core Minimal API** using the **File-based App (FBA)** model: keep directives at the top of one `.cs` file, place top-level program code first, then any type declarations.

---

## 2) Quick Start

```bash
# Create a Minimal API FBA
cat > api.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=net10.0
#:property PublishAot=False
#:property Nullable=enable

// --- Top-level program must precede type declarations ---
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuration & logging (extend as needed)
builder.Configuration.AddEnvironmentVariables(prefix: "API_");
builder.Logging.AddSimpleConsole();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();                 // standardized errors
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("default", o => { o.Window = TimeSpan.FromSeconds(1); o.PermitLimit = 20; }));
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ITimeProvider, SysTimeProvider>();

var app = builder.Build();

// Middleware order (security -> diagnostics -> docs -> routing)
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseExceptionHandler();                            // pairs with ProblemDetails
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal endpoints
var v1 = app.MapGroup("/api/v1").WithTags("v1");

v1.MapGet("/", () => Results.Ok(new { ok = true }))
  .WithName("Root")
  .WithSummary("Healthy ping")
  .WithDescription("Returns a simple health indicator.")
  .Produces(200);

v1.MapGet("/time", (ITimeProvider clock) => Results.Ok(new { now = clock.Now }))
  .WithName("GetTime")
  .WithSummary("Returns server time")
  .Produces(200);

v1.MapPost("/users", (UserDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.ValidationProblem(new() { ["name"] = ["Required"] });
    var created = dto with { Id = Guid.NewGuid() };
    return Results.Created($"/api/v1/users/{created.Id}", created);
})
.WithName("CreateUser")
.Produces<UserDto>(201)
.ProducesValidationProblem();

// Liveness
app.MapHealthChecks("/health/live");

// Run (blocks until Ctrl+C)
await app.RunAsync();

// --- Types follow ---
record UserDto(Guid? Id, string Name);

interface ITimeProvider { DateTimeOffset Now { get; } }
sealed class SysTimeProvider : ITimeProvider { public DateTimeOffset Now => DateTimeOffset.UtcNow; }
CS

# Run (agent-friendly explicit invocation)
dotnet run api.cs
```

> On Unix, keep the shebang as the **first** line to allow `chmod +x api.cs && ./api.cs`.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet`
* **SDK**: `#:sdk Microsoft.NET.Sdk.Web`
* **Properties**: `TargetFramework=net10.0`, `Nullable=enable`, `PublishAot=False` (enable AOT only after verifying compatibility)
* **Packages**: Prefer built-in BCL; pin external packages with `#:package PackageId@ExactVersion` (use MCP `nuget` server to resolve exact versions)

---

## 4) Application Builder Pattern

* **STANDARD**: `var builder = WebApplication.CreateBuilder(args);`
* Configure **services** (`builder.Services`) and **middleware** order carefully.
* Build the app with `var app = builder.Build();`
* `app.Run()`/`RunAsync()` **blocks**; terminate with Ctrl+C / SIGINT when objectives are met.
* **Top-level rule**: Executable statements **must** come before type declarations.

---

## 5) Configuration Setup

* Add env vars: `builder.Configuration.AddEnvironmentVariables("API_")`.
* Logging: `builder.Logging.AddSimpleConsole()`.
* Bind strongly-typed settings for clarity (e.g., `builder.Services.Configure<TOptions>(builder.Configuration.GetSection("Section"))` with `app.MapGet(..., (IOptions<TOptions> o) => ...)`).

---

## 6) Endpoint Mapping

* Use `MapGet/MapPost/MapPut/MapDelete/MapPatch`.
* Prefer **route groups** for versioning/prefixes.
* Keep lambdas small; move complex logic into methods or services.
* Return standardized results via `Results` or typed results (`Ok<T>`, `Created<T>`, etc.).

**Patterns**

```csharp
app.MapGet("/", () => "Hello World!");
app.MapGet("/users/{id:guid}", (Guid id) => Results.Ok(new { id }));
app.MapPost("/users", (UserDto u) => Results.Created($"/users/{Guid.NewGuid()}", u));
```

---

## 7) Modern C# (C# 12/13)

* Enable **nullable**; consider **file-scoped namespaces** as the file grows.
* Use **primary constructors**, **raw string literals**, **required/init** members where useful.
* Pattern matching (switch/property/tuple/list) for concise request validation.

---

## 8) Async & Streaming

* Minimal API apps don’t run under a UI context—**don’t** sprinkle `ConfigureAwait(false)` mechanically. Use it **only** in libraries that might be used under a capturing context.
* If no `await` is needed, return `Task.CompletedTask`/`ValueTask.CompletedTask`.
* Use `IAsyncEnumerable<T>` thoughtfully for streaming responses (and prefer `WriteAsJsonAsync` piping).

---

## 9) Organization & DI

* Group endpoints by **domain** or **version** with `MapGroup`.
* Register services with sensible lifetimes (Transient/Scoped/Singleton).
* Keep I/O at the edges; isolate business rules for testability.

---

## 10) Middleware

Recommended baseline order:

1. `UseHttpsRedirection()`
2. **Security** (authN/Z, rate limiting)
3. **Diagnostics** (`UseExceptionHandler()` + `AddProblemDetails()`)
4. **Docs** (`UseSwagger`/`UseSwaggerUI` in dev)
5. **Routing/Endpoints**

---

## 11) Performance

* **AOT**: Off by default; enable when startup perf matters and reflection is controlled. Prefer **source generators** (e.g., `System.Text.Json` context) to avoid runtime reflection.
* **Caching**: Response caching / memory cache / distributed cache as appropriate.
* **Serialization**: Tune `JsonSerializerOptions`; consider source-generated JSON for hot paths.
* Dispose `IAsyncDisposable`/`IDisposable` properly.

---

## 12) Security

* **Authentication/Authorization**: Add schemes and policies; protect endpoints with `RequireAuthorization()`.
* **Validation**: Data annotations + custom validators; sanitize inputs.
* **Transport**: Enforce HTTPS/HSTS in production; set strict TLS.
* **Throttling**: Add `RateLimiter` for abusive patterns.
* **Secrets**: Never hardcode; use environment variables or secret stores.

---

## 13) API Documentation

* Add OpenAPI/Swagger (as in the quick start).
* Use `WithName`, `WithTags`, `WithSummary`, `WithDescription`.
* Document error responses (`ProducesValidationProblem`, `ProducesProblem`).

---

## 14) Testing

* **Unit**: isolate handler logic and mock dependencies.
* **Integration**: `WebApplicationFactory` to exercise real pipeline.
* Validate auth flows, model binding, filters, and error formatting.

---

## 15) Deployment

* Environment configs per stage; health probes at `/health/*`.
* Structured logging; metrics & tracing (OpenTelemetry if needed).
* Graceful shutdown (default ASP.NET Core hosting honors SIGTERM/SIGINT).

---

## 16) Containerization

* Use multi-stage Docker builds; trim runtime images.
* Add container **health checks** and resource limits.
* Ensure graceful shutdown signals are propagated.

---

## 17) .NET Version Requirements

* **MANDATORY**: `TargetFramework=net10.0`.
* **PROHIBITED**: No dependencies requiring lower TFMs.
* Prefer latest C# features compatible with .NET 10.

---

## 18) MCP Server Integration (NuGet & Learn)

**Package Version Management**

* **CRITICAL**: Do **not** guess versions.
* **MANDATORY**: Query the **`nuget` MCP server** to pin exact stable versions for any added packages (e.g., Swagger, validators).

**Microsoft Technology Research**

* Use **`microsoft_learn` MCP** to confirm current ASP.NET Core guidance and Minimal API patterns.

---

## 19) Running FBA Apps

* Recommended: `dotnet run api.cs -- <args>`.
* Shebang allows `./api.cs` on Unix.
* FBA runs **without** a project file.

---

## 20) File-based Development Standards

* Default to **single-file** FBA unless a project is explicitly requested.
* Keep all directives (`#:property`, `#:sdk`, `#:package`) **at the top**.
* If the file grows, you may add **`Directory.Build.props`** for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 21) Project Conversion (On Request)

* Convert to a traditional project **only** when explicitly asked.
* Use an official CLI verb when available; write to a separate directory and **preserve** the original FBA.
* Re-pin packages and mirror properties post-conversion.

---

## 22) Agent Execution Compatibility

* **Do not** add terminal blockers like `Console.ReadLine()`/`ReadKey()`.
* Let the process exit naturally so agents can collect STDOUT/STDERR and exit codes.
* Web apps **intentionally** run until stopped; keep output deterministic.

---

## 23) Minimal Patterns

**Hello + Swagger + Health**

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=net10.0
#:property Nullable=enable
#:property PublishAot=False

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Text("Hello, world!"));
app.MapHealthChecks("/health/live");

await app.RunAsync();
```

**Route group + validation**

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=net10.0
#:property Nullable=enable

var b = WebApplication.CreateBuilder(args);
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();
var app = b.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

var users = app.MapGroup("/api/v1/users").WithTags("users");

users.MapPost("/", (User u) =>
{
    if (string.IsNullOrWhiteSpace(u.Name)) return Results.ValidationProblem(new() { ["name"] = ["Required"] });
    var created = u with { Id = Guid.NewGuid() };
    return Results.Created($"/api/v1/users/{created.Id}", created);
})
.Produces<User>(201)
.ProducesValidationProblem();

await app.RunAsync();

public sealed record User(Guid? Id, string Name);
```

---

### Review Checklist

* [ ] `TargetFramework=net10.0`, `Nullable=enable`; AOT choice is deliberate
* [ ] Top-level code before types; no stray terminal blockers
* [ ] Clear route groups, summaries, validation, and correct status codes
* [ ] ProblemDetails + exception handler; HTTPS, rate limiting as needed
* [ ] Packages (if any) are **pinned via MCP `nuget`**; no TFM downgrades
* [ ] Deterministic outputs; graceful shutdown; health endpoints
