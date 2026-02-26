using BirthdayCollator.AI.Semantic;
using BirthdayCollator.AI.Services;
using BirthdayCollator.Processing;
using BirthdayCollator.Resources;
using BirthdayCollator.Server;
using BirthdayCollator.Server.Processing;
using BirthdayCollator.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------
// Shared + App Services
// ---------------------------------------------
builder.Services.AddScoped<PersonDeduper>();
builder.Services.AddScoped<PersonCleaner>();
builder.Services.AddScoped<PersonFilter>();
builder.Services.AddScoped<PersonSorter>();
builder.Services.AddScoped<NearDuplicateRemover>();
builder.Services.AddScoped<DeduplicateByURL>();
builder.Services.AddScoped<PersonDedupe>();
builder.Services.AddScoped<PersonAIEnricher>();
builder.Services.AddScoped<PersonWikiEnricher>();

// ---------------------------------------------
// API Controllers
// ---------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------
// CORS (single correct policy)
// ---------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        //policy.WithOrigins("http://localhost:5178", 
        //                   "http://localhost:5173",
        //                   "http://localhost:5174",
        //                   "http://localhost:5176")
        //      .AllowAnyHeader()
        //      .AllowAnyMethod();
       policy.SetIsOriginAllowed(_ => true)
      .AllowAnyHeader()
      .AllowAnyMethod();
    });
});

// ---------------------------------------------
// SHARED HTTP CLIENT CONFIGURATION
// ---------------------------------------------
static void ConfigureWikiClient(HttpClient client)
{
    client.Timeout = Timeout.InfiniteTimeSpan;
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "BirthdayApp/1.0 (https://github.com/anthony/birthdayapp; anthony@example.com)");
}

// ---------------------------------------------
// HTTP CLIENTS
// ---------------------------------------------
builder.Services.AddHttpClient<OnThisDayHtmlFetcher>(ConfigureWikiClient);
//builder.Services.AddHttpClient<WikiBirthdayFetcher>(ConfigureWikiClient);

builder.Services.AddHttpClient("WikiClient", client =>
{
    ConfigureWikiClient(client);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true
});

// ---------------------------------------------
// FETCHERS + PARSERS
// ---------------------------------------------
builder.Services.AddScoped<PersonFactory>();

builder.Services.AddScoped<WikiHtmlFetcher>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new WikiHtmlFetcher(factory.CreateClient("WikiClient"));
});

builder.Services.AddScoped<OnThisDayParser>();

// ---------------------------------------------
// BIRTH SOURCES + PIPELINES
// ---------------------------------------------
builder.Services.AddScoped<IBirthSource, YearBirthSource>();
builder.Services.AddScoped<IBirthSource, DateBirthSource>();
builder.Services.AddScoped<IBirthSource, CategoryBirthSource>();
builder.Services.AddScoped<IBirthSource, GenariansBirthSource>();
builder.Services.AddScoped<IBirthSource, OnThisDaySource>();

builder.Services.AddScoped<BirthSourceEngine>();
builder.Services.AddScoped<IPersonPipeline, PersonPipeline>();
builder.Services.AddScoped<IFetchPipeline, FetchPipeline>();

builder.Services.AddSingleton<IYearRangeProvider, YearRangeProvider>();

// ---------------------------------------------
// Semantic Kernel + AI
// ---------------------------------------------
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddScoped<IAIService, AIService>();

// ---------------------------------------------
// MAIN FETCHERS
// ---------------------------------------------
builder.Services.AddScoped<BirthdayFetcher>();
builder.Services.AddScoped<Genarians>();
builder.Services.AddScoped<GenariansPageParser>();

builder.Services.AddSingleton<Func<string, string>>(sp =>
{
    return WikiUrlBuilder.NormalizeWikiHref;
});

var app = builder.Build();

// ---------------------------------------------
// Swagger
// ---------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------
// Middleware pipeline
// ---------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AllowFrontend");

// ---------------------------------------------
// API Endpoints
// ---------------------------------------------
app.MapControllers();

app.Logger.LogInformation("Environment: " + app.Environment.EnvironmentName);

app.Run();
