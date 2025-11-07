using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageReferenceExtractor : IConfluencePageReferenceExtractor
    {
        private readonly Kernel _kernel;

        public ConfluencePageReferenceExtractor(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<ConfluencePageReferenceDto> GetConfluencePageReferencesAsync(JiraStoryDto jiraStory)
        {
            var kernelInstance = _kernel.Clone();
            var chatService = kernelInstance.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage("""
                The user will upload a Jira user story in JSON format.

                Your task:
                - Identify any Confluence page URLs included in the story.
                - Each Confluence page URL will have a numeric page ID, for example:
                  https://dayforce.atlassian.net/wiki/spaces/CAM/pages/381158878/Camelot+CI+CD+Process
                  → The page ID is 381158878.

                Output requirements:
                - You must return a **strict JSON object**.
                - Each entry must contain:
                  - `baseUrl`: the base Confluence URL (e.g. "https://dayforce.atlassian.net")
                  - `pageId`: the extracted numeric ID as a string.

                Example output:
                {
                  "confluencePages": [
                    {
                      "baseUrl": "https://dayforce.atlassian.net/",
                      "pageId": "381158878"
                    },
                    {
                      "baseUrl": "https://dayforce.atlassian.net/",
                      "pageId": "123456789"
                    }
                  ]
                }

                Return only the JSON — no explanations, text, or markdown formatting.
                """);

            string jiraStoryJson = JsonSerializer.Serialize(jiraStory);
            history.AddUserMessage(jiraStoryJson);

            var result = await chatService.GetChatMessageContentAsync(history);
            var text = result?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("LLM returned no content.");

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<ConfluencePageReferenceDto>(text, options);

                if (dto == null)
                    throw new JsonException("Failed to deserialize ConfluencePageIdDto.");

                return dto;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse LLM response: {text}", ex);
            }
        }
    }
}
