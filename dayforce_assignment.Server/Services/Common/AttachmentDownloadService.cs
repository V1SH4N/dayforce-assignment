//using dayforce_assignment.Server.Configuration;
//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;
//using System.Text;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class AttachmentDownloadService : IAttachmentDownloadService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;

//        public AttachmentDownloadService(IHttpClientFactory httpClientFactory, AtlassianApiOptions options)
//        {
//            _httpClientFactory = httpClientFactory;
//        }

//        public async Task<KernelContent?> DownloadAttachmentAsync(string downloadLink, string mediaType)
//        {
//            try
//            {
//                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
//                var responseBytes = await httpClient.GetByteArrayAsync(downloadLink);

//                if (mediaType.StartsWith("image/"))
//                    return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);

//                if (mediaType.StartsWith("text/"))
//                    return new TextContent(Encoding.UTF8.GetString(responseBytes));

//                return null;
//            }
//            catch
//            {
//                return null;
//            }


//        }
//    }
//}



//hid


//using dayforce_assignment.Server.Configuration;
//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;
//using System.Text;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class AttachmentDownloadService : IAttachmentDownloadService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<AttachmentDownloadService> _logger;

//        public AttachmentDownloadService(IHttpClientFactory httpClientFactory, AtlassianApiOptions options, ILogger<AttachmentDownloadService> logger)
//        {
//            _httpClientFactory = httpClientFactory;
//            _logger = logger;
//        }

//        public async Task<KernelContent?> DownloadAttachmentAsync(string downloadLink, string mediaType)
//        {
//            if (string.IsNullOrWhiteSpace(downloadLink))
//            {
//                _logger.LogWarning("Download link is null or empty, skipping attachment.");
//                return null;
//            }

//            try
//            {
//                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
//                var responseBytes = await httpClient.GetByteArrayAsync(downloadLink);

//                if (responseBytes == null || responseBytes.Length == 0)
//                {
//                    _logger.LogWarning("Downloaded content is empty for link {Link}, skipping.", downloadLink);
//                    return null;
//                }

//                if (mediaType.StartsWith("image/"))
//                    return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);

//                if (mediaType.StartsWith("text/"))
//                    return new TextContent(Encoding.UTF8.GetString(responseBytes));

//                _logger.LogInformation("Unsupported media type {MediaType} for attachment {Link}, skipping.", mediaType, downloadLink);
//                return null;
//            }
//            catch (HttpRequestException ex)
//            {
//                _logger.LogWarning(ex, "Network error while downloading attachment {Link}, skipping.", downloadLink);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning(ex, "Unexpected error while downloading attachment {Link}, skipping.", downloadLink);
//                return null;
//            }
//        }
//    }
//}



using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;
using System.Text;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentDownloadService : IAttachmentDownloadService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AttachmentDownloadService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

                byte[] responseBytes;

                try
                {
                    responseBytes = await httpClient.GetByteArrayAsync(downloadLink);
                }
                catch (Exception)
                {
                    throw new AttachmentDownloadException(downloadLink, $"HTTP download failed");
                }

                if (mediaType.StartsWith("image/"))
                {
                    return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);
                }

                if (mediaType.StartsWith("text/"))
                {
                    return new TextContent(Encoding.UTF8.GetString(responseBytes));
                }

                throw new UnsupportedAttachmentMediaTypeException(mediaType);
            }
            catch (Exception ex) when (!(ex is DomainException))
            {
                throw new AttachmentDownloadException(downloadLink, $"An Unexpected error has occured");
            }
        }
    }
}
