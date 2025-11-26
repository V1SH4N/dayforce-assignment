using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class CustomFieldService : ICustomFieldService
    {
        public string GetCustomFieldId(JsonElement jiraJson, string fieldName)
        {
            try
            {
                if (jiraJson.ValueKind == JsonValueKind.Undefined ||
                    jiraJson.ValueKind == JsonValueKind.Null)
                {
                    throw new JiraCustomFieldLookupException("Input JSON is null or undefined.");
                }

                if (!jiraJson.TryGetProperty("names", out var names) ||
                    names.ValueKind != JsonValueKind.Object)
                {
                    return string.Empty;
                }

                foreach (var prop in names.EnumerateObject())
                {
                    if (prop.Value.GetString() == fieldName)
                    {
                        return prop.Name;
                    }
                }

                return string.Empty;
            }
            catch (JsonException ex)
            {
                throw new JiraCustomFieldLookupException($"Malformed JSON structure: {ex.Message}");
            }
            catch (Exception ex) when (ex is not JiraException)
            {
                throw new JiraCustomFieldLookupException($"An unexpected error has occured");
            }
        }
    }
}
