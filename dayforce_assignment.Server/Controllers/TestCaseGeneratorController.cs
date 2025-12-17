using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;

namespace dayforce_assignment.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCaseGeneratorController : ControllerBase
    {
        private readonly ITestCaseGeneratorService _testCaseGeneratorService;

        public TestCaseGeneratorController(ITestCaseGeneratorService testCaseGeneratorService)
        {
            _testCaseGeneratorService = testCaseGeneratorService;
        }

        [HttpGet("testCases")]
        public async Task<ActionResult<JsonElement>> GenerateTestCases([FromQuery] string jiraId)
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = await _testCaseGeneratorService.GenerateTestCasesAsync(jiraId);
            sw.Stop();
            Console.WriteLine($"\n\n\n\n\n\n\ntime: {sw.ElapsedMilliseconds}");
            return Ok(result);
        }






        [HttpGet("sseTest")]
        public async Task sseTest([FromQuery] string jiraId, CancellationToken cancellationToken)
        {
            // Set necessary headers for Server-Sent Events
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            //Response.Headers["Connection"] = "keep-alive";
            // Loop to continuously send events
            while (!cancellationToken.IsCancellationRequested)
            {
                var eventData = $"data: The current time is {DateTime.Now}\\n\\n";

                // Write the event data to the response body
                await Response.WriteAsync(eventData, cancellationToken);

                // Flush the buffer to send the data to the client immediately
                await Response.Body.FlushAsync(cancellationToken);

            }
        }
    }






}
