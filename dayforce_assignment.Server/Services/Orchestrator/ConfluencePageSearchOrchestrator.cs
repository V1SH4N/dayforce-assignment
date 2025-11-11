using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class ConfluencePageSearchOrchestrator : IConfluencePageSearchOrchestrator
    {
        private readonly IConfluencePageSearchParameterService _confluencePageSearchParameterService;
        private readonly IConfluencePageSearchService _confluencePageSearchService;

        public ConfluencePageSearchOrchestrator(
            IConfluencePageSearchParameterService confluencePageSearchParameterService,
            IConfluencePageSearchService confluencePageSearchService
            )
        {
            _confluencePageSearchParameterService = confluencePageSearchParameterService;
            _confluencePageSearchService = confluencePageSearchService;
        }

        public async Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraStoryDto jiraStory)
        {
            var searchPagesMetadata = new List<ConfluencePageMetadata>();

            ConfluenceSearchParametersDto searchParametersList = await _confluencePageSearchParameterService.GetSearchParametersAsync(jiraStory);

            foreach (string searchParameter in searchParametersList.SearchParameters)
            {
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

                        if (!searchPagesMetadata.Any(p => p.Id == pageMetadata.Id))
                        {
                            searchPagesMetadata.Add(pageMetadata);
                        }
                    }

                }
            }

            //searchPagesMetada contains all the accumulated search results.



            return new ConfluencePageReferencesDto();
        }

    }

}
