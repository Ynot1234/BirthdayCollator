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
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Logger.LogInformation("Environment: " + app.Environment.EnvironmentName);

app.Run();