

//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Interfaces.Confluence;
//using dayforce_assignment.Server.Interfaces.Jira;
//using dayforce_assignment.Server.Interfaces.Orchestrator;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Options;
//using System.Text.Json;

//namespace dayforce_assignment.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class TestingController : ControllerBase
//    {
//        private readonly IJiraIssueService _jiraStoryService;
//        private readonly IJiraIssueMapper _jiraStoryCleaner;
//        private readonly IConfluencePageService _confluencePageService;
//        private readonly IConfluencePageMapper _confluencePageCleaner;
//        private readonly IConfluenceAttachmentsService _attachmentsService;
//        private readonly IConfluenceAttachmentsMapper _attachmentsCleaner;
//        private readonly IConfluencePageReferenceExtractor12 _pageReferenceExtractor;
//        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
//        private readonly IConfluenceCommentsService _confluenceCommentsService;
//        private readonly IJiraRemoteLinksService _jiraRemoteLinksService;
//        private readonly IConfluencePageSummaryService123 _confluencePageSummaryService;


//        public TestingController(
//            IJiraIssueService jiraStoryService,
//            IJiraIssueMapper jiraStoryCleaner,
//            IConfluencePageService confluencePageService,
//            IConfluencePageMapper confluencePageCleaner,
//            IConfluenceAttachmentsService attachmentsService,
//            IConfluenceAttachmentsMapper attachmentsCleaner,
//            IConfluencePageReferenceExtractor12 pageReferenceExtractor,
//            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator,
//            IConfluenceCommentsService confluenceCommentsService,
//            IJiraRemoteLinksService jiraRemoteLinksService,
//            IConfluencePageSummaryService123 confluencePageSummaryService)
//        {
//            _jiraStoryService = jiraStoryService;
//            _jiraStoryCleaner = jiraStoryCleaner;
//            _confluencePageService = confluencePageService;
//            _confluencePageCleaner = confluencePageCleaner;
//            _attachmentsService = attachmentsService;
//            _attachmentsCleaner = attachmentsCleaner;
//            _confluencePageSearchOrchestrator = confluencePageSearchOrchestrator;
//            _pageReferenceExtractor = pageReferenceExtractor;
//            _confluenceCommentsService = confluenceCommentsService;
//            _jiraRemoteLinksService = jiraRemoteLinksService;
//            _jiraRemoteLinksService = jiraRemoteLinksService;
//            _confluencePageSummaryService = confluencePageSummaryService;
//        }



//        [HttpGet("jira/story/{jiraId}")]
//        public async Task<IActionResult> GetJiraStory(string jiraId)
//        {
//            var jiraJson = await _jiraStoryService.GetIssueAsync(jiraId);
//            return Ok(jiraJson);
//        }

//        [HttpGet("jira/story/{jiraId}/clean")]
//        public async Task<IActionResult> GetCleanJiraStory(string jiraId)
//        {
//            var jiraJson = await _jiraStoryService.GetIssueAsync(jiraId);
//            JsonElement rawJiraRemoteLinks = await _jiraRemoteLinksService.GetRemoteLinksAsync(jiraId);
//            var cleanedDto = _jiraStoryCleaner.MapToDto(jiraJson, rawJiraRemoteLinks);
//            var remote = rawJiraRemoteLinks;
//            Console.WriteLine($"\n\n\n\n\n{remote}");
//            return Ok(cleanedDto);
//        }



//        [HttpGet("confluence/page/{pageId}")]
//        public async Task<IActionResult> GetConfluencePage([FromQuery] string baseUrl, string pageId)
//        {
//            var rawPage = await _confluencePageService.GetPageAsync(baseUrl, pageId);
//            return Ok(rawPage);
//        }
//        [HttpGet("confluence/page/{pageId}/clean")]
//        public async Task<IActionResult> GetCleanConfluencePage([FromQuery] string baseUrl, string pageId)
//        {
//            var pageJson = await _confluencePageService.GetPageAsync(baseUrl, pageId);
//            var commentjson = await _confluenceCommentsService.GetCommentsAsync(baseUrl, pageId);
//            var cleanedDto = _confluencePageCleaner.MapToDto(pageJson, commentjson);
//            return Ok(cleanedDto);
//        }

//        [HttpGet("confluence/page/{pageId}/attachments")]
//        public async Task<IActionResult> GetConfluenceAttachments([FromQuery] string baseUrl, string pageId)
//        {
//            var attachmentsJson = await _attachmentsService.GetAttachmentsAsync(baseUrl, pageId);
//            return Ok(attachmentsJson);
//        }

//        [HttpGet("confluence/page/{pageId}/attachments/clean")]
//        public async Task<IActionResult> GetCleanConfluenceAttachments([FromQuery] string baseUrl, string pageId)
//        {
//            var attachmentsJson = await _attachmentsService.GetAttachmentsAsync(baseUrl, pageId);
//            var cleanedDto = _attachmentsCleaner.MapToDto(attachmentsJson);
//            return Ok(cleanedDto);
//        }



//        [HttpPost("jira/story/confluence-references")]
//        public async Task<IActionResult> GetConfluencePageReferences([FromBody] JiraIssueDto jiraStory)
//        {
//            var referencesDto = await _pageReferenceExtractor.GetReferencesAsync(jiraStory);
//            return Ok(referencesDto);
//        }

//        [HttpPost("jira/story/filteredsearch")]
//        public async Task<IActionResult> GetFilteredsearch([FromBody] JiraIssueDto jiraStory)
//        {
//            var filteredSearch = await _confluencePageSearchOrchestrator.SearchConfluencePageReferencesAsync(jiraStory);
//            return Ok(filteredSearch);
//        }
//        //[HttpPost("confluencePageSummary")]
//        //public async Task<IActionResult> Summarize(ConfluencePageDto confluencePage)
//        //{
            
//        //    var summary = await _confluencePageSummaryService.SummarizeConfluencePageAsync(confluencePage, "https://dayforce.atlassian.net/");
//        //    return Ok(summary);
//        //}
//    }
//}



