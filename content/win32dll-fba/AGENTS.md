# GitHub Copilot Instructions — Windows DLL File-based App (FBA) Template

> Build **Windows-native DLLs** with **.NET 10 + NativeAOT** in a **single C# file** that exports unmanaged entry points for use from C/C++ and other native callers.

---

## 1) Project Overview

This template targets a **native Windows DLL** compiled with **PublishAot=True**. You author everything in one `.cs` file with FBA directives at the top. The DLL exposes unmanaged exports via `[UnmanagedCallersOnly]` and avoids runtime features that aren’t available under NativeAOT.

---

## 2) Quick Start (zero-dependency, export by name)

```csharp
// Shebang is optional for Windows DLLs

#:property OutputType=Library
#:property PublishAot=True
#:property RuntimeIdentifier=win-x64
#:property StripSymbols=true
#:property OptimizationPreference=Size
#:property DebugType=embedded
#:property Nullable=enable

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public static class Exports
{
    // Exported as "add32" with __cdecl (common for cross-language interop)
    [UnmanagedCallersOnly(EntryPoint = "add32", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int Add32(int a, int b) => a + b;

    // Unicode input (UTF-16) from native caller: LPCWSTR -> char16_t* (wchar_t on Windows)
    [UnmanagedCallersOnly(EntryPoint = "lenw", CallConvs = new[] { typeof(CallConvStdcall) })]
    public static int LengthWide(nint lpwStr)
        => lpwStr == 0 ? 0 : (Marshal.PtrToStringUni(lpwStr)?.Length ?? 0);

    // Fill a caller-allocated UTF-16 buffer. Returns 0 on success; >0 = required chars (including NUL).
    [UnmanagedCallersOnly(EntryPoint = "hello", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int Hello(nint outBuffer, int bufferChars)
    {
        const string msg = "Hello from .NET NativeAOT 🎯";
        var needed = msg.Length + 1;
        if (bufferChars < needed) return needed;

        // copy characters; buffer is UTF-16
        unsafe
        {
            var dst = (char*)outBuffer;
            msg.AsSpan().CopyTo(new Span<char>(dst, msg.Length));
            dst[msg.Length] = '\0';
        }
        return 0;
    }
}
```

**Build (publish the native DLL):**

```bash
dotnet publish dll.cs -c Release
# Output: bin/Release/net10.0/win-x64/native/dll.dll (name follows your file name)
```

**Consume from C (example):**

```c
typedef int (__cdecl *add32_t)(int,int);
typedef int (__stdcall *lenw_t)(const wchar_t*);
typedef int (__cdecl *hello_t)(wchar_t*, int);

HMODULE h = LoadLibraryW(L"dll.dll");
add32_t add32 = (add32_t)GetProcAddress(h, "add32");
lenw_t  lenw  = (lenw_t) GetProcAddress(h, "lenw");
hello_t hello = (hello_t)GetProcAddress(h, "hello");
```

---

## 3) FBA Directive Cheatsheet

* `#:property OutputType=Library` — produce a DLL (no `Main`).
* `#:property PublishAot=True` — compile with NativeAOT.
* `#:property RuntimeIdentifier=win-x64` (or `win-arm64`) — pick a concrete RID.
* Optional hardening/size flags:

  * `StripSymbols=true`, `OptimizationPreference=Size`, `DebugType=embedded`.

> **No top-level statements** in a DLL—**don’t** write executable code outside types.

---

## 4) Export Fundamentals

* Use **`[UnmanagedCallersOnly]`** on **`static`** methods in a **`public`** type.
* Export **by name** with `EntryPoint="name"`.
* Choose a calling convention:

  * `CallConvCdecl` (portable default for cross-language C usage).
  * `CallConvStdcall` (common in Win32 APIs).
  * `CallConvWinapi` (ABI varies by arch; use only if you know the caller’s ABI).
* Accept **blittable** value types or **pointers** (`nint`/`nuint`). Avoid marshaling attributes—`UnmanagedCallersOnly` bypasses the runtime marshaler.
* Strings: pass **UTF-16** as `wchar_t*` (Windows). Convert with `Marshal.PtrToStringUni`.

---

## 5) Strings & Memory

* **Inputs**: Treat `nint` pointers as **read-only**; validate before use.
* **Outputs**: Prefer **caller-allocated** buffers (pointer + capacity). Return `0` on success or **required length** if too small.
* **Ownership**: Don’t return memory that .NET owns to native. If you must allocate to return data, allocate with **`CoTaskMemAlloc`** so the caller can free via `CoTaskMemFree`—document it.

**CoTaskMem pattern (when unavoidable):**

