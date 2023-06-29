using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Serilog;

namespace GraphCaller;

public class GraphReaderService : IHostedService
{
    private readonly string? _clientId;
    private readonly string? _secret;
    private readonly string? _tenantId;

    public GraphReaderService(IConfiguration configuration)
    {
        _clientId = configuration["AzureAd:ClientId"];
        _secret = configuration["AzureAd:ClientSecret"];
        _tenantId = configuration["AzureAd:TenantId"];
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var clientSecretCredential = new ClientSecretCredential(_tenantId, _clientId, _secret);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

        var clientApps = await graphClient.Applications.GetAsync(
            configuration => configuration.QueryParameters.Filter = "tags/Any(x: startswith(x, 'api-client'))");
        var apiProvidersApps = await graphClient.Applications.GetAsync(
            configuration => configuration.QueryParameters.Filter = "tags/Any(x: startswith(x, 'api-provider'))");

        var apiClients = clientApps.Value
            .Select(app => new ApiClient(app.AppId, app.DisplayName, app.RequiredResourceAccess)).ToList();
        var apiProviders = apiProvidersApps.Value
            .Select(app => new ApiProvider(app.AppId, app.DisplayName, app.AppRoles)).ToList();

        foreach (var client in apiClients)
        {
            foreach (var apiProvider in apiProviders)
            {
                client.AddApiProviderIfConnected(apiProvider);
            }
        }
        
        foreach (var client in apiClients)
        {
            Log.Information("Client: {ClientName} ({ClientAppId})", client.Name, client.AppId);
            foreach (var apiRoles in client.ConnectedRolesByApi)
            {
                Log.Information("  Api: {API}", apiRoles.Key);
                foreach (var role in apiRoles.Value)
                {
                    Log.Information("    Role: {Role}", role);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class ApiClient
{
    public string AppId { get; }
    public string Name { get; }

    public Dictionary<string, List<string>> ConnectedRolesByApi { get; } = new();
    public List<string> ConnectedApis { get; } = new();
    public List<string> ConnectedRoles { get; } = new();

    private readonly Dictionary<string, Guid> _connectedRoles = new();
    private readonly List<string> _connectedApis = new();

    private readonly Dictionary<string, ApiProvider> _apiProviders = new();

    public ApiClient(string appId, string name, List<RequiredResourceAccess> accesses)
    {
        AppId = appId;
        Name = name;

        foreach (var access in accesses)
        {
            _connectedApis.Add(access.ResourceAppId);
            foreach (var scope in access.ResourceAccess.Where(scope => scope.Type == "Role"))
            {
                _connectedRoles.Add(access.ResourceAppId, scope.Id.Value);
            }
        }
    }

    public void AddApiProviderIfConnected(ApiProvider apiProvider)
    {
        if (!_connectedApis.Contains(apiProvider.AppId))
            return;
        
        _apiProviders.Add(apiProvider.AppId, apiProvider);
        ConnectedRolesByApi.Add(apiProvider.Name, new List<string>());
        ConnectedApis.Add(apiProvider.Name);
        foreach (var role in apiProvider.AppRoles)
        {
            if (_connectedRoles.TryGetValue(apiProvider.AppId, out var roleId) && roleId == role.Key)
            {
                ConnectedRolesByApi[apiProvider.Name].Add(role.Value);
                ConnectedRoles.Add(role.Value);
            }
        }
    }
}

public class ApiProvider
{
    public string AppId { get; }
    public string Name { get; }
    public Dictionary<Guid, string> AppRoles { get; }

    public ApiProvider(string appId, string name, List<AppRole> roles)
    {
        AppId = appId;
        Name = name;
        AppRoles = roles.ToDictionary(role => role.Id.Value, role => role.Value);
    }
}