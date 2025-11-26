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
        private readonly IConfluencePageSearchParameterService _confluencePageSearchParameterService;
        private readonly IConfluencePageSearchService _confluencePageSearchService;
        private readonly IConfluencePageSearchFilterService _confluencePageSearchFilterService;
        private readonly string _baseUrl;

        public ConfluencePageSearchOrchestrator(
            IConfluencePageSearchParameterService confluencePageSearchParameterService,
            IConfluencePageSearchService confluencePageSearchService,
            IConfluencePageSearchFilterService confluencePageSearchFilterService,
            IConfiguration configuration)
        {
            _confluencePageSearchParameterService = confluencePageSearchParameterService;
            _confluencePageSearchService = confluencePageSearchService;
            _confluencePageSearchFilterService = confluencePageSearchFilterService;
            _baseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured."); ;
        }

        public async Task<ConfluencePageReferencesDto> SearchConfluencePageReferencesAsync(JiraIssueDto jiraIssue)
        {
            try
            {
                // Get search parameters
                ConfluenceSearchParametersDto searchParametersList = await _confluencePageSearchParameterService.GetParametersAsync(jiraIssue);

                var pagesMetadata = new ConcurrentBag<ConfluencePageMetadata>();

                // Execute search tasks in parallel
                var searchTasks = searchParametersList.SearchParameters.Select(async searchParameter =>
                {
                    JsonElement searchResult = await _confluencePageSearchService.SearchPageAsync(searchParameter);

                    if (searchResult.TryGetProperty("results", out var results))
                    {
                        foreach (var item in results.EnumerateArray())
                        {
                            var pageMetadata = new ConfluencePageMetadata
                            {
                                Id = item.GetProperty("id").GetString() ?? string.Empty,
                                Title = item.GetProperty("title").GetString() ?? string.Empty
                            };

                            if (!string.IsNullOrEmpty(pageMetadata.Id) && !pagesMetadata.Any(p => p.Id == pageMetadata.Id))
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
                var filteredSearchPagesMetadata = await _confluencePageSearchFilterService.FilterResultAsync(jiraIssue, searchPagesMetadata);

                // Convert filtered search results to ConfluencePageReferencesDto
                var confluencePageReferences = new ConfluencePageReferencesDto();

                foreach (var result in filteredSearchPagesMetadata.ConfluencePagesMetadata)
                {
                    confluencePageReferences.ConfluencePages.Add(new ConfluencePage
                    {
                        baseUrl = _baseUrl,
                        pageId = result.Id
                    });
                }

                return confluencePageReferences;
            }
            catch (DomainException)
            {
                throw; // propagate known domain exceptions
            }
            catch (Exception)
            {
                throw new ConfluencePageSearchOrchestratorException(jiraIssue.Key, "An unexpected error has occured");
            }
        }
    }
}
