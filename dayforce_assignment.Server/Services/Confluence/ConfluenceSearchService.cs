using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceSearchService: IConfluenceSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;
        private readonly string _baseUrl;
        private readonly string _space;

        public ConfluenceSearchService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService)
        {
            _httpClientFactory = httpClientFactory;
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
            _baseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured.");
            _space = configuration["Atlassian:DefaultConfluenceSpace"] ?? throw new AtlassianConfigurationException("Default Atlassian Confluence space is not configured.");
        }



        // Searches for confluence pages.
        public async Task<JsonElement> SearchPageAsync(string cql, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            string finalCql = $"type = page AND space = \"{_space}\" AND ({cql})";
            string encodedCql = Uri.EscapeDataString(finalCql);

            var baseUri = new Uri(_baseUrl);
            var url = $"wiki/rest/api/content/search?cql={encodedCql}&limit=10";

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(baseUri, url), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    throw new ConfluenceApiException($"Confluence search returned an empty response.");
                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ConfluenceUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new ConfluenceSearchBadRequestException();
            }

            throw new ConfluenceApiException($"Unexpected Confluence search API error.");

        }



        // Gets search paramters.
        // Returns new ConfluenceSearcParametersDto initialized with empty list if no parameters found.
        public async Task<ConfluenceSearchParametersDto> GetParametersAsync(JiraIssueDto jiraIssue, CancellationToken cancellationToken)
        {
            
                var dto = new ConfluenceSearchParametersDto();

                var history = new ChatHistory();

                string systemPromptPath = "SystemPrompts/ConfluencePageSearchParameter.txt";

                if (!File.Exists(systemPromptPath))
                    throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

                string systemPrompt = await File.ReadAllTextAsync(systemPromptPath, cancellationToken);

                history.AddSystemMessage(systemPrompt);

                history.AddUserMessage(JsonSerializer.Serialize(jiraIssue));

                var response = new ChatMessageContent();

            try
                {
                    response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory: history, cancellationToken:cancellationToken);
                }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
                {
                    throw new ConfluenceSearchParameterException($"Failed to get search parameters from AI for Jira issue {jiraIssue.Key}");
                }

            try
            {
                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                dto = JsonSerializer.Deserialize<ConfluenceSearchParametersDto>(jsonResponse, options);

                return dto ?? new ConfluenceSearchParametersDto();
            }
            catch (JsonException)
            {
                throw new ConfluenceSearchParameterException($"Failed to parse Json response from AI for confluence search parameters for Jira issue {jiraIssue.Key}");
            }
        }



        // Filters search result.
        // Returns new ConfluenceSearchResultsDto initialized with empty list if no relevant confluence pages found.
        public async Task<ConfluencePageReferencesDto> FilterResultAsync(JiraIssueDto jiraIssue, ConcurrentDictionary<string, ConfluencePage> searchResults, CancellationToken cancellationToken)
        {
            if (searchResults.IsEmpty)
                return new ConfluencePageReferencesDto();

            var dto = new ConfluencePageReferencesDto();

            var history = new ChatHistory();

            string systemPromptPath = "SystemPrompts/ConfluencePageSearchFilter.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath, cancellationToken);


            history.AddSystemMessage(systemPrompt);

            history.AddUserMessage(JsonSerializer.Serialize(jiraIssue));

            history.AddUserMessage(JsonSerializer.Serialize(searchResults));

            var response = new ChatMessageContent();

            try
            {
                response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory: history, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new ConfluenceSearchFilterException($"Failed to get filtered search result from AI for Jira issue {jiraIssue.Key}");
            }

            try
            {
                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                dto = JsonSerializer.Deserialize<ConfluencePageReferencesDto>(jsonResponse, options);

                return dto ?? new ConfluencePageReferencesDto();
            }
            catch (JsonException)
            {
                throw new ConfluenceSearchFilterException($"Failed to parse Json response from AI for filtered search results for Jira issue {jiraIssue.Key}");
            }
        }


    }
}


