using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface ICustomFieldService
    {
        string GetCustomFieldId(JsonElement jiraJson, string fieldName);
    }
}
