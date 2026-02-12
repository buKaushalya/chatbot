using RagApi.Api.Contracts;
using RagApi.Core.Options;
using RagApi.Database;
using RagApi.Database.Repositories;
using RagApi.Services.Chat;
using RagApi.Services.Embedding;
using RagApi.Services.Ingestion;
using RagApi.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Services.Configure<EmbeddingOptions>(builder.Configuration.GetSection("Embedding"));

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new DbConnectionFactory(builder.Configuration.GetConnectionString("Db")!));

// Repos
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Services
builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>(); // singleton: loads ONNX once
builder.Services.AddSingleton<IChunker, SimpleChunker>(); // load chunker once
builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddHttpClient<IOllamaClient, OllamaClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromMinutes(3);
});
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

//map endpoints
app.MapDocumentEndpoints();
app.MapChatEndpoints();

app.Run();