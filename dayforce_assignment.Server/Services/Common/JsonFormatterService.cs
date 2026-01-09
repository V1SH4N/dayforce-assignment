using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Exceptions;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Common
{
    public class JsonFormatterService : IJsonFormatterService
    {


        // strips invalid charcaters before & after json response
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
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(trimmedResponse);
                return jsonResponse;
            }
            catch (JsonException)
            {
                throw new JsonFormattingException($"An unexpected error has occured");
            }
        }


    }
}


