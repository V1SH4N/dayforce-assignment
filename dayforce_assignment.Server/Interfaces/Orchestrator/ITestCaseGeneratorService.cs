//using dayforce_assignment.Server.Interfaces.EventSinks;
using dayforce_assignment.Server.Interfaces.EventSinks;

namespace dayforce_assignment.Server.Interfaces.Orchestrator
{
    public interface ITestCaseGeneratorService
    {
        Task GenerateTestCasesAsync(string jiraKey, ISseEventSink events, CancellationToken cancellationToken);

    }
}
    