#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk.Web
#:property TargetFramework=NET_TFM

// You can use AOT compilation by changing PublishAot to True.
// Please note that AOT compilation may break some functionalities like reflection.
#if (EnableAot)
#:property PublishAot=True
#else
#:property PublishAot=False
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Logging.AddConsole();

using var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
