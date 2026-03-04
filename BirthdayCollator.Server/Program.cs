using BirthdayCollator.Server.Extensions;

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

var app = builder.Build();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection(); // dev only
}
else
{
    app.UseExceptionHandler("/Error");
}

// Serve index.html automatically at "/" and enable SPA fallback
app.UseDefaultFiles();
app.UseStaticFiles();

// Allow frontend to call backend
app.UseCors("AllowFrontend");

app.MapControllers();

// SPA fallback for React router
app.MapFallbackToFile("index.html");

app.Logger.LogInformation("Environment: " + app.Environment.EnvironmentName);

app.Run();
