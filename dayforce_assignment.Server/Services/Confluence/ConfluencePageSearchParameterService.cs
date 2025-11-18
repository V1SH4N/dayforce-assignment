using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchParameterService : IConfluencePageSearchParameterService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;

        public ConfluencePageSearchParameterService(
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService)
        {
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<ConfluenceSearchParametersDto> GetSearchParametersAsync(JiraStoryDto jiraStory)
        {
            if (jiraStory == null)
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid Jira story input",
                    "JiraStoryDto cannot be null",
                    internalMessage: "Received null JiraStoryDto in GetSearchParametersAsync");

            var history = new ChatHistory();

            string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSearchParameter.txt");

            history.AddSystemMessage(systemPrompt);

            try
            {
                string jiraStoryString = JsonSerializer.Serialize(jiraStory);
                history.AddUserMessage(jiraStoryString);

                var response = await _chatCompletionService.GetChatMessageContentAsync(history);

                if (response == null)
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "AI response was empty",
                        internalMessage: "ChatCompletionService returned null response");

                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var searchParameters = JsonSerializer.Deserialize<ConfluenceSearchParametersDto>(jsonResponse, options);

                if (searchParameters == null || searchParameters.SearchParameters == null)
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "Failed to parse search parameters from AI response",
                        internalMessage: $"Deserialization returned null for response: {response}");

                return searchParameters;
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
                    "Failed to generate Confluence search parameters",
                    internalMessage: ex.ToString());
            }
        }
    }
}
