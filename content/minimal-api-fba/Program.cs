#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk.Web

// You can use AOT compilation by changing PublishAot to True.
// Please note that AOT compilation may break some functionalities like reflection.
#:property PublishAot=False

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Logging.AddConsole();

using var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
