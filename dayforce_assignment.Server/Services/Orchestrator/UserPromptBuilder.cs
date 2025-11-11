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
        private readonly IJiraStoryService _jiraStoryService;
        private readonly IJiraStoryCleaner _jiraStoryCleaner;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageService _confluencePageService;
        private readonly IConfluencePageCleaner _confluencePageCleaner;
        private readonly IConfluenceAttachmentsService _confluenceAttachmentsService;
        private readonly IConfluenceAttachmentsCleaner _confluenceAttachmentsCleaner;
        private readonly IAttachmentDownloadService _attachmentDownloadService;

        public UserPromptBuilder(
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
            var history = new ChatHistory();

            // Jira Story

            JsonElement rawJiraStory = await _jiraStoryService.GetJiraStoryAsync(jiraId);
            JiraStoryDto cleanedJiraStory = _jiraStoryCleaner.CleanJiraStory(rawJiraStory);

            history.AddUserMessage("Jira story:");
            history.AddUserMessage(JsonSerializer.Serialize(cleanedJiraStory));

            if (cleanedJiraStory.Attachments?.Count > 0)
            {
                foreach (string downloadLink in cleanedJiraStory.Attachments)
                {
                    ImageContent jiraAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(downloadLink);
                    history.AddUserMessage([jiraAttachment]);
                }
            }

            // Confluence Page(s)

            ConfluencePageReferencesDto confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(cleanedJiraStory);

            if (confluencePageReferences.ConfluencePages?.Count > 0)
            {
                
                foreach (ConfluencePage confluencePage in confluencePageReferences.ConfluencePages)
                {
                    history.AddUserMessage("For context, below is the related confluence page:");

                    JsonElement rawConfluencePage = await _confluencePageService.GetConfluencePageAsync(confluencePage.baseUrl, confluencePage.pageId);

                    ConfluencePageDto cleanedConfluencePage = _confluencePageCleaner.CleanConfluencePage(rawConfluencePage);

                    JsonElement rawConfluencePageAttachments = await _confluenceAttachmentsService.GetConfluenceAttachmentsAsync(confluencePage.baseUrl, confluencePage.pageId);

                    ConfluencePageAttachmentsDto cleanedConfluencePageAttachments = _confluenceAttachmentsCleaner.CleanConfluenceAttachments(rawConfluencePageAttachments);

                    if (confluencePageReferences?.ConfluencePages?.Count > 0)
                    {
                        foreach (Attachment attachment in cleanedConfluencePageAttachments.Attachments)
                        {
                            ImageContent confluenceAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(attachment.DownloadLink);
                            history.AddUserMessage([confluenceAttachment]);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("this part handles searching for additional confluence pages");
            }


                return history;

        }
    }
}
