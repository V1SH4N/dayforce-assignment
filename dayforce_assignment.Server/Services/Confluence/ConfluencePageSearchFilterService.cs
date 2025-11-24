using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchFilterService : IConfluencePageSearchFilterService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;

        public ConfluencePageSearchFilterService(
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService)
        {
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<ConfluenceSearchResultsDto> FilterSearchResultAsync(JiraIssueDto jiraStory, ConfluenceSearchResultsDto searchResults)
        {

            var history = new ChatHistory();

            string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSearchFilterV2.txt");

            history.AddSystemMessage(systemPrompt);

            history.AddUserMessage(JsonSerializer.Serialize(jiraStory));
            history.AddUserMessage(JsonSerializer.Serialize(searchResults));

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var filteredSearchResults = JsonSerializer.Deserialize<ConfluenceSearchResultsDto>(jsonResponse, options);

            return filteredSearchResults;
        }
    }
}
