using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace dayforce_assignment.Server.Exceptions
{

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain exception occurred.");
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "External service request failed.");
                await HandleProblemDetailsAsync(
                    context,
                    "External service error",
                    "Failed to fetch data from Jira or Confluence.",
                    StatusCodes.Status503ServiceUnavailable
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected server error.");
                await HandleProblemDetailsAsync(
                    context,
                    "Internal server error",
                    "An unexpected error occurred. Please try again later.",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        private static Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
        {
            int status = ex switch
            {

                // Common Services Exceptions
                UnsupportedAttachmentMediaTypeException => StatusCodes.Status415UnsupportedMediaType,
                AttachmentDownloadException => StatusCodes.Status502BadGateway,
                AttachmentSummaryException => StatusCodes.Status500InternalServerError,
                JsonFormattingException => StatusCodes.Status500InternalServerError,


                // Jira Exceptions
                JiraIssueNotFoundException => StatusCodes.Status404NotFound,
                JiraUnauthorizedException => StatusCodes.Status401Unauthorized,
                JiraRemoteLinksForbiddenException => StatusCodes.Status403Forbidden,
                JiraBadRequestException => StatusCodes.Status400BadRequest,
                JiraRemoteLinksPayloadTooLargeException => StatusCodes.Status413PayloadTooLarge,
                JiraRemoteLinksNotFoundException => StatusCodes.Status404NotFound,
                JiraIssueMappingException => StatusCodes.Status500InternalServerError,
                JiraApiException => StatusCodes.Status502BadGateway,    
                TriageSubtaskMappingException => StatusCodes.Status500InternalServerError,
                JiraException => StatusCodes.Status503ServiceUnavailable,


                // Confluence Exceptions
                ConfluencePageNotFoundException => StatusCodes.Status404NotFound,
                ConfluenceUnauthorizedException => StatusCodes.Status401Unauthorized,
                ConfluenceBadRequestException => StatusCodes.Status400BadRequest,
                ConfluenceApiException => StatusCodes.Status502BadGateway,
                ConfluenceCommentsNotFoundException => StatusCodes.Status404NotFound,   
                ConfluenceAttachmentsNotFoundException => StatusCodes.Status404NotFound,
                ConfluenceSearchBadRequestException => StatusCodes.Status400BadRequest,
                ConfluencePageReferenceExtractionException => StatusCodes.Status500InternalServerError,
                ConfluenceSearchParameterException => StatusCodes.Status500InternalServerError,
                ConfluenceSearchFilterException => StatusCodes.Status500InternalServerError,
                ConfluencePageSummaryException => StatusCodes.Status500InternalServerError,
                ConfluenceException => StatusCodes.Status503ServiceUnavailable,

                // Orchestrator Services Exceptions
                TestCaseGenerationException => StatusCodes.Status500InternalServerError,

                // Atlassian Exceptions
                AtlassianConfigurationException => StatusCodes.Status500InternalServerError,

                _ => StatusCodes.Status500InternalServerError
            };


            return HandleProblemDetailsAsync(
                context,
                ex.GetType().Name,
                ex.Message,
                status
            );
        }

        private static Task HandleProblemDetailsAsync(HttpContext context, string title, string detail, int statusCode)
        {
            var problem = new ProblemDetails
            {
                Type = $"https://example.com/errors/{title.Replace(" ", "-").ToLower()}",
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(problem);
        }
    }

}
