using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchParameterService : IConfluencePageSearchParameterService
    {
        private readonly IChatCompletionService _chatCompletionService;
        public ConfluencePageSearchParameterService(IChatCompletionService chatCompletionService)
        {
            _chatCompletionService = chatCompletionService;
        }

        public async Task<ConfluenceSearchParametersDto> GetSearchParametersAsync(JiraStoryDto jiraStory)
        {
            var history = new ChatHistory();

            history.AddSystemMessage("""
                You are an AI assistant that specializes in generating Confluence CQL title search queries from Jira stories.

                ### Goal
                Given a Jira story (JSON with "title", "description", and "acceptanceCriteria"), generate **3–5 valid Confluence CQL title query strings** targeting the page title field.  
                Output must be valid JSON with property name `"SearchParameters"`. Each query should maximize match probability while being practical for searching existing pages.

                ---

                ### Instructions

                1. **Keyword Extraction**
                   - Extract 3–5 of the most meaningful keywords or multi-word phrases from the Jira story.
                   - Prioritize:
                     - Feature names
                     - Technical or business concepts
                     - Acronyms and CamelCase identifiers
                     - Proper nouns or domain-specific phrases
                   - Avoid generic filler words (e.g., “update”, “fix”, “issue”, “page”, “requirement”), unless essential.

                2. **CamelCase Handling**
                   - Always generate a CamelCase version for each keyword or multi-word phrase **if reasonably short**.
                   - Example: “user management” → “UserManagement”.
                   - If the phrase is long or unlikely to exist as CamelCase, include only the normal form.
                   - Every query should include both **normal and CamelCase variants** combined with OR.

                3. **Query Rules**
                   - Use wildcards (`*`) to increase recall:
                     - `(title="<keyword>*" OR title="*<keyword>" OR title="<CamelCaseKeyword>*" OR title="*<CamelCaseKeyword>")`
                   - Combine multiple keywords in a query using `OR` to produce **variants for broader matches**.
                   - Order queries from **most specific → broader**.
                   - Always include multiple variants for each search parameter to improve coverage.

                4. **Mandatory Output**
                   - Always generate **between 3 and 5 queries**, never fewer.
                   - Output JSON **must start with `{` and end with `}`**.
                   - **Do not include any text, markdown formatting, or explanation**.

                5. Do not include any text, markdown formatting, code fences, or explanations before or after the JSON.
                   - No ```json
                   - No extra text
                   - Output must start directly with { and end with }

                6. **Output Format**
                {
                  "SearchParameters": [
                    "(title~\"<keyword1>*\" OR title~\"*<keyword1>\" OR title~\"<CamelCaseKeyword1>*\" OR title~\"*<CamelCaseKeyword1>\")",
                    "(title~\"<keyword2>*\" OR title~\"*<keyword2>\" OR title~\"<CamelCaseKeyword2>*\" OR title~\"*<CamelCaseKeyword2>\")",
                    "(title~\"<keyword3>*\" OR title~\"*<keyword3>\" OR title~\"<CamelCaseKeyword3>*\" OR title~\"*<CamelCaseKeyword3>\")"
                  ]
                }
                
                """);


            string jiraStoryString = JsonSerializer.Serialize(jiraStory);
            history.AddUserMessage(jiraStoryString);

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);
            var text = response?.ToString()?.Trim();

            var searchParameters = new ConfluenceSearchParametersDto();

            searchParameters = JsonSerializer.Deserialize<ConfluenceSearchParametersDto>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ConfluenceSearchParametersDto();

            return searchParameters;
        }
    }
}
