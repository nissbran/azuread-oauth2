using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Hello, Oauth 2 caller!");

const string clientId = "<client id of your client app registration";
const string secret = "<client secret of your client app registration>";
const string tenantId = "<tenant id>";
const string scope = "<app id of the application the client are calling>/.default";
const string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["client_id"] = clientId,
    ["client_secret"] = secret,
    ["scope"] = scope,
    ["grant_type"] = "client_credentials"
});
var response = await client.SendAsync(request);
var content = await response.Content.ReadFromJsonAsync<JwtResponse>();

var apiClient = new HttpClient();
apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content?.AccessToken);
var getResponse = await apiClient.GetAsync("https://localhost:7080/api/health");
var getValidateResponse = await apiClient.GetAsync("https://localhost:7080/api/health/with-role-validation");

Console.ReadLine();

public class JwtResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}