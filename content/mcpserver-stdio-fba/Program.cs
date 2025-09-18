# !/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk.Web
#:package ModelContextProtocol@0.3.0-preview.4

// You can use AOT compilation by changing PublishAot to True.
// Please note that AOT compilation may break some functionalities like reflection.
#:property PublishAot=False

// NOTE: The dotnet run .cs command may not allow multiple clients to use this MCP server simultaneously.
using System.ComponentModel;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(default);
builder.Configuration.AddCommandLine(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpClient();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools([
        McpServerTool.Create(IpAddressTool),
    ]);

var app = builder.Build();
app.Run();

[Description("Get the public IP address of this machine.")]
async Task<string> IpAddressTool(
    IServiceProvider services,
    [Description("Get IPv6 address instead of IPv4 address")] bool ipv6)
{
    try
    {
        var client = services.GetRequiredService<HttpClient>();
        return await client.GetStringAsync(
            ipv6 ? "https://api6.ipify.org" : "https://api.ipify.org"
            ).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        return $"Error occurred: {ex.Message}";
    }
}
