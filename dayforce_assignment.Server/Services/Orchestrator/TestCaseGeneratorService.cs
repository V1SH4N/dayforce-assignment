using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService : ITestCaseGeneratorService
    {
        private readonly IJiraHttpClientService _jiraHttpClietService;
        private readonly IJiraMapperService _jiraMapperService;
        private readonly IUserPromptBuilder _userPromptBuilder;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;
        private readonly ILogger<TestCaseGeneratorService> _logger;

        public TestCaseGeneratorService(
            IJiraHttpClientService jiraHttpClietService,
            IJiraMapperService jiraMapperService,
            IUserPromptBuilder userPromptBuilder,
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService,
            ILogger<TestCaseGeneratorService> logger)
        {
            _jiraHttpClietService = jiraHttpClietService;
            _jiraMapperService = jiraMapperService;
            _userPromptBuilder = userPromptBuilder;
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
            _logger = logger;
        }

        public async Task<JsonElement> GenerateTestCasesAsync(string jirakey)
        {
            // Jira key validation
            if (string.IsNullOrWhiteSpace(jirakey))
                throw new ArgumentException("Jira key must be provided", nameof(jirakey));

            // Load system prompt
            string systemPromptPath = "SystemPrompts/TestCaseGenerator.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath);


            // Fetch Jira issue json with remote links
            JsonElement jsonJiraIssue = await _jiraHttpClietService.GetIssueAsync(jirakey);
            JsonElement jsonJiraRemoteLinks = new JsonElement();
            try
            {
                jsonJiraRemoteLinks = await _jiraHttpClietService.GetIssueRemoteLinksAsync(jirakey);
            }
            catch (Exception ex) when (ex is DomainException)
            {
                _logger.LogWarning(ex.Message, "Skipping Jira remote links.");
            }

            JiraIssueDto jiraIssue = _jiraMapperService.MapIssueToDto(jsonJiraIssue, jsonJiraRemoteLinks);

            bool isBugIssue = (jiraIssue.IssueType == IssueType.Bug);


            // Generate test cases with retry logic
            var response = await TryGenerateAsync(
                jiraIssue,
                isBugIssue,
                summarizeAttachment: false,
                systemPrompt
            );

            if (response == null)
            {
                _logger.LogWarning("Token limit exceeded. Retrying with summarizeAttachment = true.");

                response = await TryGenerateAsync(
                    jiraIssue,
                    isBugIssue,
                    summarizeAttachment: true,
                    systemPrompt
                );

                if (response == null)
                    throw new TestCaseGenerationException(jirakey, "Failed to generate test cases. Token limit exceeded");
            }

            // Format and return JSON response
            JsonElement jsonResponse = _jsonFormatterService.FormatJson(response.ToString());
            return jsonResponse;
        }



        // Generates test cases
        private async Task<ChatMessageContent?> TryGenerateAsync(
           JiraIssueDto jiraIssue,
           bool isBug,
           bool summarizeAttachment,
           string systemPrompt)
        {
            // Add system prompt
            var history = new ChatHistory();
            history.AddSystemMessage(systemPrompt);

            // Add user prompt
            ChatMessageContentItemCollection userPrompt = await _userPromptBuilder.BuildAsync(jiraIssue, isBug, summarizeAttachment);
            history.AddUserMessage(userPrompt);

            try
            {
                // LLM call
                return await _chatCompletionService.GetChatMessageContentAsync(history);
            }
            catch (HttpOperationException ex) when (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 413) // Error when chatHistory exceeds token limit
            {
                return null;
            }
        }
    }
}
