namespace dayforce_assignment.Server.Interfaces
{
    public interface ISearchConfluencePageService
    {
        Task<string> SearchConfluencePageAsync(string keywords);
    }
}