```csharp
[UnmanagedCallersOnly(EntryPoint = "getmsg", CallConvs = new[] { typeof(CallConvCdecl) })]
public static nint GetMessage()
{
    const string s = "Native string";
    var bytes = (s.Length + 1) * 2; // UTF-16 size
    var p = Marshal.AllocCoTaskMem(bytes);
    unsafe
    {
        var dst = (char*)p;
        s.AsSpan().CopyTo(new Span<char>(dst, s.Length));
        dst[s.Length] = '\0';
    }
    return p; // document: caller frees with CoTaskMemFree
}
```

---

## 6) Structs, Arrays, and Alignment

* Mirror **native layout** exactly. Use `[StructLayout(LayoutKind.Sequential, Pack = 1)]` if the ABI requires tight packing.
* Pass arrays as `(T* ptr, int count)`. The caller owns lifetime.
* Watch for **endianness** (Windows is little-endian) and **alignment**. Validate sizes on both sides.

---

## 7) Error Handling

* **Never** throw across the interop boundary. Instead:

  * Return **error codes** (e.g., `0` success, non-zero error).
  * Optionally expose a `get_last_error_message(outBuffer, cap)` function to retrieve the last error text.
* Log internally if helpful, but keep the export ABI deterministic.

---

## 8) Threading & COM

* Exported methods may be called from **any thread**. Ensure thread safety.
* If you touch COM, initialize per thread (STA/MTA) exactly as your API requires—**do not** assume the host did it.

---

## 9) AOT Compatibility

* Avoid reflection, dynamic type creation, runtime codegen, and Assembly loading.
* Prefer source generators and static generic instantiations.
* Keep exceptions internal; map to status codes at the boundary.

---

## 10) Testing

* **Interop tests**: create a native C/C++ harness that `LoadLibrary` + `GetProcAddress` for smoke tests.
* **Unit tests**: pure managed logic can be tested in a standard test project (no AOT).
* **Performance**: measure call overhead and buffer sizes with realistic data.

---

## 11) Deployment

* **Publish** with `-c Release` for the target RID.
* Ship the **single DLL** (and any dependent native assets if you added them).
* Document:

  * Target arch (x64/arm64), calling conventions, char encoding, error codes, and memory ownership.

---

## 12) Security

* Validate **all** pointers, counts, and sizes.
* Bounds-check buffers; avoid integer overflow on size math.
* Treat inputs as untrusted; be explicit about **ownership** and **lifetime**.

---

## 13) Versioning & Naming

* Use stable **EntryPoint names**; avoid breaking changes.
* If you must change signatures, add new exports rather than changing existing ones.

---

## 14) Appendix — Patterns

**In-place transform (bytes):**

```csharp
[UnmanagedCallersOnly(EntryPoint = "xor_mask", CallConvs = new[] { typeof(CallConvCdecl) })]
public static int XorMask(nint data, int length, byte key)
{
    if (data == 0 || length < 0) return 1;
    unsafe
    {
        var p = (byte*)data;
        for (int i = 0; i < length; i++) p[i] ^= key;
    }
    return 0;
}
```

**Struct echo (Sequential layout):**

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct PointD { public double X, Y; }

[UnmanagedCallersOnly(EntryPoint = "norm2", CallConvs = new[] { typeof(CallConvStdcall) })]
public static double Norm2(PointD p) => p.X * p.X + p.Y * p.Y;
```

---

## 15) Review Checklist

* [ ] `OutputType=Library`, `PublishAot=True`, `RuntimeIdentifier=win-x64|win-arm64`
* [ ] No top-level executable code; **only** types with static exports
* [ ] Exports use `[UnmanagedCallersOnly(EntryPoint=..., CallConvs=...)]`
* [ ] C-compatible signatures; UTF-16 policy documented
* [ ] Caller-allocated buffer pattern or documented `CoTaskMem` ownership
* [ ] No exceptions crossing the boundary; return **status codes**
* [ ] Avoid reflection/dynamic features; AOT-safe
* [ ] Interop harness validated with `LoadLibrary`/`GetProcAddress`
* [ ] Release build, symbols/optimizations configured

---

## 16) Notes on Tooling & Research

* **NuGet pinning**: If you add packages, **never guess versions**. Resolve exact versions with your **`nuget` MCP server** and pin them via `#:package PackageId@X.Y.Z`.
* **Microsoft guidance**: Use **`microsoft_learn` MCP** for current NativeAOT, interop, and Windows ABI best practices.

---

## 17) Why not `dotnet run`?

DLLs are **not** runnable artifacts. Use **`dotnet publish`** to produce the native library, then load it from your native host.
