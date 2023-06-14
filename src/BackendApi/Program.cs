using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
// Change namespace to BackendApi.Manual to test manual
using BackendApi.Manual;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.local.json", false);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}