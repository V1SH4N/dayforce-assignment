namespace dayforce_assignment.Server.Interfaces
{
    public interface IJiraStoryService
    {
        Task<string> FetchJiraStoryAsync (string jiraId);
    }
}
