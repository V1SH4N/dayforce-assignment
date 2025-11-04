using dayforce_assignment.Server.Interfaces;
using Microsoft.SemanticKernel;

namespace dayforce_assignment.Server.Services
{
    public class JiraAttachmentService : IJiraAttachmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JiraAttachmentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ImageContent> GetJiraAttachmentAsync(string contentId)
        {
            var client = _httpClientFactory.CreateClient("dayforce");

            var responseBytes = await client.GetByteArrayAsync($"rest/api/3/attachment/content/{contentId}");

            return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), "image/jpeg");
        }
    }
}
