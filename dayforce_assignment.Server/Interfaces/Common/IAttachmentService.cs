using Microsoft.SemanticKernel;
using dayforce_assignment.Server.DTOs.Common;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IAttachmentService
    {
        Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType, string fileName, CancellationToken cacellationToken);

        Task<List<KernelContent>> DownloadAttachmentListAsync(IEnumerable<Attachment> attachments, CancellationToken cancellationToken);

        Task<List<string>> SummarizeAttachmentListAsync(IEnumerable<Attachment> attachments, CancellationToken cancellationToken);

        Task<string> SummarizeImageAttachmentAsync(string filename, ImageContent attachment, CancellationToken cancellationToken);

    }
}
