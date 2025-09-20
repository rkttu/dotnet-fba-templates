# GitHub Copilot Instructions — .NET Aspire File-based App (FBA) Template

> Opinionated, execution-ready guidance for building distributed apps with **.NET 10** + **Aspire** in a **single C# file**.

---

## 1) Project Overview

This template showcases **.NET Aspire** hosting patterns using the **File-based App (FBA)** model. You declare build/runtime metadata with directives at the top of a single `.cs` file, then compose distributed resources (containers, services, parameters, secrets) with the Aspire builder.

---

## 2) Quick Start

```bash
# Create an Aspire FBA file
cat > apphost.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Aspire.AppHost.Sdk
#:package Aspire.Hosting.AppHost@<pin-exact-version>
#:property TargetFramework=net10.0
#:property PublishAot=False
#:property Nullable=enable
#:property UserSecretsId=<GUID>  # use `dotnet user-secrets init` to generate

// --- Top-level program (must precede type declarations) ---
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Example: parameter + secret (dev-time convenient; don't hardcode secrets)
var dbPassword = builder.AddParameter("db:password", secret: true);

// Container with endpoint, volume, and dependency wiring
var postgres = builder
    .AddContainer("db", "postgres:16")
    .WithEnvironment("POSTGRES_PASSWORD", dbPassword)
    .WithVolume("pgdata", "/var/lib/postgresql/data")
    .WithEndpoint(containerPort: 5432, hostPort: 5432, name: "postgres")
    .WithHealthCheck("/health", interval: TimeSpan.FromSeconds(15))   // if image supports it
    .WaitFor(dbPassword);

// Add your app(s) and connect them to resources
var api = builder
    .AddProject("api", path: "./src/Api")       // or .AddContainer(...) for external services
    .WithReference(postgres)
    .WithEndpoint(8080, name: "http");

// Optional in-memory config for local dev overlays
builder.Configuration.AddInMemoryCollection(new[]
{
    new KeyValuePair<string,string?>("FeatureFlags:Experimental", "true"),
});

// Build and run (blocks until Ctrl+C / SIGINT)
var app = builder.Build();
await app.RunAsync();
CS

# Run (explicit invocation is agent-friendly)
dotnet run apphost.cs -- --help
```

> Keep all `#:sdk`, `#:package`, and `#:property` directives **at the top** of the file.
> On Unix, the shebang `#!/usr/bin/env dotnet` must be the first line to allow `chmod +x` + direct execution.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet`
* **SDK**: `#:sdk Aspire.AppHost.Sdk` (rarely needs a version pin; toolchains evolve)
* **Packages**: `#:package PackageId@ExactVersion` (pin **exact** versions)
* **Properties**: `#:property Name=Value` (e.g., `TargetFramework=net10.0`, `Nullable=enable`, `PublishAot=False`, `UserSecretsId=…`)

---

## 4) Aspire Program Structure

* **STANDARD**: `DistributedApplication.CreateBuilder(args)`
* **Top-level rule**: Executable statements **must** come before type declarations.
* Compose resources with **fluent** APIs (`AddContainer`, `AddProject`, `WithEndpoint`, `WithVolume`, `WaitFor`, etc.).
* Use **parameters** and **secrets** for sensitive values, not literals.
* After `app.RunAsync()`, the process runs until a termination signal (Ctrl+C / SIGINT).

---

## 5) Container & Resource Management

* **Containers**: `builder.AddContainer(name, image)`; set env via `.WithEnvironment()`.
* **Endpoints**: Expose ports via `.WithEndpoint(containerPort, hostPort, name)`.
* **Volumes**: Persist state with `.WithVolume(name, mountPath)`.
* **Dependencies**: Model startup order with `.WaitFor(...)`.
* **Networking**: Aspire links resources for you; name endpoints meaningfully.
* **Parameters/Secrets**: `builder.AddParameter("key", secret: true)`; supply values via User-Secrets, env vars, or CI secrets.

---

## 6) Modern C# (C# 12/13)

* Use **nullable** reference types and file-scoped namespaces.
* Prefer **primary constructors**, **raw string literals**, **required/init** members when they clarify intent.
* Pattern matching: switch/property/tuple/list patterns for concise domain logic.

---

## 7) Async & Streaming

* In console/FBA apps there’s no UI context—**do not** mechanically append `ConfigureAwait(false)` everywhere. Use it **only** in reusable libraries that may run under a capturing context.
* Return `Task.CompletedTask` / `ValueTask.CompletedTask` when no `await` is used.
* Use `IAsyncEnumerable<T>` for streaming workloads; propagate **CancellationToken**.

---

## 8) Code Organization

* Group related resource declarations together; use consistent, descriptive names for containers, endpoints, and volumes.
* Keep configuration in one place; overlay with in-memory config for dev only.
* Add comments where resource coupling isn’t obvious.

---

## 9) Observability & Resilience

* Use structured logging (`Microsoft.Extensions.Logging`) with meaningful levels.
* Add health checks for critical services (image or sidecar-based).
* Implement graceful degradation when external dependencies are unavailable.
* Keep STDOUT deterministic for automation; send diagnostics to STDERR or logs.

---

## 10) Security

