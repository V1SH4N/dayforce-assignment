using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Orchestrator
{
    public interface ITestCaseGeneratorService
    {
        Task<JsonElement> GenerateTestCasesAsync(string jiraId);

    }
}
