# GitHub Copilot Instructions — Windows **rundll32** File-based App (FBA) Template

> Build **Windows-native DLLs** with **.NET 10 + NativeAOT** that expose a **rundll32-compatible** entry point, authored in a **single C# file**.

---

## 1) Project Overview

This template targets a **native Windows DLL** compiled with **PublishAot=True**. The DLL exposes an unmanaged export matching `rundll32.exe`’s callback signature so you can execute it via:

```bat
rundll32.exe YourDll.dll,RunFunction optional arguments...
```

All build/runtime settings live at the top of a single `.cs` file (FBA). No project file is required.

---

## 2) Quick Start (export for `rundll32`)

```csharp
// Shebang optional for Windows DLLs

#:property OutputType=Library
#:property PublishAot=True
#:property RuntimeIdentifier=win-x64
#:property Nullable=enable
#:property StripSymbols=true
#:property OptimizationPreference=Size
#:property DebugType=embedded

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static class RunExports
{
    // rundll32 signature: void CALLBACK EntryPoint(HWND, HINSTANCE, LPSTR, int)
    // CALLBACK is __stdcall on Win32; on x64 the Microsoft x64 ABI is used.
    [UnmanagedCallersOnly(EntryPoint = "RunFunction", CallConvs = new[] { typeof(CallConvStdcall) })]
    public static void RunFunction(nint hwnd, nint hinst, nint lpszCmdLine, int nCmdShow)
    {
        // Parse command line (ANSI) into args
        var raw = Marshal.PtrToStringAnsi(lpszCmdLine) ?? string.Empty;
        var args = ArgSplit(raw);

        // Example behavior: /silent => no UI; otherwise show a message box
        if (HasSwitch(args, "silent"))
            return;

        var msg = $"HWND=0x{hwnd:X}, HINST=0x{hinst:X}\nArgs= [{string.Join(", ", args)}]\nShow={nCmdShow}";
        _ = MessageBoxW(hwnd, msg, "Hello from .NET NativeAOT (rundll32)", 0x00000040 /* MB_ICONINFORMATION */);
    }

    // --------------- helpers ----------------

    // Minimal, allocation-light arg splitter (quotes keep spaces)
    private static string[] ArgSplit(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return Array.Empty<string>();
        var list = new System.Collections.Generic.List<string>(8);
        var sb = new StringBuilder(cmd.Length);
        bool inQuotes = false;

        foreach (char c in cmd)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (sb.Length > 0) { list.Add(sb.ToString()); sb.Clear(); }
            }
            else sb.Append(c);
        }
        if (sb.Length > 0) list.Add(sb.ToString());
        return list.ToArray();
    }

    private static bool HasSwitch(string[] args, string name)
    {
        foreach (var a in args)
            if (a.Equals($"/{name}", StringComparison.OrdinalIgnoreCase) ||
                a.Equals($"-{name}", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    [LibraryImport("user32", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int MessageBoxW(nint hWnd, string text, string caption, uint type);
}
```

**Build (publish the native DLL):**

```powershell
dotnet publish rundll.cs -c Release
# => bin\Release\net10.0\win-x64\native\<file-name>.dll
```

**Execute with `rundll32`:**

```powershell
rundll32.exe .\bin\Release\net10.0\win-x64\native\rundll.dll,RunFunction "/silent"
rundll32.exe .\bin\Release\net10.0\win-x64\native\rundll.dll,RunFunction "do-something --target ""C:\Path With Spaces"""
```

> **Note**: Everything after the comma is passed to your function as a **single ANSI string** (`LPSTR`). Your code must parse it.

---

## 3) FBA Directive Cheatsheet

* `OutputType=Library` — produce a DLL (no `Main`).
* `PublishAot=True` — compile with **NativeAOT** to expose unmanaged exports.
* `RuntimeIdentifier=win-x64` (or `win-arm64`) — choose a concrete RID.
* Optional: `StripSymbols=true`, `OptimizationPreference=Size`, `DebugType=embedded`, `Nullable=enable`.

