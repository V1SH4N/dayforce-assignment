//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.Interfaces.Common;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;

//namespace dayforce_assignment.Server.Services.Common
//{
//    public class ImageAttachmentSummaryService: IImageAttachmentSummaryService
//    {
//        private readonly IChatCompletionService _chatCompletionService;

//        public ImageAttachmentSummaryService(IChatCompletionService chatCompletionService)
//        {
//            _chatCompletionService = chatCompletionService;
//        }

//        public async Task<string> SummarizeAttachmentAsync(ImageContent attachment)
//        {
//            var history = new ChatHistory();
//            var userPrompt = new ChatMessageContentItemCollection();

//            string systemPrompt = File.ReadAllText("SystemPrompts/ImageAttachmentSummary.txt");

//            history.AddSystemMessage(systemPrompt);

//            userPrompt.Add(attachment);

//            history.AddUserMessage(userPrompt);

//            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

//            return response.ToString().Trim();
//        }
        
//    }
//}
