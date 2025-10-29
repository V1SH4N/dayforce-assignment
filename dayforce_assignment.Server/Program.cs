using dayforce_assignment.Server.Interfaces;
using dayforce_assignment.Server.Services;
using DotNetEnv;
using System.Net.Http;

Env.Load();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add dayforce hhtp client
builder.Services.AddHttpClient("dayforce", client =>
{
    client.BaseAddress = new Uri("https://dayforce.atlassian.net/");
    string authenticationString = $"{Environment.GetEnvironmentVariable("dayforceEmail")}:{Environment.GetEnvironmentVariable("dayforceToken")}";
    byte[] authenticationBytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
    string base64EncodedAuthenticationString = Convert.ToBase64String(authenticationBytes);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
});

// Add test hhtp client
builder.Services.AddHttpClient("test", client =>
{
    client.BaseAddress = new Uri("https://vishandaby305.atlassian.net/");
    string authenticationString = $"{Environment.GetEnvironmentVariable("testEmail")}:{Environment.GetEnvironmentVariable("testToken")}";
    byte[] authenticationBytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
    string base64EncodedAuthenticationString = Convert.ToBase64String(authenticationBytes);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
});

// Inject JiraStoryService 
builder.Services.AddScoped<IJiraStoryService, JiraStoryService>(); // need to check scope of service, added as scoped for now

// Inject SearchConfluenceService
builder.Services.AddScoped<ISearchConfluencePageService, SearchConfluencePageService>();


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

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
