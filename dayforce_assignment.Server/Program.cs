using dayforce_assignment.Server.Interfaces;
using dayforce_assignment.Server.Services;
using DotNetEnv;

Env.Load();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("JiraClient", client =>
{
    client.BaseAddress = new Uri("https://vishandaby305.atlassian.net/");
});

builder.Services.AddScoped<IJiraStoryService, JiraStoryService>(); // need to check scope of service, added as scoped for now





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
