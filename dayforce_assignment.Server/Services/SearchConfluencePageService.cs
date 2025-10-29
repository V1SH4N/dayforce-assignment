﻿using dayforce_assignment.Server.Interfaces;

namespace dayforce_assignment.Server.Services
{
    public class SearchConfluencePageService : ISearchConfluencePageService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchConfluencePageService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> SearchPageAsync(string cql)
        {
            var httpClient = _httpClientFactory.CreateClient("dayforce");

            //search?cql=(title~"delete*" or title~"*delete") and (title~"*container" or title="container*") and space="AR"
            var httpResponseMessage = await httpClient.GetAsync($"wiki/rest/api/content/search?{cql}");
            //httpResponseMessage.EnsureSuccessStatusCode();
            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            return json;
        }
    }
}
