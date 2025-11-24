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
            try 
            {
                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
                var responseBytes = await httpClient.GetByteArrayAsync(downloadLink);

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
