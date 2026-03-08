using BirthdayCollator.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBirthdayCors()
    .AddWikiHttpClients()
    .AddBirthdayCore()
    .AddBirthdayPipelines()
    .AddBirthdayAi()
    .AddAiHttpClients();


//builder.Services.AddVectorStore(builder.Configuration);
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
    app.UseHttpsRedirection(); 
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

//app.Logger.LogInformation("Environment: " + app.Environment.EnvironmentName);

app.Run();
