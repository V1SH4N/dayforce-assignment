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
            _baseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured."); ;
        }

        public async Task<ConfluencePageReferencesDto> SearchConfluencePageReferencesAsync(JiraIssueDto jiraIssue)
        {
            // Get search query parameters
            ConfluenceSearchParametersDto searchParametersList = await _confluenceSearchService.GetParametersAsync(jiraIssue);

            if (!searchParametersList.SearchParameters.Any())
                return new ConfluencePageReferencesDto();


            // Execute search tasks in parallel
            var pagesMetadata = new ConcurrentDictionary<string, ConfluencePageMetadata>();

            var searchTasks = searchParametersList.SearchParameters.Select(async searchParameter =>
            {
                JsonElement searchResult = await _confluenceSearchService.SearchPageAsync(searchParameter);

                if (searchResult.TryGetProperty("results", out JsonElement results))
                {
                    foreach (JsonElement item in results.EnumerateArray())
                    {
                        string id = item.GetProperty("id").GetString() ?? string.Empty;
                        string title = item.GetProperty("title").GetString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(id))
                        {
                            pagesMetadata.TryAdd(id, new ConfluencePageMetadata { Id = id, Title = title });
                        }
                    }
                }
            });

            await Task.WhenAll(searchTasks);

            var searchResults = new ConfluenceSearchResultsDto
            {
                ConfluencePagesMetadata = pagesMetadata.Values.ToList()
            };

            // Filter search results
            ConfluenceSearchResultsDto filteredSearchResults = await _confluenceSearchService.FilterResultAsync(jiraIssue, searchResults);

            // Convert filtered search results to ConfluencePageReferencesDto
            var confluencePageReferences = new ConfluencePageReferencesDto();

            foreach (ConfluencePageMetadata result in filteredSearchResults.ConfluencePagesMetadata)
            {
                confluencePageReferences.ConfluencePages.Add(new ConfluencePage
                {
                    baseUrl = _baseUrl,
                    pageId = result.Id
                });
            }

            return confluencePageReferences;
        }
    }
}
