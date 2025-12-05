using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
            var result = await _testCaseGeneratorService.GenerateTestCasesAsync(jiraId);
            return Ok(result);
        }

    }
}
