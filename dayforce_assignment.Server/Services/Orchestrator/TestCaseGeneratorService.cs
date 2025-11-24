using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Diagnostics;


namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class TestCaseGeneratorService: ITestCaseGeneratorService
    {
        private readonly IUserMessageBuilder _userMessageBuilder;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IJsonFormatterService _jsonFormatterService;
        public TestCaseGeneratorService(IUserMessageBuilder userMessageBuilder, IChatCompletionService chatCompletionService, IJsonFormatterService jsonFormatterService)
        {
            _chatCompletionService = chatCompletionService;
            _userMessageBuilder = userMessageBuilder;
            _jsonFormatterService = jsonFormatterService;
        }

        public async Task<JsonElement> GenerateTestCasesAsync(string JiraId)
        {
            // stopwatch 
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var history = new ChatHistory();

            string systemPrompt = File.ReadAllText("SystemPrompts/TestCaseGeneratorV3.txt");

            history.AddSystemMessage(systemPrompt);
           
            ChatMessageContentItemCollection userMessage = await _userMessageBuilder.BuildUserMessageAsync(JiraId);

            history.AddUserMessage(userMessage);

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            var jsonResponse = _jsonFormatterService.FormatJson(response.ToString());

            // Time measurment
            stopwatch.Stop(); 
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine($"[Timer] Elapsed time: {elapsed.TotalMilliseconds} ms");

            return jsonResponse;

        }
    }
}
