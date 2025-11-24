using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageReferenceExtractor : IConfluencePageReferenceExtractor
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;

        public ConfluencePageReferenceExtractor(
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService)
        {
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraIssueDto jiraStory)
        {
            var history = new ChatHistory();

            string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageReferenceExtractor.txt");

            history.AddSystemMessage(systemPrompt);

            history.AddUserMessage(JsonSerializer.Serialize(jiraStory));

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dto = JsonSerializer.Deserialize<ConfluencePageReferencesDto>(jsonResponse, options);

            return dto;
           
        }
    }
}
