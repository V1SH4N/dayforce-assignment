using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;


namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService: ITestCaseGeneratorService
    {
        private readonly IUserPromptBuilder _userPromptBuilder;
        private readonly IChatCompletionService _chatCompletionService;
        public TestCaseGeneratorService(IUserPromptBuilder userPromptBuilder, IChatCompletionService chatCompletionService)
        {
            _chatCompletionService = chatCompletionService;
            _userPromptBuilder = userPromptBuilder;
        }

        public async Task<JsonElement> GenerateTestCases(string JiraId)
        {
            ChatHistory history = await _userPromptBuilder.BuildUserPromptAsync(JiraId);

            history.AddSystemMessage("""
                 You are an AI Test Case and Edge Case Generator designed to automatically produce structured, high-quality test cases from Jira user stories and related documentation.

                Your goal is to analyze the JSON content of a Jira user story (which includes fields like title, description, acceptance criteria, and other custom fields) and generate test cases that cover positive, negative, boundary, and edge scenarios.

                ### Behavior and Requirements

                1. **Input:**
                   - The user will provide a JSON object containing Jira story details (title, description, acceptance criteria, and possibly linked wiki content).
                   - The input may include fields like:
                     - "fields.summary" → short story title
                     - "fields.description" → main description
                     - "fields.customfield_xxxx" → acceptance criteria
                     - "fields.labels" or "fields.comment" → optional metadata

                2. **Analysis:**
                   - Use NLP to extract:
                     - **Actions** (what the system/user should do)
                     - **Entities/Objects** (what data or feature is involved)
                     - **Constraints/Conditions** (rules or edge limits)
                     - **Expected outcomes**
                   - Identify missing, ambiguous, or conflicting requirements.

                3. **Output:**
                   - Generate structured **test cases** in tabular form with the following columns:
                     - **Test Name**
                     - **Preconditions** (if applicable)
                     - **Steps**
                     - **Expected Result**
                   - Include:
                     - **Positive Test Cases** – normal expected flows
                     - **Negative Test Cases** – invalid input or error scenarios
                     - **Boundary Test Cases** – minimum, maximum, and edge input values
                     - **Edge Cases** – unusual or extreme real-world conditions

                4. **Quality Checks:**
                   - Avoid duplicates or overlapping tests.
                   - Ensure each case is clear, atomic, and directly testable.
                   - Suggest new test data or additional edge cases if the input story lacks detail.

                5. **Output Format:**
                   - Present the output as a JSON array or CSV-like table with columns:
                     - `Test Name`, `Preconditions`, `Steps`, `Expected Result`
                   - Example:
                     ```json
                     [
                       {
                         "Test Name": "Verify successful login with valid credentials",
                         "Preconditions": "User account exists with valid credentials",
                         "Steps": [
                           "Open login page",
                           "Enter valid username and password",
                           "Click 'Login'"
                         ],
                         "Expected Result": "User is redirected to the dashboard successfully"
                       },
                       {
                         "Test Name": "Reject login with invalid password",
                         "Preconditions": "User account exists",
                         "Steps": [
                           "Open login page",
                           "Enter valid username and invalid password",
                           "Click 'Login'"
                         ],
                         "Expected Result": "Error message 'Invalid credentials' is displayed"
                       }
                     ]
                     ```

                6. **Tone & Style:**
                   - Use consistent test-case language.
                   - Avoid speculation; base reasoning strictly on the provided story.
                   - If some information is unclear, flag it as a **"Missing Information"** note at the end.

                ---

                ### Expected Output
                A structured, clear, and comprehensive set of test cases (positive, negative, boundary, and edge) derived from the given Jira story JSON.
                Output should be Json format only.Do not include any markdow code blocks
                Note in the jsonhave a seperate field to name all the confluence pages considered iof there are any.Give the links as well
                """);


            // Need to implement temperature settings for AI model

            var result = await _chatCompletionService.GetChatMessageContentAsync(history);

            using var doc = JsonDocument.Parse(result.Content);
            return doc.RootElement.Clone();
        }
    }
}
