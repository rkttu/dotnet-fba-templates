# GitHub Copilot Instructions — Python.NET File-based App (FBA) Template

> Practical, execution-ready guidance for integrating **Python** into a **single-file .NET 10** app via **pythonnet**.

---

## 1) Project Overview

This template shows how to embed Python in a **File-based App (FBA)**: declare build/runtime directives at the top of one `.cs` file, run top-level code first, and keep any type declarations below.

---

## 2) Quick Start

```bash
# Create an FBA file
cat > py.cs << 'CS'
#!/usr/bin/env dotnet
#:property TargetFramework=net10.0
#:package pythonnet@3.0.5
#:property PublishAot=False
#:property Nullable=enable

using System;
using System.Runtime.InteropServices;
using Python.Runtime;

try
{
    // 1) Resolve Python runtime path(s) before Initialize
    PythonRuntimeConfigurator.TryConfigureFromEnvironment(); // see helper below

    // 2) Start Python and acquire the GIL for all Python interactions
    PythonEngine.Initialize();
    using (Py.GIL())
    {
        dynamic sys = Py.Import("sys");
        Console.WriteLine($"Python {sys.version}");
        dynamic math = Py.Import("math");
        Console.WriteLine($"cos(0) = {math.cos(0.0)}");
    }
}
catch (PythonException pex)
{
    Console.Error.WriteLine($"Python error: {pex.Message}");
    Environment.ExitCode = 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
finally
{
    if (PythonEngine.IsInitialized) PythonEngine.Shutdown();
}

// --- helper types follow top-level code ---
static class PythonRuntimeConfigurator
{
    // Minimal, safe defaults. Prefer env vars when present.
    public static void TryConfigureFromEnvironment()
    {
        // If PYTHONNET_PYDLL is set, pythonnet will use it.
        // Optionally set PythonEngine.PythonHome/PythonPath if your layout requires it.
        var pyDll = Environment.GetEnvironmentVariable("PYTHONNET_PYDLL");
        if (!string.IsNullOrWhiteSpace(pyDll)) return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Common Windows locations; adjust versions as needed.
            // Example: C:\Python313\python313.dll
            var candidates = new[]
            {
                $@"C:\Python313\python313.dll",
                $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python313\python313.dll"
            };
            foreach (var c in candidates) if (System.IO.File.Exists(c))
            {
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", c);
                return;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Homebrew framework dylib (Apple Silicon default path)
            var c = "/opt/homebrew/Frameworks/Python.framework/Versions/3.13/lib/libpython3.13.dylib";
            if (System.IO.File.Exists(c))
            {
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", c);
                // Optional: Ensure encodings can be found if needed
                var home = "/opt/homebrew/Frameworks/Python.framework/Versions/3.13";
                Environment.SetEnvironmentVariable("PYTHONHOME", home);
            }
        }
        else
        {
            // Linux typical soname path; prefer distro packages or venv lib
            foreach (var c in new[]
            {
                "/usr/lib/x86_64-linux-gnu/libpython3.13.so",
                "/usr/lib/libpython3.13.so"
            })
            if (System.IO.File.Exists(c))
            {
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", c);
                return;
            }
        }
    }
}
CS

# Run (explicit invocation is agent-friendly)
dotnet run py.cs
```

> Keep `#:property`/`#:package` **at the top**. AOT must remain **off** for pythonnet.

---

## 3) FBA Directive Cheatsheet

* **Shebang**: `#!/usr/bin/env dotnet` (first line on Unix to allow `./py.cs`)
* **Target**: `#:property TargetFramework=net10.0`
* **Package**: `#:package pythonnet@3.0.5` (pin exact version)
* **AOT**: `#:property PublishAot=False` (pythonnet relies on reflection/interop)
* **Nullable**: `#:property Nullable=enable`

---

## 4) Engine Lifecycle & GIL

* Call **`PythonEngine.Initialize()`** **before** any Python work; call **`PythonEngine.Shutdown()`** in `finally`.
* Wrap Python calls in **`using (Py.GIL())`** blocks to own the GIL.
* For multi-threaded scenarios, acquire the GIL per thread entering Python.

---

## 5) Runtime Configuration

* Prefer environment variables:

  * **`PYTHONNET_PYDLL`**: absolute path to `python3XY` shared lib (`.dll` / `.so` / `.dylib`).
  * **`PYTHONHOME`**: root of the Python installation (ensures `encodings/` etc.).
  * **`PYTHONPATH`**: additional module paths if needed (e.g., site-packages).
* Platform hints:

  * **Windows**: `C:\Python313\python313.dll` or per-user installs.
  * **macOS (Homebrew, Apple Silicon)**: `/opt/homebrew/Frameworks/Python.framework/Versions/3.13/lib/libpython3.13.dylib` and set `PYTHONHOME` to the `Versions/3.13` root.
  * **Linux**: distro paths under `/usr/lib…/libpython3.13.so` or venv `lib/`.

