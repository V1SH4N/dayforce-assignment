using dayforce_assignment.Server.DTOs.Common;
using Microsoft.SemanticKernel;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IAttachmentListDownloadService
    {
        Task<List<KernelContent>> DownloadAttachmentsAsync(IEnumerable<Attachment> attachments);
    }
}
