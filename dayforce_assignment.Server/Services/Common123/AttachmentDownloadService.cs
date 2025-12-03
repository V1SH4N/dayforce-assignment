//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.Exceptions;
//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;
//using System.Net;
//using System.Net.Http.Headers;
//using System.Text;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class AttachmentDownloadService : IAttachmentDownloadService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;

//        public AttachmentDownloadService(IHttpClientFactory httpClientFactory)
//        {
//            _httpClientFactory = httpClientFactory;
//        }

//        public async Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType)
//        {
//            try
//            {
//                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

//                byte[] responseBytes;

//                try
//                {
//                    var response = await httpClient.GetAsync(downloadLink);

//                    // Redirect status codes
//                    if ((int)response.StatusCode == 302)
//                    {
//                        var redirectDownloadLink = response.Headers.Location;
//                        responseBytes = await httpClient.GetByteArrayAsync(redirectDownloadLink);
//                    }
//                    else
//                    {
//                        responseBytes = await response.Content.ReadAsByteArrayAsync();

//                    }
//                }
//                catch (Exception)
//                {
//                    return null; 
//                }

//                if (mediaType.StartsWith("image/"))
//                {
//                    return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);
//                }

//                if (mediaType.StartsWith("text/"))
//                {
//                    return new TextContent(Encoding.UTF8.GetString(responseBytes));
//                }

//                //throw new UnsupportedAttachmentMediaTypeException(mediaType); must log this
//            }
//            catch (Exception ex) when (!(ex is DomainException))
//            {
//                throw new AttachmentDownloadException(downloadLink, $"An Unexpected error has occured");
//            }
//        }
//    }
//}
