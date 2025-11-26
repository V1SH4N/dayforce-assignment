using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

public class JiraRemoteLinksService : IJiraRemoteLinksService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jiraBaseUrl;

    public JiraRemoteLinksService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraBaseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Deafult Atlassian base URL is not configured.");
    }

    public async Task<JsonElement> GetRemoteLinksAsync(string jiraId)
    {
        var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
        HttpResponseMessage response;

        try
        {
            response = await client.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraId}/remotelink"));
        }
        catch (HttpRequestException ex)
        {
            throw new JiraException($"Failed to connect to Jira API: {ex.Message}");
        }

        // Handle 200 OK
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (jsonResponse.ValueKind == JsonValueKind.Undefined)
            {
                throw new JiraApiException(200, $"Jira returned an empty response for remote links of issue {jiraId}.");
            }
            return jsonResponse;
        }

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.BadRequest: // 400
                var badContent = await response.Content.ReadAsStringAsync();
                throw new JiraBadRequestException($"Bad request to Jira API: {badContent}");

            case System.Net.HttpStatusCode.Unauthorized: // 401
                throw new JiraUnauthorizedException();

            case System.Net.HttpStatusCode.Forbidden: // 403
                throw new JiraForbiddenException();

            case System.Net.HttpStatusCode.NotFound: // 404
                throw new JiraRemoteLinksNotFoundException(jiraId);

            case System.Net.HttpStatusCode.RequestEntityTooLarge: // 413
                throw new JiraPayloadTooLargeException();

            default:
                var content = await response.Content.ReadAsStringAsync();
                throw new JiraApiException((int)response.StatusCode, $"Jira API error ({(int)response.StatusCode}): {content}");
        }
    }
}
