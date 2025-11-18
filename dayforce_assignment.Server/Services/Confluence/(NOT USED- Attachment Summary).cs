//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Interfaces.Common;
//using dayforce_assignment.Server.Interfaces.Confluence;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using System.Runtime.CompilerServices;
//using System.Text.Json;

//// This service is not currently being used.

//namespace dayforce_assignment.Server.Services.Confluence
//{
//    public class ConfluenceAttachmentSummaryService : IConfluencePageSummaryService
//    {
//        private readonly IChatCompletionService _chatCompletionService;
//        private readonly IConfluenceAttachmentsService _confluenceAttachmentsService;
//        private readonly IConfluenceAttachmentsCleaner _confluenceAttachmentsCleaner;
//        private readonly IAttachmentDownloadService _attachmentDownloadService;

//        public ConfluenceAttachmentSummaryService(IChatCompletionService chatCompletionService, IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner, IConfluenceAttachmentsService confluenceAttachmentsService, IAttachmentDownloadService attachmentDownloadService)
//        {
//            _chatCompletionService = chatCompletionService;
//            _confluenceAttachmentsCleaner = confluenceAttachmentsCleaner;
//            _confluenceAttachmentsService = confluenceAttachmentsService;
//            _attachmentDownloadService = attachmentDownloadService;
//        }
//        public async Task<string> SummarizeConfluenceAttachmentAsync(KernelContent attachment)
//        {
//            var history = new ChatHistory();
//            var userMessage = new ChatMessageContentItemCollection();
//            string systemPrompt = ("""
//                The user will upload an attachments.

//                # Your task:
//                * Analyze the attachment
//                * Convert each attachment into a clear, accurate, and complete text-based description.
//                * Extract and describe all relevant information, including visible content, structure, and meaning.
//                * Produce text that can fully replace the attachment for the purpose of generating test cases.

//                # Output requirements:
//                * For the attachment, output a structured text description.
//                * The description must contain all essential details, so that the original attachment is not required.
//                * Do not summarize—describe precisely, including:
//                  - All visible text
//                  - Key sections or headings
//                  - Tables, lists, diagrams, or UI elements
//                  - Relationships, workflows, or interactions shown in the attachment
//                * If the attachment is an image, describe the image content in detail.
//                * If the attachment is a document (PDF, Word, HTML, etc.), extract all meaningful textual content.

//                # Output format:
//                  Attachment N: Text Description
//                """);
            
//            history.AddSystemMessage(systemPrompt);

//            userMessage.Add(attachment);

//            //JsonElement rawConfluencePageAttachments = await _confluenceAttachmentsService.GetConfluenceAttachmentsAsync(baseUrl, confluencePage.Id);

//            //ConfluencePageAttachmentsDto cleanedConfluencePageAttachments = _confluenceAttachmentsCleaner.CleanConfluenceAttachments(rawConfluencePageAttachments);


//            //if (cleanedConfluencePageAttachments.Attachments?.Count > 0)
//            //{
//            //    foreach (DTOs.Confluence.Attachment attachment in cleanedConfluencePageAttachments.Attachments)
//            //    {
//            //        // Add Confluence Attachment to user message
//            //        var confluenceAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(attachment.DownloadLink, attachment.mediaType);
//            //        userMessage.Add(confluenceAttachment);
//            //    }
//            //}

//            history.AddUserMessage(userMessage);

//            var response = await _chatCompletionService.GetChatMessageContentAsync(history);
//            string stringResponse = response.ToString().Trim();

//            return stringResponse;
//        }
//    }
//}
