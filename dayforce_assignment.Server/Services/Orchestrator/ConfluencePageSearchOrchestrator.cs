using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using System.Collections.Concurrent;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class ConfluencePageSearchOrchestrator : IConfluencePageSearchOrchestrator
    {
        private readonly IConfluencePageSearchParameterService _confluencePageSearchParameterService;
        private readonly IConfluencePageSearchService _confluencePageSearchService;
        private readonly IConfluencePageSearchFilterService _confluencePageSearchFilterService;
        private readonly string _baseUrl;


        public ConfluencePageSearchOrchestrator(
            IConfluencePageSearchParameterService confluencePageSearchParameterService,
            IConfluencePageSearchService confluencePageSearchService,
            IConfluencePageSearchFilterService confluencePageSearchFilterService,
             IConfiguration configuration
            )
        {
            _confluencePageSearchParameterService = confluencePageSearchParameterService;
            _confluencePageSearchService = confluencePageSearchService;
            _confluencePageSearchFilterService = confluencePageSearchFilterService;
            _baseUrl = configuration["Atlassian:BaseUrl"];
        }

        public async Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraIssueDto jiraStory)
        {
            //var pagesMetadata = new List<ConfluencePageMetadata>();

            // Get search parameters
            ConfluenceSearchParametersDto searchParametersList = await _confluencePageSearchParameterService.GetSearchParametersAsync(jiraStory);

            var pagesMetadata = new ConcurrentBag<ConfluencePageMetadata>(); // thread safe collection

            // Create list of tasks for all search parameters
            var searchTasks = searchParametersList.SearchParameters.Select(async searchParameter =>
            {
                // Get search results
                JsonElement searchResult = await _confluencePageSearchService.SearchConfluencePageAsync(searchParameter);

                if (searchResult.TryGetProperty("results", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var pageMetadata = new ConfluencePageMetadata
                        {
                            Id = item.GetProperty("id").GetString() ?? string.Empty,
                            Title = item.GetProperty("title").GetString() ?? string.Empty
                        };

                        // Add only if it doesn't exist already
                        if (!pagesMetadata.Any(p => p.Id == pageMetadata.Id))
                        {
                            pagesMetadata.Add(pageMetadata);
                        }
                    }
                }
            });

            await Task.WhenAll(searchTasks);

            var searchPagesMetadata = new ConfluenceSearchResultsDto
            {
                ConfluencePagesMetadata = pagesMetadata.ToList()
            };

            // Filter search results
            ConfluenceSearchResultsDto filteredSearchPagesMetada = await _confluencePageSearchFilterService.FilterSearchResultAsync(jiraStory, searchPagesMetadata);

            // Convert filtered search results to ConfluencePagereferencesDto
            var confluencePageReferences = new ConfluencePageReferencesDto();

            foreach (var result in filteredSearchPagesMetada.ConfluencePagesMetadata)
            {
                var pageReference = new ConfluencePage
                {
                    baseUrl = _baseUrl, 
                    pageId = result.Id,
                    
                };
                confluencePageReferences.ConfluencePages.Add(pageReference);
            }

            return confluencePageReferences;
        }

    }

}

