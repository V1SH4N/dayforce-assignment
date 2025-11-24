using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

public class JiraRemoteLinksService : IJiraRemoteLinksService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jiraBaseUrl;

    public JiraRemoteLinksService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraBaseUrl = configuration["Atlassian:BaseUrl"];
    }

    public async Task<JsonElement> GetJiraRemoteLinksAsync(string jiraId)
    {
        var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

        var response = await client.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraId}/remotelink"));

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

        return jsonResponse;
    }       
}
