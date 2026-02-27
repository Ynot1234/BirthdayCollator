using BirthdayCollator.Server.Extensions;
using BirthdayCollator.Server.Processing.Enrichment;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBirthdayCors()
    .AddWikiHttpClients()
    .AddBirthdayCore()
    .AddBirthdayPipelines()
    .AddBirthdayAi(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPersonEnrichmentService, PersonEnrichmentService>();


var app = builder.Build();

app.MapHealthChecks("/health");


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Logger.LogInformation("Environment: " + app.Environment.EnvironmentName);

app.Run();
