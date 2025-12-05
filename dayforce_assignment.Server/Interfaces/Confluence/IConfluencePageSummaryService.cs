using dayforce_assignment.Server.DTOs.Confluence;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageSummaryService
    {
        Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl, bool summarizeAttachment);
    }
}

