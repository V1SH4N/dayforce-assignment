using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

public class JiraStoryService : IJiraStoryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jiraBaseUrl;

    public JiraStoryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraBaseUrl = configuration["Jira:BaseUrl"];
    }

    public async Task<JsonElement> GetJiraStoryAsync(string jiraId)
    {
        var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

        var response = await httpClient.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraId}"));

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

        return jsonResponse; 
    }
}
