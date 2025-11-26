using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService : ITestCaseGeneratorService
    {
        private readonly IJiraIssueService _jiraIssueService;
        private readonly IJiraRemoteLinksService _jiraRemoteLinksService;
        private readonly IJiraIssueMapper _jiraIssueMapper;
        private readonly IUserPromptBuilder _userPromptBuilder;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;

        public TestCaseGeneratorService(
            IJiraIssueService jiraIssueService,
            IJiraRemoteLinksService jiraRemoteLinksService,
            IJiraIssueMapper jiraIssueMapper,
            IUserPromptBuilder userPromptBuilder,
            IChatCompletionService chatCompletionService,
            IJsonFormatterService jsonFormatterService)
        {   
            _jiraIssueService = jiraIssueService;
            _jiraRemoteLinksService = jiraRemoteLinksService;
            _jiraIssueMapper = jiraIssueMapper;
            _userPromptBuilder = userPromptBuilder;
            _chatCompletionService = chatCompletionService;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<JsonElement> GenerateTestCasesAsync(string jirakey)
        {

            if (string.IsNullOrWhiteSpace(jirakey)) 
            {
                throw new ArgumentException("Jira key must be provided", nameof(jirakey));
            }

            try
            {
                var history = new ChatHistory();

                // System prompt
                string systemPrompt = File.ReadAllText("SystemPrompts/TestCaseGeneratorV6.txt");
                history.AddSystemMessage(systemPrompt);

                // Get Jira issue json with remote links
                var jsonJiraIssueTask = _jiraIssueService.GetIssueAsync(jirakey);
                var jsonJiraRemoteLinksTask = _jiraRemoteLinksService.GetRemoteLinksAsync(jirakey);
                await Task.WhenAll(jsonJiraIssueTask, jsonJiraRemoteLinksTask);

                // Map issue json to DTO
                JiraIssueDto jiraIssue = _jiraIssueMapper.MapToDto(jsonJiraIssueTask.Result, jsonJiraRemoteLinksTask.Result);

                // User prompt
                bool isBugIssue = (jiraIssue.IssueType == IssueType.Bug);
                ChatMessageContentItemCollection userMessage = await _userPromptBuilder.BuildAsync(jiraIssue, isBugIssue);
                history.AddUserMessage(userMessage);

                var response = await _chatCompletionService.GetChatMessageContentAsync(history);
                var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());
                return jsonResponse;

            }
            catch (Exception ex) when (!(ex is DomainException))
            {
                throw new TestCaseGenerationException(jirakey, "An unexpected error has occurred.");
            }
        }
    }
}
