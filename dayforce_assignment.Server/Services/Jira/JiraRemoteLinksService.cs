using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Net;
using System.Text.Json;

public class JiraRemoteLinksService : IJiraRemoteLinksService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jiraBaseUrl;

    public JiraRemoteLinksService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraBaseUrl = configuration["Atlassian:BaseUrl"]
            ?? throw new ArgumentNullException("Jira base URL is not configured");
    }

    public async Task<JsonElement> GetJiraRemoteLinksAsync(string jiraId)
    {
        var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

        try
        {
            var response = await client.GetAsync(new Uri(new Uri(_jiraBaseUrl), $"rest/api/3/issue/{jiraId}/remotelink"));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (json.ValueKind == JsonValueKind.Undefined)
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "Jira API returned empty response",
                        internalMessage: "Received empty JSON from Jira Remote Links API.");

                return json;
            }

            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new ApiException(
                    StatusCodes.Status404NotFound,
                    "The requested Jira story could not be found.",
                    internalMessage: $"Jira story '{jiraId}' remote links not found."),

                HttpStatusCode.Unauthorized => new ApiException(
                    StatusCodes.Status401Unauthorized,
                    "Invalid Jira credentials",
                    internalMessage: "Unauthorized access to Jira API."),

                HttpStatusCode.Forbidden => new ApiException(
                    StatusCodes.Status403Forbidden,
                    "You do not have permission to access this Jira resource."),

                HttpStatusCode.BadRequest => new ApiException(
                    StatusCodes.Status400BadRequest,
                    "The request to Jira API was invalid."),

                _ when (int)response.StatusCode == 413 => new ApiException(
                    StatusCodes.Status413PayloadTooLarge,
                    "The request payload sent to Jira is too large."),

                _ => new ApiException(
                    (int)response.StatusCode,
                    $"Unexpected status code {(int)response.StatusCode} from Jira API",
                    internalMessage: $"Unexpected response from Jira Remote Links API")
            };
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                StatusCodes.Status502BadGateway,
                "Failed to call Jira API",
                internalMessage: ex.ToString());
        }
        catch (NotSupportedException ex)
        {
            throw new ApiException(
                StatusCodes.Status502BadGateway,
                "The content type from Jira API is not supported.",
                internalMessage: ex.ToString());
        }
        catch (JsonException ex)
        {
            throw new ApiException(
                StatusCodes.Status502BadGateway,
                "Error parsing Jira response JSON",
                internalMessage: ex.ToString());
        }
    }
}
