using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Net;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageService : IConfluencePageService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluencePageService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonElement> GetConfluencePageAsync(string baseUrl, string id)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence base URL is missing.",
                    detail: "Cannot call Confluence API because the base URL is empty or null.",
                    internalMessage: "GetConfluencePageAsync called with empty baseUrl parameter."
                );
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence page ID is missing.",
                    detail: "Cannot retrieve Confluence page because the ID is empty or null.",
                    internalMessage: "GetConfluencePageAsync called with empty id parameter."
                );
            }

            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
            HttpResponseMessage response;

            try
            {
                response = await httpClient.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}?body-format=storage"));
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Failed to call Confluence API.",
                    internalMessage: ex.ToString()
                );
            }
            catch (NotSupportedException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Unsupported content type returned from Confluence API.",
                    internalMessage: ex.ToString()
                );
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Error parsing Confluence API response.",
                    internalMessage: ex.ToString()
                );
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    {
                        throw new ApiException(
                            StatusCodes.Status502BadGateway,
                            "Confluence API returned empty response.",
                            internalMessage: "ReadFromJsonAsync returned undefined JsonElement."
                        );
                    }
                    return jsonResponse;

                case HttpStatusCode.BadRequest:
                    throw new ApiException(
                        StatusCodes.Status400BadRequest,
                        "Invalid request to Confluence API.",
                        internalMessage: $"Confluence API returned 400 for page ID '{id}' and baseUrl '{baseUrl}'."
                    );

                case HttpStatusCode.Unauthorized:
                    throw new ApiException(
                        StatusCodes.Status401Unauthorized,
                        "Invalid Confluence credentials.",
                        internalMessage: $"Unauthorized access when calling Confluence API for page ID '{id}'."
                    );

                case HttpStatusCode.NotFound:
                    throw new ApiException(
                        StatusCodes.Status404NotFound,
                        "Confluence page not found.",
                        internalMessage: $"Confluence API returned 404 for page ID '{id}'."
                    );

                default:
                    throw new ApiException(
                        (int)response.StatusCode,
                        $"Unexpected status code {(int)response.StatusCode} from Confluence API.",
                        internalMessage: $"Response from Confluence API for page ID '{id}': {response.StatusCode}."
                    );
            }
        }
    }
}
