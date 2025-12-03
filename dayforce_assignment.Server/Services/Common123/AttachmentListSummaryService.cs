//using dayforce_assignment.Server.DTOs.Common;
//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class AttachmentListSummaryService : IAttachmentListSummaryService
//    {
//        private readonly IImageAttachmentSummaryService _attachmentSummaryService;
//        private readonly ILogger<AttachmentListDownloadService> _logger;

//        public AttachmentListSummaryService(
//            IImageAttachmentSummaryService attachmentSummaryService,
//            ILogger<AttachmentListDownloadService> logger)
//        {
//            _attachmentSummaryService = attachmentSummaryService;
//            _logger = logger;
//        }

//        public async Task<List<string>> SummarizeAttachmentsAsync(IEnumerable<KernelContent> attachments)
//        {
//            var result = new List<string>();

//            if (attachments == null || !attachments.Any())
//                return result;

//            var tasks = attachments.Select(async att =>
//            {
//                switch (att)
//                {
//                    case TextContent text:
//                        // Directly return the text data
//                        return text.Text;

//                    case ImageContent image:
//                        // Summarize the image using the LLM
//                        return await _attachmentSummaryService.SummarizeAttachmentAsync(image);

//                    default:
//                        _logger.LogWarning("Unknown attachment type encountered: {Type}", att.GetType().Name);
//                        return null;
//                }
//            });

//            var results = await Task.WhenAll(tasks);

//            return results
//                .Where(r => !string.IsNullOrWhiteSpace(r))
//                .ToList()!;
//        }
//    }
//}
