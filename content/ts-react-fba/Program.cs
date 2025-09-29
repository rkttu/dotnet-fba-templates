#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=NET_TFM

#:package Microsoft.TypeScript.MSBuild@5.9.2
#:property PublishAot=False

// Zero-config TypeScript + React fullstack development with single-file approach
// Run with `dotnet run --no-cache Program.cs`

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/hello", () => new { Message = "Hello from ASP.NET Core!" });

app.Run();
