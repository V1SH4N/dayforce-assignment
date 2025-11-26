using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentListDownloadService: IAttachmentListDownloadService
    {
        private readonly IAttachmentDownloadService _attachmentDownloadService;
        public AttachmentListDownloadService(IAttachmentDownloadService attachmentDownloadService, ILogger<AttachmentListDownloadService> logger)
        {
            _attachmentDownloadService = attachmentDownloadService;
        }
        public async Task<List<KernelContent>> DownloadAttachmentsAsync(IEnumerable<DTOs.Common.Attachment> attachments)
        {
            var tasks = attachments.Select(async att =>
            {
                try
                {
                    return await _attachmentDownloadService.DownloadAttachmentAsync(att.DownloadLink, att.MediaType);
                }
                catch (Exception)
                {
                    return null; // Ignore failed attachment downloads
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList()!;
        }
    }
}
