using GraphCaller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

builder.Configuration.AddJsonFile("appsettings.local.json", true);
builder.Services.AddHostedService<GraphReaderService>();

var host = builder.Build();
host.Run();
