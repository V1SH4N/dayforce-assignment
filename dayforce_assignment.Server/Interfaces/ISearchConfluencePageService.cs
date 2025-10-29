namespace dayforce_assignment.Server.Interfaces
{
    public interface ISearchConfluencePageService
    {
        Task<string> SearchPageAsync(string keywords);
    }
}
