using dayforce_assignment.Server.Configuration;
using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;
using System.Text;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentDownloadService : IAttachmentDownloadService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AttachmentDownloadService(IHttpClientFactory httpClientFactory, AtlassianApiOptions options)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<KernelContent?> DownloadAttachmentAsync(string downloadLink, string mediaType)
        {
            if (string.IsNullOrWhiteSpace(downloadLink))
                return null; 

            try
            {
                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
                var responseBytes = await httpClient.GetByteArrayAsync(downloadLink);

                if (responseBytes == null || responseBytes.Length == 0)
                    return null; // ignore empty attachments

                if (mediaType.StartsWith("image/"))
                    return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);

                if (mediaType.StartsWith("text/"))
                    return new TextContent(Encoding.UTF8.GetString(responseBytes));

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
