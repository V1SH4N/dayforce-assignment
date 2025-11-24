using Microsoft.SemanticKernel;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IAttachmentDownloadService
    {
        Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType);

    }
}
