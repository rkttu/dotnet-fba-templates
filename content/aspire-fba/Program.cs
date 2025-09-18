#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk
#:sdk Aspire.AppHost.Sdk@9.4.2

#:package Aspire.Hosting.AppHost@9.4.2

// Aspire AppHost cannot use AOT compilation.
#:property PublishAot=False

// The UserSecretsId is used to store sensitive information during development.
// Please change this GUID using generator and ensure this ID is unique to your project.
#:property UserSecretsId=7BD6F9B6-0150-4BD5-B50B-165F5F5B3045

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// For convenience, we add some configuration values directly here.
// Please consider using environment variables or user secrets for sensitive information in real applications.
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "DOTNET_ENVIRONMENT", "Development" },
    { "MYSQL_ROOT_PASSWORD", "aew!uwe5UTG7tzg0gqy" },
    { "MYSQL_DATABASE", "wordpress" },
    { "MYSQL_USER", "wordpress" },
    { "MYSQL_PASSWORD", "vvuu42XUUGgN7thvodQZ" },
    { "ASPNETCORE_URLS", "http://localhost:18888" },
    { "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:18889" },
    { "ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://localhost:18890" },
    { "ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true" },
});

var mysqlRootPassword = builder.AddParameterFromConfiguration("MySqlRootPassword", "MYSQL_ROOT_PASSWORD", true);
var mysqlDatabase = builder.AddParameterFromConfiguration("MySqlDatabase", "MYSQL_DATABASE");
var mysqlUser = builder.AddParameterFromConfiguration("MySqlUser", "MYSQL_USER");
var mysqlPassword = builder.AddParameterFromConfiguration("MySqlPassword", "MYSQL_PASSWORD", true);

// If MYSQL_USER or MYSQL_PASSWORD changes, the volume must be deleted and recreated.
var mysqlContainer = builder.AddContainer("mysql", "docker.io/mysql", "latest")
    .WithEndpoint(port: 3306, targetPort: 3306, name: "mysql-tcp")
    .WithEnvironment("MYSQL_ROOT_PASSWORD", mysqlRootPassword)
    .WithEnvironment("MYSQL_DATABASE", mysqlDatabase)
    .WithEnvironment("MYSQL_USER", mysqlUser)
    .WithEnvironment("MYSQL_PASSWORD", mysqlPassword)
    .WithVolume("mysql-data", "/var/lib/mysql") // Volumes remain intact even after AppHost is shut down.
    ;

var mysqlEndpoint = mysqlContainer.GetEndpoint("mysql-tcp");

_ = builder.AddContainer("phpmyadmin", "docker.io/phpmyadmin", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 80)
    .WithEnvironment("PMA_HOST", string.Join(':', mysqlContainer.Resource.Name, mysqlEndpoint?.TargetPort))
    .WithEnvironment("PMA_USER", "root")
    .WithEnvironment("PMA_PASSWORD", mysqlRootPassword)
    .WaitFor(mysqlContainer)
    ;

var wordpressContainer = builder.AddContainer("wordpress", "docker.io/wordpress", "latest")
    .WithHttpEndpoint(port: 8081, targetPort: 80, name: "wp-http")
    .WithEnvironment("WORDPRESS_DB_HOST", string.Join(':', mysqlContainer.Resource.Name, mysqlEndpoint?.TargetPort))
    .WithEnvironment("WORDPRESS_DB_NAME", mysqlDatabase)
    .WithEnvironment("WORDPRESS_DB_USER", mysqlUser)
    .WithEnvironment("WORDPRESS_DB_PASSWORD", mysqlPassword)
    .WithVolume("wp-data", "/var/www/html") // Volumes remain intact even after AppHost is shut down.
    .WaitFor(mysqlContainer)
    ;

var wordpressHttp = wordpressContainer.GetEndpoint("wp-http");

wordpressContainer.WithEnvironment("WORDPRESS_CONFIG_EXTRA", @"
    if (isset($_SERVER['HTTP_X_FORWARDED_PROTO']) && $_SERVER['HTTP_X_FORWARDED_PROTO'] === 'https') {
        $_SERVER['HTTPS'] = 'on';
    }
    ");

builder.Build().Run();
