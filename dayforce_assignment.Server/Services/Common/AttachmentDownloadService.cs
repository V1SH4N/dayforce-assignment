using dayforce_assignment.Server.Configuration;
using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;
using System.Net.Http;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentDownloadService: IAttachmentDownloadService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AttachmentDownloadService(IHttpClientFactory httpClientFactory, AtlassianApiOptions options)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ImageContent> DownloadAttachmentAsync(string downloadLink)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
            var responseBytes = await httpClient.GetByteArrayAsync(downloadLink);

            return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), "image/jpeg");
        }
    }
}
