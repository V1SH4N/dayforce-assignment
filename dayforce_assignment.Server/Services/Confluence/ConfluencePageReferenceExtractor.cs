using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
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
            _chatCompletionService = chatCompletionService
                ?? throw new ArgumentNullException(nameof(chatCompletionService));

            _jsonFormatterService = jsonFormatterService
                ?? throw new ArgumentNullException(nameof(jsonFormatterService));
        }

        public async Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraStoryDto jiraStory)
        {
            if (jiraStory == null)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid Jira story input",
                    internalMessage: "jiraStory is null in GetConfluencePageReferencesAsync");
            }

            try
            {
                var history = new ChatHistory();


                string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageReferenceExtractor.txt");

                history.AddSystemMessage(systemPrompt);

                string jiraStoryJson = JsonSerializer.Serialize(jiraStory);
                history.AddUserMessage(jiraStoryJson);

                var response = await _chatCompletionService.GetChatMessageContentAsync(history);

                if (response == null)
                {
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "AI response was empty",
                        internalMessage: "ChatCompletionService returned null in GetConfluencePageReferencesAsync");
                }

                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<ConfluencePageReferencesDto>(jsonResponse, options);

                if (dto == null)
                {
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "Failed to parse Confluence page reference results",
                        internalMessage: $"Deserialization returned null for response: {jsonResponse}");
                }

                return dto;
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Error parsing AI JSON response",
                    internalMessage: ex.ToString());
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Failed to extract Confluence page references",
                    internalMessage: ex.ToString());
            }
        }
    }
}
