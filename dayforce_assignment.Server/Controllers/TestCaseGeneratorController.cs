using dayforce_assignment.Server.EventSinks;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.AspNetCore.Mvc;

namespace dayforce_assignment.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCaseGeneratorController : ControllerBase
    {
        private readonly ITestCaseGeneratorService _testCaseGeneratorService;
        private readonly ILogger<TestCaseGeneratorController> _logger;

        public TestCaseGeneratorController(ITestCaseGeneratorService testCaseGeneratorService, ILogger<TestCaseGeneratorController> logger)

        {
            _testCaseGeneratorService = testCaseGeneratorService;
            _logger = logger;
        }


        [HttpGet("generate/stream")]
        public async Task Stream([FromQuery] string jiraKey, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-cache";
            Response.ContentType = "text/event-stream";

            var sink = new SseEventSink(Response);
            HttpContext.Items["SseEventSink"] = sink;

            try
            {
                await _testCaseGeneratorService.GenerateTestCasesAsync(jiraKey, sink, cancellationToken);
            }
            catch (OperationCanceledException) { }
            
            catch (DomainException domainEx)
            {
                await sink.ErrorEventAsync(domainEx.GetType().Name, domainEx.Message, (int)domainEx.StatusCode, cancellationToken);
            }
            catch (HttpRequestException httpEx)
            {
                string title = "External service error";
                string detail = "Failed to fetch data from Jira endpoint";
                await sink.ErrorEventAsync(title, detail, StatusCodes.Status503ServiceUnavailable, cancellationToken);
                _logger.LogError(httpEx, "Failed to fetch data from Jira for Jira key: {JiraKey}", jiraKey);
            }
            catch (Exception ex)
            {
                string title = "Internal server error";
                string detail = "An unexpected error occurred.";
                await sink.ErrorEventAsync(title, detail, StatusCodes.Status500InternalServerError, cancellationToken);
                _logger.LogError(ex, "Unexpected error occurred while generating test cases for Jira key: {JiraKey}", jiraKey);
            }
            finally
            {
                await sink.RequestCompleteAsync(cancellationToken);
            }
           
        }
    }
}
