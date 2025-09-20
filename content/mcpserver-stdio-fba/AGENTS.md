# GitHub Copilot Instructions — MCP Server File-based App (FBA) Template

> Opinionated, execution-ready guidance for building **Model Context Protocol (MCP) servers** as a **single C# file** targeting **.NET 10**.

---

## 1) Project Overview

This template demonstrates how to build an **MCP server** that integrates with AI assistants and other MCP clients using the **File-based App (FBA)** model: directives at the top of one `.cs` file, top-level program first, types below.

---

## 2) Quick Start

```bash
# Create an MCP server FBA
cat > mcp.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package ModelContextProtocol@<pin-exact-version>
#:property TargetFramework=net10.0
#:property PublishAot=False            # reflection/DI typical; keep AOT off unless verified
#:property Nullable=enable

// --- Top-level program (must precede type declarations) ---
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// using ModelContextProtocol; // example namespace; adjust to actual package

var builder = Host.CreateApplicationBuilder(args);

// Configuration & logging
builder.Configuration.AddEnvironmentVariables(prefix: "MCP_");
builder.Logging.AddSimpleConsole();

// HTTP clients for tools
builder.Services.AddHttpClient();

// Register MCP server and transport
builder.Services
    .AddMcpServer(server =>
        server
            .WithStdioServerTransport()                     // stdio transport for editors/agents
            .WithTools(tools =>
            {
                tools.Add("ip-address", IpAddressTool)
                     .WithDescription("Get this machine's public IP address.");
            })
    );

using var host = builder.Build();
await host.RunAsync();

// --- Tool implementations below ---
[Description("Get the public IP address of this machine.")]
static async Task<string> IpAddressTool(IServiceProvider services,
    [Description("Return IPv6 instead of IPv4.")] bool ipv6 = false)
{
    var client = services.GetRequiredService<IHttpClientFactory>().CreateClient();
    var uri = ipv6 ? "https://api64.ipify.org" : "https://api.ipify.org";
    var ip = await client.GetStringAsync(uri);
    return ip.Trim();
}
CS

# Run (agent-friendly explicit invocation)
dotnet run mcp.cs
```

> Keep `#:sdk`, `#:package`, and `#:property` **at the top**.
> On Unix, `#!/usr/bin/env dotnet` must be the first line for `chmod +x mcp.cs && ./mcp.cs`.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet`
* **SDK**: `#:sdk Microsoft.NET.Sdk` (console host; Web SDK not required for stdio servers)
* **Packages**: `#:package ModelContextProtocol@ExactVersion` (pin **exact** version)
* **Properties**: `TargetFramework=net10.0`, `Nullable=enable`, `PublishAot=False` (toggle to `True` only after verifying compatibility)

---

## 4) Host Builder Pattern (MCP)

* Use **`Host.CreateApplicationBuilder(args)`** for DI, config, logging.
* Add configuration sources (env, JSON, command line) as needed.
* Register **HTTP clients** for outbound calls.
* Add MCP server via `.AddMcpServer(…)`, configure **transport** (e.g., `.WithStdioServerTransport()`), and **tools** with `.WithTools(…)`.
* **Top-level program first**; define tool types/methods **after** the top-level code.

---

## 5) MCP Server Configuration

* **Transport**: prefer **STDIO** for editor/agent compatibility.
* **Tools**: keep them **focused**; provide clear names & `[Description]` attributes for tools and parameters.
* **Factories**: when tools need services/config, accept `IServiceProvider` and resolve dependencies inside the tool.

**Tool signature pattern**

```csharp
[Description("Does something useful.")]
static async Task<ReturnType> ToolName(IServiceProvider services, /* params with [Description] */)
```

---

## 6) Modern C# Guidance (C# 12/13)

* Enable **nullable**; use **file-scoped namespaces** if/when adding namespaces.
* Prefer **raw string literals** for multi-line payloads; **required/init** members when appropriate.
* Pattern matching (switch/property/tuple/list) for concise request handling.

---

## 7) Async & Streaming

* Console/FBA apps have no UI context—**don’t** append `ConfigureAwait(false)` mechanically. Use it **only** in reusable libraries that might run under a capturing context.
* If a method contains no `await`, return `Task.CompletedTask`/`ValueTask.CompletedTask`.
* Use `IAsyncEnumerable<T>` for streaming results when it genuinely helps clients.