---

## 6) Interop Patterns

* Import: `dynamic np = Py.Import("numpy");`
* Call: `var r = np.array(new[] {1,2,3});`
* Convert:

  * **.NET → Py**: `using var pyObj = new PyInt(42);`
  * **Py → .NET**: `int x = pyInt.As<int>();`
* Collections: convert lists/dicts deliberately to avoid copies in hot paths.

---

## 7) Modern C# (C# 12/13)

* **Nullable** on; use raw string literals for embedded Python scripts.
* Use primary constructors / `required` / `init` where they improve clarity.
* Pattern matching for robust type checks on dynamic results.

---

## 8) Async & Concurrency

* Console/FBA apps don’t capture a UI context—**don’t** sprinkle `ConfigureAwait(false)` by default. Reserve it for **library** code that might run under a capturing context.
* If a method has no `await`, return `Task.CompletedTask`/`ValueTask.CompletedTask`.
* Consider **batching** Python calls to reduce GIL churn.

---

## 9) Data Conversion Tips

* **NumPy**: exchange large numeric buffers via arrays; prefer zero-copy where possible.
* **Pandas**: serialize to/from CSV/Parquet/Arrow when sizes are large; avoid per-row marshaling.
* **Datetime**: normalize to UTC; convert via ISO strings or epoch where practical.

---

## 10) Error Handling & Diagnostics

* Catch **`PythonException`** for Python-side failures; log both .NET and Python messages.
* Keep STDOUT for app outputs; send diagnostics to STDERR or logs.
* Standardize messages for agent pipelines.

---

## 11) Performance

* **Reuse** `HttpClient`/resources in tools invoked from Python.
* Cache imported modules; avoid repeated `Py.Import` in loops.
* Release the GIL when doing long .NET-only work.
* Measure allocations; avoid needless conversions/copies.

---

## 12) Security

* Treat inputs as untrusted; validate all arguments passed to Python.
* Avoid executing arbitrary code; constrain module/function lists.
* Do not keep secrets in Python globals longer than required.

---

## 13) Testing

* **Unit**: isolate conversion helpers and business logic; mock Python where possible.
* **Integration**: validate venvs, module availability, and error paths.
* **Matrix**: test multiple Python minor versions and OSes you support.

---

## 14) Deployment

* Prefer **documented** Python requirements (version, packages).
* Options:

  * Depend on a **system Python** and set env vars at startup.
  * Ship an **embedded/redistributable Python** (per OS) and point `PYTHONHOME/PYTHONNET_PYDLL` to it.
  * **Containers**: choose a base image that already includes Python; install packages, set env at build time.
* Verify `encodings` is present; missing `encodings` → set correct **`PYTHONHOME`**.

---

## 15) MCP Integration (NuGet & Learn)

**Package Version Management**

* **Do not guess versions**. Pin `pythonnet` with the **exact** version (use your MCP `nuget` server to resolve).
* Validate compatibility when upgrading Python minor versions.

**Microsoft Guidance**

* Use your MCP `microsoft_learn` source for current .NET hosting/interop best practices.

---

## 16) Running FBA

* Recommended: `dotnet run py.cs -- <args>`.
* Shebang allows `./py.cs` on Unix.
* FBA runs **without** a project file.

---

## 17) File-based Development Standards

* Default to **single-file FBA**; keep directives at the **top**.
* If the file grows, add a `Directory.Build.props` for shared MSBuild settings—**the FBA file remains authoritative**.

---

## 18) Project Conversion (On Request)

* Convert to a traditional project **only** when explicitly asked; emit to a separate directory and **preserve** the original FBA.
* Re-pin packages and port build properties.

---

## 19) Agent Execution Compatibility

* Do **not** add terminal blockers like `Console.ReadLine()`/`ReadKey()`.
* Let the process exit naturally so agents can capture STDOUT/STDERR and exit codes.

---

## 20) Troubleshooting (field-proven)

* **`ModuleNotFoundError: No module named 'encodings'`**
  Set **`PYTHONHOME`** to the Python root (so `.../lib/python3.X/encodings` is discoverable).
  On macOS (Homebrew):

  * `PYTHONHOME=/opt/homebrew/Frameworks/Python.framework/Versions/3.13`
  * `PYTHONNET_PYDLL=/opt/homebrew/Frameworks/Python.framework/Versions/3.13/lib/libpython3.13.dylib`
* **DLL/SO not found**
  Ensure `PYTHONNET_PYDLL` points to a valid file for the current architecture (x64/arm64).
* **Version mismatch**
  Python minor versions must match the lib (e.g., 3.13 ↔ `libpython3.13.*`).
* **Crashes at shutdown**
  Ensure all `PyObject` wrappers are disposed or out of scope **before** `PythonEngine.Shutdown()`; keep finalizers from running after shutdown.
