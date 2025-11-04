using Microsoft.SemanticKernel;
namespace dayforce_assignment.Server.Interfaces
{
    public interface IJiraAttachmentService
    {
        Task<ImageContent> GetJiraAttachmentAsync(string contentId);
    }
}