---

## 8) Error Handling & Observability

* Validate inputs; return clear error messages without leaking sensitive details.
* Log exceptions with context; keep STDOUT for protocol, STDERR/logs for diagnostics.
* Prefer structured logging via `Microsoft.Extensions.Logging`.

---

## 9) Performance

* Reuse **HttpClient** via the factory; add **caching** where repeated lookups occur.
* Avoid unnecessary allocations; prefer span-friendly APIs for heavy text processing.
* Monitor latency of tool calls; set sane timeouts and cancellation.

---

## 10) Security

* Treat all inputs as **untrusted**; validate and constrain.
* Rate-limit expensive tools; guard outbound calls with timeouts.
* Don’t keep secrets in memory longer than necessary; prefer env/secret stores.

---

## 11) Configuration & Environments

* Support env-driven configuration (e.g., `MCP_*`).
* Register telemetry if needed; separate dev/prod settings.
* For secrets, use platform vaults or environment-specific providers.

---

## 12) Testing

* **Unit tests** for tool methods (mock `IHttpClientFactory`, config, clock, etc.).
* **Integration tests**: start the server with stdio transport and exercise tool discovery/invocation.
* Validate protocol compatibility & error paths.

---

## 13) Deployment

* Run as a **child process** under clients/editors (stdio) or host as a service if your transport supports it.
* Implement graceful shutdown (SIGINT/Ctrl+C).
* Emit minimal, deterministic output for agents; measure memory/CPU.

---

## 14) MCP Server Integration (NuGet & Learn)

**Package Version Management**

* **CRITICAL**: Do **not** guess versions.
* **MANDATORY**: Query the **`nuget` MCP server** for the exact **stable/required** `ModelContextProtocol` version and pin it in `#:package`.
* Verify **protocol compatibility** when upgrading.

**Microsoft Technology Research**

* Use **`microsoft_learn` MCP** to cross-check .NET runtime/hosting patterns and evolving guidance.

---

## 15) Running FBA Apps

* Recommended: `dotnet run mcp.cs -- <args>` for clarity and agent pipelines.
* Shebang enables direct execution on Unix.
* FBA runs **without** a project file.

---

## 16) File-based Development Standards

* Default to **single-file FBA** unless a project is explicitly requested.
* Keep all directives (`#:property`, `#:sdk`, `#:package`) **at the top**.
* If the file grows, you may add **`Directory.Build.props`** for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 17) Project Conversion (On Request)

* Convert to a traditional project **only when explicitly asked**.
* Use an official CLI verb if/when available; write to a separate directory and **preserve** the original FBA source.
* Re-pin packages and mirror properties post-conversion.

---

## 18) Agent Execution Compatibility

* **Do not** add blockers like `Console.ReadLine()`/`ReadKey()` at the end.
* Let the process exit naturally so automation can capture STDOUT/STDERR and exit codes.
* Terminate promptly once work is done.

---

## 19) Minimal Patterns

**Single tool (public IP)**

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package ModelContextProtocol@<pin-exact-version>
#:property TargetFramework=net10.0
#:property PublishAot=False
#:property Nullable=enable

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// using ModelContextProtocol;

var b = Host.CreateApplicationBuilder(args);
b.Services.AddHttpClient();
b.Services.AddMcpServer(s => s
    .WithStdioServerTransport()
    .WithTools(t => t.Add("ip", GetIpAsync).WithDescription("Get public IP"))
);
await b.Build().RunAsync();

static async Task<string> GetIpAsync(IServiceProvider sp, bool ipv6 = false)
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var url = ipv6 ? "https://api64.ipify.org" : "https://api.ipify.org";
    return (await http.GetStringAsync(url)).Trim();
}
```

---

### Review Checklist

* [ ] `TargetFramework=net10.0`, `Nullable=enable`, **no** stray blocking calls
* [ ] `ModelContextProtocol` package **pinned** (via **`nuget` MCP** lookup)
* [ ] `Host.CreateApplicationBuilder(args)` used; config/logging/HTTP clients wired
* [ ] MCP server added; **STDIO** transport configured; tools registered with clear descriptions
* [ ] Inputs validated; deterministic output; meaningful error messages
* [ ] Natural termination; graceful shutdown handling
