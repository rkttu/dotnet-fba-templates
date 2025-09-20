# GitHub Copilot Instructions — AWS CDK File-based App (FBA) Template

> Opinionated, execution-ready guidance for writing **AWS CDK v2** apps as a **single C# file** targeting **.NET 10**.

---

## 1) Project Overview

This template shows how to author **Infrastructure as Code** with **AWS CDK v2** using the **File-based App (FBA)** model. You keep build/runtime directives at the top of one `.cs` file and synthesize CloudFormation via `App.Synth()`.

---

## 2) Quick Start

```bash
# Create a CDK FBA file
cat > cdk.cs << 'CS'
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Amazon.CDK.Lib@<pin-exact-version>
#:property TargetFramework=net10.0
#:property PublishAot=False        # CDK uses reflection; keep AOT off
#:property Nullable=enable

// --- Top-level program must precede types ---
using Amazon.CDK;
using Constructs;

var app = new App();

// Example: environment-aware stack name
var env = new Environment { Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                            Region  = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") };

_ = new SampleStack(app, "SampleStack", new StackProps { Env = env });

app.Synth();

// --- Types follow top-level code ---
sealed class SampleStack : Stack
{
    public SampleStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // Example resource:
        // var bucket = new Amazon.CDK.AWS.S3.Bucket(this, "AppBucket", new()
        // {
        //     RemovalPolicy = RemovalPolicy.DESTROY,          // dev only
        //     AutoDeleteObjects = true                        // dev only
        // });

        // new CfnOutput(this, "BucketName", new() { Value = bucket.BucketName });
    }
}
CS

# Run the FBA (executes CDK app and creates cdk.out)
dotnet run cdk.cs

# Deploy with CDK CLI (after bootstrapping your account/region)
# npm i -g aws-cdk@2  &&  cdk bootstrap  &&  cdk deploy
```

> Keep `#:sdk`, `#:package`, and `#:property` directives **at the top**.
> On Unix, the shebang must be the first line for `chmod +x cdk.cs && ./cdk.cs`.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet`
* **SDK**: `#:sdk Microsoft.NET.Sdk` (CDK apps are console apps; **not** Web SDK)
* **Packages**: `#:package Amazon.CDK.Lib@ExactVersion` (pin exact v2)
* **Properties**: `TargetFramework=net10.0`, `PublishAot=False`, `Nullable=enable`

---

## 4) CDK App Structure (C#)

* **Top-level rule**: Executable statements **first**; type declarations follow.
* **Create the app**: `var app = new App();`
* **Add stacks**: `new MyStack(app, "MyStack", new StackProps { Env = … });`
* **Finish**: `app.Synth();` (writes templates into `cdk.out`)
* **Naming**: Use meaningful logical IDs and stack names; prefer environment suffixes (e.g., `MyApp-Prod`).

**Standard stack constructor**
`public MyStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props) { … }`

---

## 5) Resource Management

* Use v2 namespaces (`Amazon.CDK.AWS.*`) for resources (S3, Lambda, SQS, etc.).
* **Removal policy**: `DESTROY` for **dev**, `RETAIN` for **prod**.
* **S3 (dev only)**: `AutoDeleteObjects = true`.
* Prefer **environment-agnostic** defaults but allow overrides via context/env vars.
* Use **construct IDs** that communicate intent and avoid collisions.

**Outputs & cross-stack references**

* Use `new CfnOutput(this, "OutputName", new() { Value = …, Description = … });`
* Export only what other stacks or tools need; keep surface minimal.

---

## 6) Modern C# Guidelines (C# 12/13)

* Enable **nullable**; use file-scoped namespaces if/when you add namespaces.
* Prefer primary constructors / required members where they add clarity.
* Use raw string literals for multi-line policy docs or user data.
* Pattern matching (switch/property/tuple/list) for concise configuration logic.

---

## 7) Async & Concurrency (when applicable)

* CDK apps are typically **sync**; if you introduce async I/O, don’t add `.ConfigureAwait(false)` mechanically—there’s no UI context. Reserve it for **library** code that could run under a capturing context.
* If an async method has no `await`, return `Task.CompletedTask`/`ValueTask.CompletedTask`.
* Use `IAsyncEnumerable<T>` only when you truly stream data.

---

## 8) Security

* **Never** hardcode credentials or secrets.
* Use AWS IAM **least privilege** for roles/policies.
* Enable encryption at rest/in transit where available (e.g., S3, KMS, RDS).
* Sanitize user-supplied names/inputs used in resource identifiers.

---

## 9) Cost Management

* Choose appropriate S3 storage classes and lifecycle policies.
* Right-size compute (Lambda memory/timeout; Fargate CPU/mem).
* Add CloudWatch alarms for budget-impacting metrics.
* Consider reserved/savings plans for predictable workloads.

---

## 10) Environments & Tagging

* Use context/env for account/region: `CDK_DEFAULT_ACCOUNT`, `CDK_DEFAULT_REGION`.
* Apply consistent **tags** for cost allocation (e.g., `Project`, `Env`, `Owner`).
* Consider **CDK Pipelines** or your CI to promote across environments.

