using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Exceptions;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Common
{
    public class JsonFormatterService : IJsonFormatterService
    {
        public JsonElement FormatJson(string responseString)
        {
            if (string.IsNullOrWhiteSpace(responseString))
                throw new JsonFormattingException("Response string is null or empty.");

            int start = responseString.IndexOf('{');
            int last = responseString.LastIndexOf('}') + 1;

            if (start == -1 || last == 0 || last <= start)
                throw new JsonFormattingException("Response string does not contain valid JSON object.");

            string trimmedResponse = responseString.Substring(start, last - start);

            try
            {
                var jsonDocument = JsonDocument.Parse(trimmedResponse);
                return jsonDocument.RootElement;
            }
            catch (JsonException ex)
            {
                throw new JsonFormattingException($"An unexpected error has occured");
            }
        }
    }
}