> **No top-level statements** in a DLL—put code inside types only.

---

## 4) `rundll32` Entry Point Requirements

* Exported method must be **static** and decorated with **`[UnmanagedCallersOnly]`**.
* **EntryPoint name** after the comma must match exactly, e.g., `RunFunction`.
* Signature (recommended):

  ```csharp
  void RunFunction(nint hwnd, nint hinst, nint lpszCmdLine, int nCmdShow)
  ```

  * `hwnd` → `HWND`
  * `hinst` → `HINSTANCE`
  * `lpszCmdLine` → `LPSTR` (ANSI) → use `Marshal.PtrToStringAnsi`
  * `nCmdShow` → window show state (`SW_*`)
* On x86, specify `CallConvStdcall`; on x64 the attribute is ignored (x64 ABI is fixed).

---

## 5) Command Line Processing

* `rundll32` passes **one** raw string (no argv\[]), so you must tokenize.
* Honor quotes to preserve spaces; accept `/switch` or `-switch`.
* Provide a `/silent` (no UI) mode for automation scenarios.

---

## 6) Windows API Usage

* Use **source-generated P/Invoke** (`[LibraryImport]`) for AOT-friendly interop.
* Prefer **Unicode** Win32 APIs (`...W`) and marshal as UTF-16.
* Handle errors via `GetLastError` (set `SetLastError=true` on imports).

---

## 7) AOT-Compatible Code

* Avoid reflection, dynamic code gen, runtime assembly loading.
* Keep exports simple, rely on blittable types or pointers (`nint`/`nuint`).
* Do **not** throw exceptions across the boundary—catch and handle internally.

---

## 8) Error Handling

* Don’t propagate exceptions to native callers.
* Use **status codes**, logs, or a secondary export (e.g., `get_last_error`) if needed.
* For user-facing errors, prefer **MessageBox** (optional) or Event Log entries.

---

## 9) Security Guidelines

* Treat all parameters as **untrusted**.
* Validate handles and buffer sizes; guard against integer overflow.
* Avoid elevation patterns in `rundll32`; perform privilege checks explicitly.
* Don’t expose sensitive functionality via public exports on user-writable paths.

---

## 10) Testing Strategies

* **Interop testing**: native C/C++ harness that `LoadLibrary` + `GetProcAddress` + calls your export.
* **rundll32 testing**: cover quoting, empty args, `/silent`, and invalid switches.
* **System testing**: different Windows versions, user privileges, session types (console vs. service).

---

## 11) Deployment & Registration

* Publish **Release** for the target RID; ship the resulting native DLL.
* If integrating with shell/Control Panel, document registry entries and cleanup.
* Version exports **by adding new entry points** rather than changing signatures.

---

## 12) What about `dotnet run`?

* DLLs aren’t runnable artifacts. Use **`dotnet publish`** to build, then invoke via **`rundll32.exe <dll>,<Export>`** or load from a native host.

---

## 13) Review Checklist

* [ ] `OutputType=Library`, `PublishAot=True`, `RuntimeIdentifier=win-x64|win-arm64`
* [ ] Export marked with `[UnmanagedCallersOnly(EntryPoint=..., CallConvs=...)]`
* [ ] Signature matches `rundll32` (HWND, HINSTANCE, LPSTR, int)
* [ ] ANSI command line parsed correctly; quotes honored; `/silent` supported
* [ ] No exceptions cross boundary; optional status codes or UI
* [ ] AOT-safe (no reflection/dynamic loading)
* [ ] Tested via `rundll32` and native harness

---

## 14) MCP Integration (NuGet & Docs)

* **Never guess package versions**. If you add packages, resolve exact versions via your **`nuget` MCP server** and pin with `#:package PackageId@X.Y.Z`.
* Use **`microsoft_learn` MCP** to track current NativeAOT, interop, and Windows shell guidance.
