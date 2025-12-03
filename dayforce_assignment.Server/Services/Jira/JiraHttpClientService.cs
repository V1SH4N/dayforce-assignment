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


        // Get Jira Issue json 
        public async Task<JsonElement> GetIssueAsync(string jiraKey)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraKey}?expand=names"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                {
                    throw new JiraApiException((int)response.StatusCode, $"Jira returned an empty response for issue {jiraKey}.");
                }

                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new JiraUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new JiraIssueNotFoundException(jiraKey);

            throw new JiraApiException((int)response.StatusCode, $"Unexpected Jira API error");
        }


        // Get Jira Issue Remote Links json
        public async Task<JsonElement> GetIssueRemoteLinksAsync(string jiraKey)
        {
            var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            HttpResponseMessage response = await client.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraKey}/remotelink"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
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

            throw new JiraApiException((int)response.StatusCode, $"Unexpected Jira API error.");
        }



    }
}
