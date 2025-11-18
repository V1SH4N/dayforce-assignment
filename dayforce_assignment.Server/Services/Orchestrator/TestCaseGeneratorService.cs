using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Diagnostics;


namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService: ITestCaseGeneratorService
    {
        private readonly IUserMessageBuilder _userPromptBuilder;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;
        public TestCaseGeneratorService(IUserMessageBuilder userPromptBuilder, IChatCompletionService chatCompletionService, IJsonFormatterService jsonFormatterService)
        {
            _chatCompletionService = chatCompletionService;
            _userPromptBuilder = userPromptBuilder;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<JsonElement> GenerateTestCasesAsync(string JiraId)
        {
            // stopwatch to measure time taken
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var history = new ChatHistory();

            string systemPrompt = File.ReadAllText("SystemPrompts/TestCaseGenerator.txt");

            history.AddSystemMessage(systemPrompt);
           
            ChatMessageContentItemCollection userMessage = await _userPromptBuilder.BuildUserMessageAsync(JiraId);

            history.AddUserMessage(userMessage);

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

            stopwatch.Stop(); // Stops the time measurement
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine($"[Timer] Elapsed time: {elapsed.TotalMilliseconds} ms");

            return jsonResponse;

        }
    }
}
