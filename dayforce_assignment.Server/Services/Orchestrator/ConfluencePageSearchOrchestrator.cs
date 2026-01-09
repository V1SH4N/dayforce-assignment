using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using System.Collections.Concurrent;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class ConfluencePageSearchOrchestrator : IConfluencePageSearchOrchestrator
    {
        private readonly IConfluenceSearchService _confluenceSearchService;
        private readonly string _baseUrl;

        public ConfluencePageSearchOrchestrator(
            IConfluenceSearchService confluenceSearchService,
            IConfiguration configuration)
        {
            _confluenceSearchService = confluenceSearchService;
            _baseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured.");
        }


        public async Task<ConfluencePageReferencesDto> SearchConfluencePageReferencesAsync(JiraIssueDto jiraIssue, ConfluencePageReferencesDto confluencePagereferences, CancellationToken cancellationToken)
        {

            // Get search query parameters
            ConfluenceSearchParametersDto searchParametersList = await _confluenceSearchService.GetParametersAsync(jiraIssue, cancellationToken);

            if (!searchParametersList.SearchParameters.Any())
                return confluencePagereferences;



            // Execute search tasks in parallel
            var searchResults = new ConcurrentDictionary<string, ConfluencePage>();

            var searchTasks = searchParametersList.SearchParameters.Select(async searchParameter =>
            {
                JsonElement searchResponse = await _confluenceSearchService.SearchPageAsync(searchParameter, cancellationToken);

                if (searchResponse.TryGetProperty("results", out JsonElement results))
                {
                    foreach (JsonElement item in results.EnumerateArray())
                    {
                        string id = item.GetProperty("id").GetString() ?? string.Empty;
                        string title = item.GetProperty("title").GetString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(id))
                        {
                            searchResults.TryAdd(id, new ConfluencePage { 
                                pageId = id,
                                title = title,
                                baseUrl = _baseUrl
                            });
                        }
                    }
                }
            });

            await Task.WhenAll(searchTasks);



            // Filter search results
            ConfluencePageReferencesDto filteredSearchResults = await _confluenceSearchService.FilterResultAsync(jiraIssue, searchResults, cancellationToken);

            foreach (ConfluencePage result in filteredSearchResults.ConfluencePages.Values)
            {
                confluencePagereferences.ConfluencePages.TryAdd(result.pageId, new ConfluencePage
                {
                    baseUrl = _baseUrl,
                    pageId = result.pageId
                });   
            }




            return confluencePagereferences;

        }
    }
}
