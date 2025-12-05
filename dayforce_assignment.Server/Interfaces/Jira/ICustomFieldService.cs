using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface ICustomFieldService
    {
        string GetCustomFieldId(JsonElement jsonIssue, string fieldName);
    }
}
