using dayforce_assignment.Server.Interfaces.Common;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Common
{
    public class JsonFormatterService : IJsonFormatterService
    {
        public JsonElement FormatJson(string responseString)
        {

            int start = responseString.IndexOf('{');
            int last = responseString.LastIndexOf('}') + 1;

            string trimmedResponse = responseString.Substring(start, last - start);

            var jsonDocument = JsonDocument.Parse(trimmedResponse);
            return jsonDocument.RootElement;

        }
    }
}
