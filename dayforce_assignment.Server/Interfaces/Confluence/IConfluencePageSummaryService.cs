using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Interfaces.EventSinks;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageSummaryService
    {
        Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl, bool summarizeAttachment, CancellationToken cancellationToken, ISseEventSink events);
    }
}

