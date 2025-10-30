namespace dayforce_assignment.Server.Interfaces
{
    public interface IJiraStoryService
    {
        Task<string> GetJiraStoryAsync (string jiraId);
    }
}
