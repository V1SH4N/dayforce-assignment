namespace dayforce_assignment.Server.Interfaces
{
    public interface IConfluencePageService
    {
        Task<string> GetConfluencePageAsync(string pageId);
    }
}
