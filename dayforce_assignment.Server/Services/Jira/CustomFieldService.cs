using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class CustomFieldService : ICustomFieldService
    {
        // Gets custom field id mapping for fieldName. Returns empty string if not found. 
        public string GetCustomFieldId(JsonElement jsonIssue, string fieldName)
        {
            if (!jsonIssue.TryGetProperty("names", out var names) ||
                names.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            foreach (var prop in names.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == fieldName)
                {
                    return prop.Name;
                }
            }
            return string.Empty;
        }
    }
}
