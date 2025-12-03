//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class AttachmentListDownloadService : IAttachmentListDownloadService
//    {
//        private readonly IAttachmentDownloadService _attachmentDownloadService;
//        private readonly ILogger<AttachmentListDownloadService> _logger;

//        public AttachmentListDownloadService(
//            IAttachmentDownloadService attachmentDownloadService,
//            ILogger<AttachmentListDownloadService> logger)
//        {
//            _attachmentDownloadService = attachmentDownloadService;
//            _logger = logger;
//        }

//        public async Task<List<KernelContent>> DownloadAttachmentsAsync(IEnumerable<DTOs.Common.Attachment> attachments)
//        {
//            if (attachments == null || !attachments.Any())
//                return new List<KernelContent>();

//            var tasks = attachments.Select(att =>  _attachmentDownloadService.DownloadAttachmentAsync(att.DownloadLink, att.MediaType));

//            var results = await Task.WhenAll(tasks);

//            return results.Where(r => r != null).ToList()!;
//        }
//    }
//}
