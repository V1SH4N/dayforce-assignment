using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

public class JiraIssueService : IJiraIssueService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jiraBaseUrl;

    public JiraIssueService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraBaseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured.");
    }

    public async Task<JsonElement> GetIssueAsync(string jiraId)
    {
        var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraId}?expand=names"));
        }
        catch (HttpRequestException ex)
        {
            throw new JiraException($"Failed to connect to Jira API: {ex.Message}");
        }

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (jsonResponse.ValueKind == JsonValueKind.Undefined)
            {
                throw new JiraApiException(200, $"Jira returned an empty response for issue {jiraId}.");
            }
            return jsonResponse;
        }

        // 401 Unauthorized
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new JiraUnauthorizedException();
        }

        // 404 Not Found
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new JiraIssueNotFoundException(jiraId);
        }

        // Other 4xx/5xx errors
        var content = await response.Content.ReadAsStringAsync();
        throw new JiraApiException((int)response.StatusCode, $"Jira API error ({(int)response.StatusCode}): {content}");
    }
}
