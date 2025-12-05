using Aida.Client;
using Aida.Client.Extensions;
using dayforce_assignment.Server.Configuration;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using dayforce_assignment.Server.Services.Common;
using dayforce_assignment.Server.Services.Confluence;
using dayforce_assignment.Server.Services.Jira;
using dayforce_assignment.Server.Services.Orchestrator;
using DotNetEnv;
using Microsoft.SemanticKernel;
using System.ClientModel.Primitives;
using System.Text;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Console logging
builder.Logging.AddConsole();

// AIDA options
AidaApiOptions aidaApiOptions = new()
{
    Environment = AidaApiEnvironment.Development
};

// Atlassian API options
var atlassianOptions = new AtlassianApiOptions
{
    AuthEmail = Environment.GetEnvironmentVariable("AUTH_EMAIL") ?? throw new AtlassianConfigurationException("Atlassian authentication email is not configured."),
    AuthToken = Environment.GetEnvironmentVariable("AUTH_TOKEN") ?? throw new AtlassianConfigurationException("Atlassian authentication token is not configured.")
};

builder.Services.AddSingleton(atlassianOptions);

// HttpClient configuration
builder.Services.AddHttpClient("AtlassianAuthenticatedClient", client =>
{
    var credentials = $"{atlassianOptions.AuthEmail}:{atlassianOptions.AuthToken}";
    var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));


    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);
});


// Jira services
builder.Services.AddScoped<IJiraHttpClientService, JiraHttpClientService>();
builder.Services.AddScoped<IJiraMapperService, JiraMapperService>();
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
builder.Services.AddScoped<ITriageSubtaskService, TriageSubtaskService>();


// Confluence Services
builder.Services.AddScoped<IConfluenceHttpClientService, ConfluenceHttpClientService>();
builder.Services.AddScoped<IConfluenceMapperService, ConfluenceMapperService>();
builder.Services.AddScoped<IConfluencePageReferenceExtractor, ConfluencePageReferenceExtractor>();
builder.Services.AddScoped<IConfluencePageSummaryService, ConfluencePageSummaryService>();
builder.Services.AddScoped<IConfluenceSearchService, ConfluenceSearchService>();


// Common Services
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IJsonFormatterService, JsonFormatterService>();


// Orchestrator Services
builder.Services.AddScoped<IUserPromptBuilder, UserPromptBuilder>();
builder.Services.AddScoped<ITestCaseGeneratorService, TestCaseGeneratorService>();
builder.Services.AddScoped<IConfluencePageSearchOrchestrator, ConfluencePageSearchOrchestrator>();


// Aida services
builder.Services.AddSingleton(aidaApiOptions);
builder.Services.AddSingleton(ClientPipeline.Create());
builder.Services.AddSingleton<IAidaApiClient, AidaApiClient>();
builder.Services.AddAidaOpenAIClient(aidaApiOptions, "Test Team", "Test Tool");
builder.Services.AddKernel();
builder.Services.AddOpenAIChatCompletion("chat4o_global");

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

app.UseMiddleware<GlobalExceptionMiddleware>();

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
