using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class UserPromptBuilder : IUserPromptBuilder
    {
        private readonly Kernel _kernel;
        private readonly IJiraStoryService _jiraStoryService;
        private readonly IJiraStoryCleaner _jiraStoryCleaner;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageService _confluencePageService;
        private readonly IConfluencePageCleaner _confluencePageCleaner;
        private readonly IConfluenceAttachmentsService _confluenceAttachmentsService;
        private readonly IConfluenceAttachmentsCleaner _confluenceAttachmentsCleaner;
        private readonly IAttachmentDownloadService _attachmentDownloadService;

        public UserPromptBuilder(
            Kernel kernel,
            IJiraStoryService jiraStoryService,
            IJiraStoryCleaner jiraStoryCleaner,
            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
            IConfluencePageService confluencePageService,
            IConfluencePageCleaner confluencePageCleaner,
            IConfluenceAttachmentsService confluenceAttachmentsService,
            IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner,
            IAttachmentDownloadService attachmentDownloadService
            )
        {
            _kernel = kernel;
            _jiraStoryService = jiraStoryService;
            _jiraStoryCleaner = jiraStoryCleaner;
            _confluencePageReferenceExtractor = confluencePageReferenceExtractor;
            _confluencePageService = confluencePageService;
            _confluencePageCleaner = confluencePageCleaner;
            _confluenceAttachmentsService = confluenceAttachmentsService;
            _confluenceAttachmentsCleaner = confluenceAttachmentsCleaner;
            _attachmentDownloadService = attachmentDownloadService;
        }

        public async Task<ChatHistory> BuildUserPromptAsync(string jiraId)
        {
            var kernelInstance = _kernel.Clone();
            var chatService = kernelInstance.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();

            // Jira Story

            JsonElement rawJiraStory = await _jiraStoryService.GetJiraStoryAsync(jiraId);
            JiraStoryDto cleanedJiraStory = _jiraStoryCleaner.CleanJiraStory(rawJiraStory);

            history.AddUserMessage("Jira story:");
            history.AddUserMessage(JsonSerializer.Serialize(cleanedJiraStory));

            if (cleanedJiraStory.Attachments != null && cleanedJiraStory.Attachments.Count != 0) // need to verify this check
            {
                foreach (string downloadLink in cleanedJiraStory.Attachments)
                {
                    ImageContent jiraAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(downloadLink);
                    history.AddUserMessage([jiraAttachment]);
                }
            }

            // Confluence Pages

            ConfluencePageReferenceDto confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(cleanedJiraStory);

            if (confluencePageReferences.ConfluencePages != null && confluencePageReferences.ConfluencePages.Count != 0)
            {
                
                foreach (ConfluencePage confluencePage in confluencePageReferences.ConfluencePages)
                {
                    history.AddUserMessage("For context, below is the related confluence page:");

                    JsonElement rawConfluencePage = await _confluencePageService.GetConfluencePageAsync(confluencePage.baseUrl, confluencePage.pageId);

                    ConfluencePageDto cleanedConfluencePage = _confluencePageCleaner.CleanConfluencePage(rawConfluencePage);

                    JsonElement rawConfluencePageAttachments = await _confluenceAttachmentsService.GetConfluenceAttachmentsAsync(confluencePage.baseUrl, confluencePage.pageId);

                    ConfluencePageAttachmentsDto cleanedConfluencePageAttachments = _confluenceAttachmentsCleaner.CleanConfluenceAttachments(rawConfluencePageAttachments);

                    if (cleanedConfluencePageAttachments.Attachments != null && cleanedConfluencePageAttachments.Attachments.Count != 0)  // need to verify this check
                    {
                        foreach (Attachment attachment in cleanedConfluencePageAttachments.Attachments)
                        {
                            ImageContent confluenceAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(attachment.DownloadLink);
                            history.AddUserMessage([confluenceAttachment]);
                        }
                    }
                }
            }


            return history;

        }
    }
}
