using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Common;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Common
{
    public class JsonFormatterService : IJsonFormatterService
    {
        public JsonElement FormatJson(string responseString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    throw new ApiException(
                        StatusCodes.Status400BadRequest,
                        title: "The input JSON string is empty.",
                        detail: "No content was provided to format as JSON.",
                        internalMessage: "FormatJson received null or empty string.");
                }

                int start = responseString.IndexOf('{');
                int last = responseString.LastIndexOf('}') + 1;

                if (start < 0 || last <= 0 || last <= start)
                {
                    throw new ApiException(
                        StatusCodes.Status400BadRequest,
                        title: "Invalid JSON format in input string.",
                        detail: "The input string does not contain valid JSON object delimiters.",
                        internalMessage: $"Invalid JSON string: {responseString}");
                }

                string trimmedResponse = responseString.Substring(start, last - start);

                var jsonDocument = JsonDocument.Parse(trimmedResponse);
                return jsonDocument.RootElement;
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    title: "Failed to parse JSON string.",
                    detail: "The provided string could not be converted into a JSON document.",
                    internalMessage: ex.ToString());
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    StatusCodes.Status500InternalServerError,
                    title: "Unexpected error formatting JSON.",
                    detail: "An unexpected error occurred while processing the input string.",
                    internalMessage: ex.ToString());
            }
        }
    }
}
