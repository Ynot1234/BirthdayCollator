using BirthdayCollator.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBirthdayCollator();

var app = builder.Build();
app.UseBirthdayPipeline();
app.Run();