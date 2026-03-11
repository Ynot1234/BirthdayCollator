using BirthdayCollator.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---
builder.Services
    .AddBirthdayCors()
    .AddWikiHttpClients()
    .AddBirthdayCore()
    .AddBirthdayHelpers()
    .AddBirthdayFactories()
    .AddBirthdayParsers()
    .AddBirthdayPipelines()
    .AddBirthdaySources()
    .AddBirthdayProcessing()
    .AddBirthdayAi();
   // .AddAiHttpClients();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();

var app = builder.Build();

// --- Middleware ---
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
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
