using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
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
            _chatCompletionService = chatCompletionService 
                ?? throw new ArgumentNullException(nameof(chatCompletionService));
            _jsonFormatterService = jsonFormatterService 
                ?? throw new ArgumentNullException(nameof(jsonFormatterService));
        }

        public async Task<ConfluenceSearchResultsDto> FilterSearchResultAsync(
            JiraStoryDto jiraStory, 
            ConfluenceSearchResultsDto searchResults)
        {
            if (jiraStory == null)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid Jira story input",
                    internalMessage: "jiraStory is null in FilterSearchResultAsync");
            }

            if (searchResults == null)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid search results input",
                    internalMessage: "searchResults is null in FilterSearchResultAsync");
            }

            try
            {
                var history = new ChatHistory();

                string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSearchFilter.txt");

                history.AddSystemMessage(systemPrompt);

                history.AddUserMessage(JsonSerializer.Serialize(jiraStory));
                history.AddUserMessage(JsonSerializer.Serialize(searchResults));

                var response = await _chatCompletionService.GetChatMessageContentAsync(history);

                if (response == null)
                {
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "AI response was empty",
                        internalMessage: "ChatCompletionService returned null in FilterSearchResultAsync");
                }

                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var filteredSearchResults = JsonSerializer.Deserialize<ConfluenceSearchResultsDto>(jsonResponse, options);

                if (filteredSearchResults == null)
                {
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "Failed to parse filtered search results",
                        internalMessage: $"Deserialization returned null for response: {response}");
                }

                return filteredSearchResults;
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
                    "Failed to filter Confluence search results",
                    internalMessage: ex.ToString());
            }
        }
    }
}