* **Never** hardcode secrets; use **User-Secrets** locally and a vault (e.g., **Key Vault**) in non-dev.
* Use least-privilege for container permissions and network exposure.
* Prefer HTTPS and secure defaults for all externally reachable services.

---

## 11) .NET Version Requirements

* **MANDATORY**: Target **.NET 10** (`TargetFramework=net10.0`).
* **PROHIBITED**: No packages requiring lower target frameworks.
* Verify compatibility before adding dependencies; prefer the newest C# features.

---

## 12) Performance Notes

* **AOT disabled by default** for Aspire; enable only when it demonstrably helps.
* Use connection pooling; dispose `IDisposable`/`IAsyncDisposable`.
* Monitor resource usage; plan for **scaling** (horizontal where possible).

---

## 13) Testing Recommendations

* **Integration-first** mindset for distributed graphs:

  * Validate container startup, wiring (`WaitFor`), and endpoint reachability.
  * Test failover/health-check behavior.
* Use **TestContainers** (or equivalent) for reproducible external dependencies.
* Add configuration overlay tests (env, secrets, command-line).

---

## 14) MCP Server Integration

**Package Version Management**

* **CRITICAL**: Do **not** guess versions.
* **MANDATORY**: Query the **`nuget` MCP server** for exact **stable** versions of Aspire and related packages, then pin them in `#:package` directives.
* Confirm **Aspire package compatibility** as versions are interrelated.

**Microsoft Technology Research**

* Use the **`microsoft_learn` MCP server** to pull current Aspire guidance, patterns, and tutorials.
* Align implementations with official recommendations.

---

## 15) Running FBA Apps

* Recommended: `dotnet run apphost.cs -- <args>` for clarity and agent pipelines.
* Ensure the shebang line exists for Unix direct execution.
* FBA should run **without** a project file.

---

## 16) File-based Development Standards

* Default to **single-file FBA** unless the user requests a traditional project.
* Keep all dependencies and properties via **FBA directives** at the top.
* If the file becomes too large, you may introduce a **`Directory.Build.props`** for shared MSBuild settings—**but the FBA file remains authoritative**.

---

## 17) Project Conversion (On Request)

* Convert to a traditional project **only when explicitly requested**.
* Use the official CLI conversion verb **when available**; write output to a separate directory and **preserve the original FBA**.
* Re-pin packages and migrate properties post-conversion.

> Tooling evolves—prefer built-in commands over ad-hoc scripts, and document the exact verb/version used.

---

## 18) Agent Execution Compatibility

* **Do not** add artificial blockers like `Console.ReadLine()`/`ReadKey()` at the end.
* Let the process exit naturally so agents can capture STDOUT/STDERR and exit codes.
* Avoid unnecessary delays; terminate promptly once objectives are met.

---

## 19) Minimal Aspire FBA Skeletons

**Container + API wiring**

```csharp
#!/usr/bin/env dotnet
#:sdk Aspire.AppHost.Sdk
#:package Aspire.Hosting.AppHost@<pin-exact-version>
#:property TargetFramework=net10.0
#:property Nullable=enable
#:property PublishAot=False

using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Secrets/params
var apiKey = builder.AddParameter("api:key", secret: true);

// External dependency
var redis = builder
    .AddContainer("redis", "redis:7")
    .WithEndpoint(6379, name: "redis")
    .WithVolume("redisdata", "/data")
    .WaitFor(apiKey);

// App (project or container) depending on redis
var api = builder
    .AddProject("api", "./src/Api")
    .WithReference(redis)
    .WithEndpoint(8080, name: "http")
    .WithEnvironment("API_KEY", apiKey);

// Build & run
await builder.Build().RunAsync();
```

**Parameterized Postgres (env overlay-friendly)**

```csharp
#!/usr/bin/env dotnet
#:sdk Aspire.AppHost.Sdk
#:package Aspire.Hosting.AppHost@<pin-exact-version>
#:property TargetFramework=net10.0

using Aspire.Hosting;

var b = DistributedApplication.CreateBuilder(args);

var user = b.AddParameter("db:user", defaultValue: "app");
var pwd  = b.AddParameter("db:password", secret: true);
var name = b.AddParameter("db:name", defaultValue: "appdb");

var pg = b.AddContainer("pg", "postgres:16")
    .WithEnvironment("POSTGRES_USER", user)
    .WithEnvironment("POSTGRES_PASSWORD", pwd)
    .WithEnvironment("POSTGRES_DB", name)
    .WithEndpoint(5432, name: "postgres")
    .WithVolume("pgdata", "/var/lib/postgresql/data")
    .WaitFor(pwd);

await b.Build().RunAsync();
```

---

### Review Checklist

* [ ] `TargetFramework=net10.0`, `Nullable=enable`, **no** stray blocking calls
* [ ] **All packages pinned** to exact versions (via **`nuget` MCP** lookup)
* [ ] Secrets via parameters/User-Secrets; no hardcoded sensitive data
* [ ] Resources have clear names; endpoints/volumes are explicit
* [ ] Health checks and dependency order modeled (`WaitFor`)
* [ ] AOT intentionally disabled (or justified) for Aspire
* [ ] Deterministic output; proper exit codes; natural termination
