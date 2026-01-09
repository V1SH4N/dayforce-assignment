using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class JiraHttpClientService : IJiraHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _jiraBaseUrl;

        public JiraHttpClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<JiraHttpClientService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _jiraBaseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured.");
        }


        // Gets Jira Issue json.
        // Throws exception if not found. 
        public async Task<JsonElement> GetIssueAsync(string jiraKey, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraKey}?expand=names"), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                {
                    throw new JiraApiException($"Jira returned an empty response for issue {jiraKey}.");
                }

                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new JiraUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new JiraIssueNotFoundException(jiraKey);

            throw new JiraApiException($"Unexpected Jira API error");
        }


        // Gets Jira Issue Remote Links json.
        // Throws exception if not found.
        public async Task<JsonElement> GetIssueRemoteLinksAsync(string jiraKey, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            HttpResponseMessage response = await client.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraKey}/remotelink"), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new JiraUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new JiraRemoteLinksNotFoundException(jiraKey);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new JiraRemoteLinksForbiddenException(jiraKey);

            if (response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
                throw new JiraRemoteLinksPayloadTooLargeException(jiraKey);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new JiraBadRequestException(jiraKey);
            }

            throw new JiraApiException($"Unexpected Jira API error.");
        }



    }
}
