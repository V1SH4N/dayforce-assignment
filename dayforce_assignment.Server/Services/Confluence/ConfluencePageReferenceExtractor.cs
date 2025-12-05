using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
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

        // Extracts confluence page link references from Jira issue information. Returns new ConfluencePageReferencesDto initialized with empty list if not found.
        public async Task<ConfluencePageReferencesDto> GetReferencesAsync(JiraIssueDto jiraIssue)
        {
            var dto = new ConfluencePageReferencesDto();
            var history = new ChatHistory();

            // Load system prompt
            string systemPromptPath = "SystemPrompts/ConfluencePageReferenceExtractor.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath);

            history.AddSystemMessage(systemPrompt);

            history.AddUserMessage(JsonSerializer.Serialize(jiraIssue));


            var response = new ChatMessageContent();

            try
            {
                response = await _chatCompletionService.GetChatMessageContentAsync(history);
            }
            catch (Exception)
            {
                throw new ConfluencePageReferenceExtractionException($"Failed to extract confluence page references from Jira issue {jiraIssue.Key}");
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
            catch (JsonException ex)
            {
                throw new ConfluencePageReferenceExtractionException($"Failed to parse Json response from AI for confluence page references for Jira issue {jiraIssue.Key}");
            }

        }
    }
}
