//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Exceptions;
//using dayforce_assignment.Server.Interfaces.Common;
//using dayforce_assignment.Server.Interfaces.Confluence;
//using dayforce_assignment.Server.Services.Common;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using System.Text.Json;

//namespace dayforce_assignment.Server.Services.Confluence
//{
//    public class ConfluencePageSummaryService123 : IConfluencePageSummaryService123
//    {
//        private readonly IChatCompletionService _chatCompletionService;
//        private readonly IConfluenceAttachmentsService _confluenceAttachmentsService;
//        private readonly IConfluenceAttachmentsMapper _confluenceAttachmentsMapper;
//        private readonly IAttachmentListDownloadService _attachmentListDownloadService;
//        private readonly IAttachmentListSummaryService _attachmentListSummaryService;
//        private readonly ILogger<GlobalExceptionMiddleware> _logger;


//        public ConfluencePageSummaryService123(
//            IChatCompletionService chatCompletionService,
//            IConfluenceAttachmentsMapper confluenceAttachmentsMapper,
//            IConfluenceAttachmentsService confluenceAttachmentsService,
//            IAttachmentListDownloadService attachmentListDownloadService,
//            IAttachmentListSummaryService attachmentListSummaryService,
//            ILogger<GlobalExceptionMiddleware> logger
//            )
//        {
//            _chatCompletionService = chatCompletionService;
//            _confluenceAttachmentsMapper = confluenceAttachmentsMapper;
//            _confluenceAttachmentsService = confluenceAttachmentsService;
//            _attachmentListDownloadService = attachmentListDownloadService;
//            _attachmentListSummaryService = attachmentListSummaryService;
//            _logger = logger;
//        }

//        public async Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl, bool summarizeAttachment)
//        {
//            var pageId = confluencePage?.Id ?? "unknown";

//            try
//            {
//                ChatMessageContent response;
//                var history = new ChatHistory();
//                var userPrompt = new ChatMessageContentItemCollection();

//                string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSummary.txt");

//                // Get Confluence attachments 
//                JsonElement jsonAttachments = await _confluenceAttachmentsService.GetAttachmentsAsync(baseUrl, pageId);
//                ConfluencePageAttachmentsDto confluencePageAttachments = _confluenceAttachmentsMapper.MapToDto(jsonAttachments);

//                try
//                {
//                    history.AddSystemMessage(systemPrompt);

//                    userPrompt.Add(new TextContent(JsonSerializer.Serialize(confluencePage, new JsonSerializerOptions { WriteIndented = true })));

//                    userPrompt.Add(new TextContent("Confluence page attachments:\n"));

//                    if (confluencePageAttachments.Attachments?.Any() == true)
//                    {
//                        await AddConfluenceAttachment(userPrompt, confluencePageAttachments, summarizeAttachment);
//                    }

//                    history.AddUserMessage(userPrompt);

//                    response = await _chatCompletionService.GetChatMessageContentAsync(history);
//                }
//                catch (HttpOperationException ex) when (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 413) // Error status code when chatHistory excees token limit
//                {
//                    history = new ChatHistory();

//                    history.AddSystemMessage(systemPrompt);

//                    history.AddUserMessage(JsonSerializer.Serialize(confluencePage, new JsonSerializerOptions { WriteIndented = true }));

//                    userPrompt.Add(new TextContent("Confluece page attachments:\n"));

//                    if (confluencePageAttachments.Attachments?.Any() == true)
//                    {
//                        await AddConfluenceAttachment(userPrompt, confluencePageAttachments, true);
//                    }

//                    response = await _chatCompletionService.GetChatMessageContentAsync(history);
//                }

//                return response.ToString().Trim();
//            }
//            //catch (Exception ex)
//            catch (Exception ex) when (!(ex is DomainException))
//            {


//                throw new ConfluencePageSummaryException(pageId, ex.Message);
//            }
//        }




//        private async Task AddConfluenceAttachment(ChatMessageContentItemCollection userPrompt, ConfluencePageAttachmentsDto confluencePageAttachments, bool summarizeAttachment)
//        {
//            userPrompt.Add(new TextContent("Confluence page attachments:\n"));

//            if (confluencePageAttachments.Attachments?.Any() == true)
//            {
//                if (summarizeAttachment)
//                {
//                    var downloadedAttachments = await _attachmentListDownloadService.DownloadAttachmentsAsync(confluencePageAttachments.Attachments);
//                    var summarizedAttachments = await _attachmentListSummaryService.SummarizeAttachmentsAsync(downloadedAttachments);
//                    foreach (var attachment in summarizedAttachments)
//                    {
//                        userPrompt.Add(new TextContent(attachment));
//                    }
//                }
//                else
//                {
//                    var downloadedAttachments = await _attachmentListDownloadService.DownloadAttachmentsAsync(confluencePageAttachments.Attachments);
//                    foreach (var attachment in downloadedAttachments)
//                    {
//                        userPrompt.Add((attachment));
//                    }
//                }
//            }
//        }

