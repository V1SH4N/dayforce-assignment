using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.RegularExpressions;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageReferenceExtractor : IConfluencePageReferenceExtractor
    {
        private readonly ILogger<ConfluencePageReferenceExtractor> _logger;

        public ConfluencePageReferenceExtractor(ILogger<ConfluencePageReferenceExtractor> logger)
        {
            _logger = logger;
        }

        // Extracts confluence page link references from Jira issue information. Returns new ConfluencePageReferencesDto initialized with empty list if not found.
        public ConfluencePageReferencesDto GetReferences(JiraIssueDto jiraIssue)
        {
            var dto = new ConfluencePageReferencesDto();

            var descriptionRegex = new Regex(@"https:\/\/[^\/]+\/wiki\/spaces\/[^\/]+\/pages\/(?<id>\d+)", RegexOptions.IgnoreCase);
            var remoteLinksRegex = new Regex(@"https:\/\/[^\/]+\/wiki\/pages\/viewpage\.action\?pageId=(?<id>\d+)", RegexOptions.IgnoreCase);
            
            if (!string.IsNullOrWhiteSpace(jiraIssue.Description))
            {
                foreach (Match match in descriptionRegex.Matches(jiraIssue.Description))
                {
                    try
                    {
                        var uri = new Uri(match.Value);
                        string pageId = match.Groups["id"].Value;
                        dto.ConfluencePages.TryAdd(pageId, new ConfluencePage
                        {
                            baseUrl = $"{uri.Scheme}://{uri.Host}/",
                            pageId = match.Groups["id"].Value
                        });
                    }
                    catch (UriFormatException ex)
                    {
                        _logger.LogWarning(ex.Message, "Invalid confluence url in jira description");
                    }
                }
            }

            foreach (var link in jiraIssue.RemoteLinks)
            {
                foreach (Match match in remoteLinksRegex.Matches(link))
                {
                    try
                    {
                        var uri = new Uri(match.Value);
                        var pageId = match.Groups["id"].Value;
                        dto.ConfluencePages.TryAdd(pageId, new ConfluencePage
                        {
                            baseUrl = $"{uri.Scheme}://{uri.Host}/",
                            pageId = pageId
                        });
                    }
                    catch (UriFormatException ex)
                    {
                        _logger.LogWarning(ex.Message, "Invalid confluence url in jira remote links");
                    }
                }
            }
            
            return dto;
        }
    }
}
