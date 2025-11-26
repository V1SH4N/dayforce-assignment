using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageSummaryService
    {
        Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl);
    }
}

