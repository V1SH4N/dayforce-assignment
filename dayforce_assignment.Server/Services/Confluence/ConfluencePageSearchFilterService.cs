using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchFilterService : IConfluencePageSearchFilterService
    {
        private readonly IChatCompletionService _chatCompletionService;

        public ConfluencePageSearchFilterService(IChatCompletionService chatCompletionService)
        {
            _chatCompletionService = chatCompletionService;
        }

        public async Task<ConfluenceSearchResultsDto> FilterSearchResultAsync(JiraStoryDto jiraStory, ConfluenceSearchResultsDto searchResults)
        {
            var history = new ChatHistory("""
                    You are an AI assistant that filters Confluence pages based on a Jira story.

                    Task:
                    1. You will receive a Jira story (title, description, acceptance criteria) in JSON.
                    2. You will also receive a list of Confluence pages (each with id and title) in JSON.
                    3. Identify which pages are relevant to the Jira story based on the title.
                    4. Output **only a JSON object** with the relevant pages, including Id and Title.

                    Output format:
                    {
                      "ConfluencePagesMetadata": [
                        { "id": "<relevant_page_id>", "title": "<page_title>" },
                        { "id": "<another_relevant_page_id>", "title": "<page_title>" }
                      ]
                    }

                    Rules:
                    - Include only pages that appear to be relevant. If unsure, include pages that might match closely.
                    - Do not include extra text outside the JSON object.
                    - Preserve the original titles in the output.
                
                """);
            history.AddSystemMessage("temp");

            history.AddUserMessage(JsonSerializer.Serialize(jiraStory));
            history.AddUserMessage(JsonSerializer.Serialize(searchResults));

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);
            var text = response?.ToString()?.Trim();

            var filteredSearchResults = new ConfluenceSearchResultsDto();

            filteredSearchResults = JsonSerializer.Deserialize<ConfluenceSearchResultsDto>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ConfluenceSearchResultsDto();

            return filteredSearchResults;
        }
    }
}
