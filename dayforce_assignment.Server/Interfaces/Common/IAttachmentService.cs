using Microsoft.SemanticKernel;
using dayforce_assignment.Server.DTOs.Common;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IAttachmentService
    {
        Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType, string fileName);

        Task<List<KernelContent>> DownloadAttachmentListAsync(IEnumerable<Attachment> attachments);

        Task<List<string>> SummarizeAttachmentListAsync(IEnumerable<Attachment> attachments);

        Task<string> SummarizeImageAttachmentAsync(ImageContent attachment);

    }
}
