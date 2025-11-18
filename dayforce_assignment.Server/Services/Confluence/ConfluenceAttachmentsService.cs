using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Net;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceAttachmentsService : IConfluenceAttachmentsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluenceAttachmentsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonElement> GetConfluenceAttachmentsAsync(string baseUrl, string id)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence base URL is required.",
                    internalMessage: "baseUrl parameter was null or empty."
                );

            if (string.IsNullOrWhiteSpace(id))
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence page ID is required.",
                    internalMessage: "id parameter was null or empty."
                );

            try
            {
                var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
                var url = new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/attachments");

                var response = await httpClient.GetAsync(url);                    

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                        throw new ApiException(
                        StatusCodes.Status400BadRequest,
                        "Invalid request to Confluence API.",
                        internalMessage: $"Confluence API returned 400 for page ID '{id}' and baseUrl '{baseUrl}'."
                    );
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ApiException(
                        StatusCodes.Status404NotFound,
                        "Confluence page not found.",
                        internalMessage: $"No attachments found for page ID '{id}' at '{baseUrl}'."
                    );
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ApiException(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized to access Confluence attachments.",
                        internalMessage: $"Unauthorized HTTP request to '{url}'."
                    );
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined || jsonResponse.ValueKind == JsonValueKind.Null)
                {
                    return new JsonElement(); // return empty JSON safely
                }

                return jsonResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Failed to call Confluence attachments API.",
                    internalMessage: ex.ToString()
                );
            }
            catch (NotSupportedException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "The content type from Confluence API is not supported.",
                    internalMessage: ex.ToString()
                );
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Error parsing Confluence attachments JSON.",
                    internalMessage: ex.ToString()
                );
            }
        }
    }
}
