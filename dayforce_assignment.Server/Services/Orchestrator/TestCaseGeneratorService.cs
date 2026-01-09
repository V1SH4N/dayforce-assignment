using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.EventSinks;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService : ITestCaseGeneratorService
    {
        private readonly IJiraHttpClientService _jiraHttpClietService;
        private readonly IJiraMapperService _jiraMapperService;
        private readonly IUserPromptBuilder _userPromptBuilder;
        private readonly IChatCompletionService _chatCompletionService;

        public TestCaseGeneratorService(
            IJiraHttpClientService jiraHttpClietService,
            IJiraMapperService jiraMapperService,
            IUserPromptBuilder userPromptBuilder,
            IChatCompletionService chatCompletionService)
        {
            _jiraHttpClietService = jiraHttpClietService;
            _jiraMapperService = jiraMapperService;
            _userPromptBuilder = userPromptBuilder;
            _chatCompletionService = chatCompletionService;
        }


        public async Task GenerateTestCasesAsync(string jirakey, ISseEventSink events, CancellationToken cancellationToken)
        {

            // Get Jira issue dto.

            if (string.IsNullOrWhiteSpace(jirakey))
                throw new ArgumentException("Jira key must be provided", nameof(jirakey));

            JsonElement jsonJiraIssue = await _jiraHttpClietService.GetIssueAsync(jirakey, cancellationToken);

            var jsonJiraRemoteLinks = new JsonElement();
            try
            {
                jsonJiraRemoteLinks = await _jiraHttpClietService.GetIssueRemoteLinksAsync(jirakey, cancellationToken);
            }
            catch (DomainException) { }

            JiraIssueDto jiraIssue = _jiraMapperService.MapIssueToDto(jsonJiraIssue, jsonJiraRemoteLinks);

            bool isBugIssue = (jiraIssue.IssueType == IssueType.Bug);

            await events.JiraFetchedAsync(jiraIssue.Key, jiraIssue.Title, cancellationToken);




            // Generate test cases.

            string systemPromptPath = "SystemPrompts/TestCaseGenerator.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath, cancellationToken);

            bool tokenExceeded =  await TryGenerateAsync(
                jiraIssue,
                isBugIssue,
                summarizeAttachment: false,
                systemPrompt,
                events,
                cancellationToken
            );

            if (tokenExceeded == true)
            {
                tokenExceeded = await TryGenerateAsync(
                    jiraIssue,
                    isBugIssue,
                    summarizeAttachment: true,
                    systemPrompt,
                    events,
                    cancellationToken
                );

                if (tokenExceeded == true)
                    throw new TestCaseGenerationException(jirakey, "Failed to generate test cases. Token limit exceeded");
            }

        }




        // Adds system message and user message to chat history, then calls AI model to stream chat content.
        // Returns True if token limit exceeded.
        // Returns False if test cases successfully generated.
        private async Task<bool> TryGenerateAsync(
            JiraIssueDto jiraIssue,
            bool isBug,
            bool summarizeAttachment,
            string systemPrompt,
            ISseEventSink events,
            CancellationToken cancellationToken
            )
        {
            var history = new ChatHistory();
            history.AddSystemMessage(systemPrompt);

            var userPrompt = await _userPromptBuilder.BuildAsync(jiraIssue, isBug, summarizeAttachment, events, cancellationToken);
            history.AddUserMessage(userPrompt);

            try
            {
                var buffer = new StringBuilder();
                int startIndex = -1;
                int endIndex = -1;

                await foreach (var token in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory: history,cancellationToken: cancellationToken))
                {

                    if (string.IsNullOrEmpty(token.Content))
                        continue;

                    Console.Write($"\n{token.Content.ToString()}");

                    buffer.Append(token.Content);
                    string bufferString = buffer.ToString();

                    // Logic to parse individual test cases from streaming chat content.
                    while (true)
                    {
                        startIndex = bufferString.IndexOf('{');
                        endIndex = bufferString.IndexOf('}');

                        if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                            break;

                        string testCase = bufferString.Substring(startIndex, endIndex - startIndex + 1);
                        JsonElement jsonTestCase = JsonSerializer.Deserialize<JsonElement>(testCase);

                        await events.TestCaseGeneratedAsync(jsonTestCase, cancellationToken);

                        buffer.Remove(startIndex, endIndex - startIndex + 1);
                        bufferString = buffer.ToString();
                    }
                }
            }
            catch (HttpOperationException ex) when (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 413) // Error when chatHistory exceeds token limit
            {
                return true;
            }
            await events.TestCasesFinishedAsync(cancellationToken);
            return false;
        }
    }
}
