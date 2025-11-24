using Aida.Client;
using Aida.Client.Extensions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using dayforce_assignment.Server.Services;
using dayforce_assignment.Server.Services.Common;
using dayforce_assignment.Server.Services.Confluence;
using dayforce_assignment.Server.Services.Orchestrator;
using DotNetEnv;
using Microsoft.SemanticKernel;
using System.ClientModel.Primitives;
using dayforce_assignment.Server.Configuration;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

// AIDA options
AidaApiOptions aidaApiOptions = new()
{
    Environment = AidaApiEnvironment.Development
};

// Atlassian API options
var atlassianOptions = new AtlassianApiOptions
{
    AuthEmail = Environment.GetEnvironmentVariable("AUTH_EMAIL") ?? string.Empty,
    AuthToken = Environment.GetEnvironmentVariable("AUTH_TOKEN") ?? string.Empty
};

builder.Services.AddSingleton(atlassianOptions);

// HttpClient configuration
builder.Services.AddHttpClient("AtlassianAuthenticatedClient", client =>
{
    var credentials = $"{atlassianOptions.AuthEmail}:{atlassianOptions.AuthToken}";
    var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);
});


// Jira services
builder.Services.AddScoped<IJiraIssueService, JiraIssueService>();
builder.Services.AddScoped<IJiraRemoteLinksService, JiraRemoteLinksService>();
builder.Services.AddScoped<IJiraIssueCleaner, JiraIssueCleaner>();

// Confluence Services
builder.Services.AddScoped<IConfluencePageService, ConfluencePageService>();
builder.Services.AddScoped<IConfluencePageCleaner, ConfluencePageCleaner>();
builder.Services.AddScoped<IConfluencePageReferenceExtractor, ConfluencePageReferenceExtractor>();
builder.Services.AddScoped<IConfluenceAttachmentsService, ConfluenceAttachmentsService>();
builder.Services.AddScoped<IConfluenceCommentsService, ConfluenceCommentsService>();
builder.Services.AddScoped<IConfluenceAttachmentsCleaner, ConfluenceAttachmentsCleaner>();
builder.Services.AddScoped<IConfluencePageSearchService, ConfluencePageSearchService>();
builder.Services.AddScoped<IConfluencePageSearchParameterService, ConfluencePageSearchParameterService>();
builder.Services.AddScoped<IConfluencePageSearchOrchestrator, ConfluencePageSearchOrchestrator>();
builder.Services.AddScoped<IConfluencePageSearchFilterService, ConfluencePageSearchFilterService>();
builder.Services.AddScoped<IConfluencePageSummaryService, ConfluencePageSummaryService>();

// Common Services
builder.Services.AddScoped<IAttachmentDownloadService, AttachmentDownloadService>();
builder.Services.AddScoped<IJsonFormatterService, JsonFormatterService>();

// Orchestrator Services
builder.Services.AddScoped<IUserMessageBuilder, UserMessageBuilder>();
builder.Services.AddScoped<ITestCaseGeneratorService, TestCaseGeneratorService>();

// Aida services
builder.Services.AddSingleton(aidaApiOptions);
builder.Services.AddSingleton(ClientPipeline.Create());
builder.Services.AddSingleton<IAidaApiClient, AidaApiClient>();
builder.Services.AddAidaOpenAIClient(aidaApiOptions, "Test Team", "Test Tool");
builder.Services.AddKernel();
builder.Services.AddOpenAIChatCompletion("chat4o_global");
//builder.Services.AddOpenAIChatCompletion("o3-mini");
//builder.Services.AddOpenAIChatCompletion("gpt-5-mini");
//builder.Services.AddOpenAIChatCompletion("o4-mini");


// Handle CORS  
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowReactApp");

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
