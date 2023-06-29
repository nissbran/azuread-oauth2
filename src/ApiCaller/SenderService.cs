using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApiCaller;

public class SenderService : IHostedService
{
    private readonly string? _clientId;
    private readonly string? _secret;
    private readonly string? _tenantId;
    private readonly string? _scope;
    private readonly string _tokenEndpoint;
    
    public SenderService(IConfiguration configuration)
    {
        _clientId = configuration["AzureAd:ClientId"];
        _secret = configuration["AzureAd:ClientSecret"];
        _tenantId = configuration["AzureAd:TenantId"];
        _scope = configuration["AzureAd:Scope"];
        _tokenEndpoint = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["client_id"] = _clientId,
            ["client_secret"] = _secret,
            ["scope"] = _scope,
            ["grant_type"] = "client_credentials"
        });
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<JwtResponse>();

        var apiClient = new HttpClient();
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content?.AccessToken);
        var getResponse = await apiClient.GetAsync("https://localhost:7080/api/health");
        var getValidateResponse = await apiClient.GetAsync("https://localhost:7080/api/health/with-role-validation");

        Console.ReadLine();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class JwtResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}