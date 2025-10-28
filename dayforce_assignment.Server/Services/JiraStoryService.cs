using dayforce_assignment.Server.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace dayforce_assignment.Server.Services
{
    public class JiraStoryService : IJiraStoryService
    {

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public JiraStoryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

        }
        public async Task<string> FetchJiraStoryAsync(string jiraId)
        {
            var httpClient = _httpClientFactory.CreateClient("JiraClient");

            string authenticationString = $"{_configuration["jiraEmail"]}:{_configuration["jiraToken"]}";
            byte[] authenticationBytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
            string base64EncodedAuthenticationString = Convert.ToBase64String(authenticationBytes);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            var httpResponseMessage = await httpClient.GetAsync($"rest/api/3/issue/{jiraId}");
            //httpResponseMessage.EnsureSuccessStatusCode();
            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            return json;
        }
    }
}