//    }
//}


//        //Console.WriteLine("\n\n\n\n\n\n\n\n\n\n");
//        //        Console.WriteLine(ex);
//        //        Console.WriteLine("\n\n\n\n\n\n\n\n\n\n");
//        //public async Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl, bool summarizeAttachment)
//        //{
//        //    if (confluencePage == null)
//        //        throw new ArgumentNullException(nameof(confluencePage));

//        //    if (string.IsNullOrWhiteSpace(baseUrl))
//        //        throw new ArgumentException("Base URL must be provided.", nameof(baseUrl));

//        //    string pageId = confluencePage.Id ?? "unknown";
//        //    string systemPrompt = LoadSystemPrompt();

//        //    try
//        //    {
//        //        // Fetch attachments
//        //        var json = await _confluenceAttachmentsService.GetAttachmentsAsync(baseUrl, pageId);
//        //        ConfluencePageAttachmentsDto attachmentsDto = _confluenceAttachmentsMapper.MapToDto(json);

//        //        // 1st attempt
//        //        var response = await TrySummarizePageAsync(
//        //            confluencePage,
//        //            attachmentsDto,
//        //            summarizeAttachment,
//        //            systemPrompt);

//        //        if (response != null)
//        //            return response.ToString().Trim();

//        //        // Retry on token overflow (summarize attachments = true)
//        //        _logger.LogWarning("Token limit exceeded for Confluence page {PageId}. Retrying with summarizeAttachment=true.", pageId);

//        //        response = await TrySummarizePageAsync(
//        //            confluencePage,
//        //            attachmentsDto,
//        //            summarizeAttachment: true,
//        //            systemPrompt);

//        //        if (response == null)
//        //            throw new ConfluencePageSummaryException(pageId, "Failed to summarize page after retry.");

//        //        return response.ToString().Trim();
//        //    }
//        //    catch (Exception ex) when (ex is not DomainException)
//        //    {
//        //        throw new ConfluencePageSummaryException(pageId, ex.Message);
//        //    }
//        //}


//        //private async Task<ChatMessageContent?> TrySummarizePageAsync(
//        //    ConfluencePageDto page,
//        //    ConfluencePageAttachmentsDto attachmentsDto,
//        //    bool summarizeAttachment,
//        //    string systemPrompt)
//        //{
//        //    var history = new ChatHistory();
//        //    history.AddSystemMessage(systemPrompt);

//        //    ChatMessageContentItemCollection userPrompt = await BuildUserPromptAsync(
//        //        page,
//        //        attachmentsDto,
//        //        summarizeAttachment);

//        //    history.AddUserMessage(userPrompt);

//        //    try
//        //    {
//        //        return await _chatCompletionService.GetChatMessageContentAsync(history);
//        //    }
//        //    catch (HttpOperationException ex) when (IsTokenLimitError(ex))
//        //    {
//        //        return null; // signal retry
//        //    }
//        //}

//        //private async Task<ChatMessageContentItemCollection> BuildUserPromptAsync(
//        //    ConfluencePageDto page,
//        //    ConfluencePageAttachmentsDto attachmentsDto,
//        //    bool summarizeAttachment)
//        //{
//        //    var userPrompt = new ChatMessageContentItemCollection();

//        //    // Page JSON
//        //    userPrompt.Add(new TextContent(
//        //        JsonSerializer.Serialize(page, new JsonSerializerOptions { WriteIndented = true })
//        //    ));

//        //    // Attachments
//        //    userPrompt.Add(new TextContent("Confluence page attachments:\n"));

//        //    if (attachmentsDto.Attachments?.Any() == true)
//        //    {
//        //        await AddAttachmentsToPromptAsync(userPrompt, attachmentsDto, summarizeAttachment);
//        //    }

//        //    return userPrompt;
//        //}


//        //private async Task AddAttachmentsToPromptAsync(
//        //    ChatMessageContentItemCollection userPrompt,
//        //    ConfluencePageAttachmentsDto attachmentDto,
//        //    bool summarizeAttachment)
//        //{
//        //    var attachments = attachmentDto.Attachments;
//        //    if (attachments == null || !attachments.Any())
//        //        return;

//        //    var downloaded = await _attachmentListDownloadService.DownloadAttachmentsAsync(attachments);

//        //    if (summarizeAttachment)
//        //    {
//        //        var summaries = await _attachmentListSummaryService.SummarizeAttachmentsAsync(downloaded);
//        //        foreach (var summary in summaries)
//        //            userPrompt.Add(new TextContent(summary));
//        //    }
//        //    else
//        //    {
//        //        foreach (var attachment in downloaded)
//        //            userPrompt.Add(attachment);
//        //    }
//        //}

//        // ---------
//        //}
//        //}




////    }
////}