---

## 11) Development Workflow

* **Prereqs**: Node.js LTS, AWS CLI, AWS CDK CLI v2; Docker/Podman if needed.
* **Local emulation**: LocalStack + `cdklocal`/`awslocal` (optional).
* **Bootstrap** each target account/region once: `cdk bootstrap`.
* **CI/CD**: validate (`cdk synth`), diff, then gated deploy; enable automatic **rollback**.

---

## 12) Testing

* **Unit tests**: construct behavior with CDK assertions.
* **Snapshot tests**: template diffs are powerful for regression.
* **Integration tests**: run against real AWS where meaningful (or LocalStack).
* Validate **fail-closed** defaults (e.g., deny-all until explicitly allowed).

---

## 13) Performance

* Prefer **CDK v2** (single package) and keep dependencies lean.
* Cache package restores in CI.
* Keep stacks cohesive to reduce template size & synth time.

---

## 14) MCP Server Integration

**Package Version Management**

* **CRITICAL**: Do **not** guess package versions.
* **MANDATORY**: Query the **`nuget` MCP server** for the exact stable `Amazon.CDK.Lib` (and any `Amazon.CDK.AWS.*` packages if used), then pin via `#:package`.

**Microsoft / AWS Guidance**

* Use **`microsoft_learn` MCP** for .NET language/runtime practices.
* Consult AWS CDK official docs for IaC patterns; align with least-privilege, environment promotion, and tagging guidance.

---

## 15) Running File-based Apps

* Recommended: `dotnet run cdk.cs` (explicit invocation works well in agent pipelines).
* Shebang enables `./cdk.cs` on Unix systems.
* FBA runs **without** a project file; artifacts appear in `cdk.out`.
* Use `cdk diff` and `cdk deploy` after synth.

---

## 16) File-based Development Standards

* Default to **single-file FBA** unless a user explicitly requests a project.
* Keep `#:property`, `#:sdk`, and `#:package` at the top; **pin** versions.
* If the file grows large, you may add a **`Directory.Build.props`** for shared MSBuild settings—**FBA remains authoritative**.

---

## 17) Project Conversion (On Request)

* Convert to a traditional project **only** when explicitly asked.
* Use an official CLI verb when available; write to a separate directory and **preserve** the original FBA source.
* Re-pin packages and mirror properties post-conversion.

---

## 18) Agent Execution Compatibility

* **Do not** add `Console.ReadLine()`/`ReadKey()` to block termination.
* Let the process exit naturally so automation can capture STDOUT/STDERR and exit codes.
* Keep output deterministic for pipelines.

---

## 19) Minimal Patterns

**S3 Bucket (dev-friendly)**

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Amazon.CDK.Lib@<pin-exact-version>
#:property TargetFramework=net10.0
#:property PublishAot=False
#:property Nullable=enable

using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Constructs;

var app = new App();
var env = new Environment { Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                            Region  = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") };

_ = new StorageStack(app, "StorageStack-Dev", new StackProps { Env = env });
app.Synth();

sealed class StorageStack : Stack
{
    public StorageStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        var bucket = new Bucket(this, "AppBucket", new BucketProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,  // dev only
            AutoDeleteObjects = true                // dev only
        });

        _ = new CfnOutput(this, "BucketName", new CfnOutputProps { Value = bucket.BucketName });
    }
}
```

**Lambda + SQS (naming & outputs)**

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Amazon.CDK.Lib@<pin-exact-version>
#:property TargetFramework=net10.0

using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Constructs;

var app = new App();
var env = new Environment { Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                            Region  = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") };

_ = new MessagingStack(app, "MessagingStack", new StackProps { Env = env });
app.Synth();

sealed class MessagingStack : Stack
{
    public MessagingStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        var queue = new Queue(this, "IngestQueue", new QueueProps { VisibilityTimeout = Duration.Seconds(30) });

        var fn = new Function(this, "WorkerFn", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,               // runtime for deployed Lambda (independent of CDK’s TFM)
            Handler = "Worker::Worker.Function::Handler",
            Code = Code.FromAsset("lambda-publish")   // artifact path
        });

        queue.GrantSendMessages(fn);

        _ = new CfnOutput(this, "QueueUrl", new() { Value = queue.QueueUrl });
        _ = new CfnOutput(this, "FunctionName", new() { Value = fn.FunctionName });
    }
}
```

---

### Review Checklist

* [ ] `#:sdk Microsoft.NET.Sdk`, `TargetFramework=net10.0`, `PublishAot=False`, `Nullable=enable`
* [ ] **All CDK packages pinned** to exact versions (via **`nuget` MCP** lookup)
* [ ] `App` → stacks → `app.Synth()` present; top-level code before types
* [ ] Removal policies & dev-only flags (`AutoDeleteObjects`) guarded by environment
* [ ] No secrets/credentials in code; IAM least-privilege enforced
* [ ] Deterministic outputs; meaningful IDs and stack names
* [ ] Natural termination (no blocking reads)
