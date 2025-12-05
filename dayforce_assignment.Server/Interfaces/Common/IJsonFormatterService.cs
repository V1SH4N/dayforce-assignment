using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IJsonFormatterService
    {
        JsonElement FormatJson(string responseString);
    }
}
