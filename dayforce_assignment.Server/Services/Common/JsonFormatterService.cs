using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Exceptions;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Common
{
    public class JsonFormatterService : IJsonFormatterService
    {
        //Formats jsonResponse from Ai to valid json.
        public JsonElement FormatJson(string responseString)
        {
            if (string.IsNullOrWhiteSpace(responseString))
                throw new JsonFormattingException("Response string is null or empty.");

            int start = responseString.IndexOf('{');
            int last = responseString.LastIndexOf('}') + 1;

            if (start == -1 || last == 0 || last <= start)
                throw new JsonFormattingException("Response string does not contain valid JSON object.");

            string trimmedResponse = responseString.Substring(start, last - start);
            Console.WriteLine($"\n\n\n\n\n\n\n{trimmedResponse}");
            try
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(trimmedResponse);
                return jsonResponse;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"\n\n\n\n\n\n{ex.Message}");
                return new JsonElement();
                throw new JsonFormattingException($"An unexpected error has occured");
            }
        }








        //public JsonElement FormatJson(string responseString)
        //{
        //    if (string.IsNullOrWhiteSpace(responseString))
        //        throw new JsonFormattingException("Response string is null or empty.");

        //    int start = responseString.IndexOf('{');
        //    int last = responseString.LastIndexOf('}') + 1;

        //    if (start == -1 || last == 0 || last <= start)
        //        throw new JsonFormattingException("Response string does not contain valid JSON object.");

        //    string trimmedResponse = responseString.Substring(start, last - start);
        //    Console.WriteLine($"\nTrimmed JSON:\n{trimmedResponse}");

        //    try
        //    {
        //        using var doc = JsonDocument.Parse(trimmedResponse);
        //        return doc.RootElement.Clone(); // clone to return JsonElement safely
        //    }
        //    catch (JsonException ex)
        //    {
        //        Console.WriteLine($"\nJSON Parsing Error: {ex.Message}");
        //        throw new JsonFormattingException("Failed to parse JSON response.");
        //    }
        //}

    }
}


