using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using RagApi.Core.Options;

namespace RagApi.Services.Chat;
public sealed class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _opt;

    public OllamaClient(HttpClient http, IOptions<OllamaOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
    {
        var payload = new
        {
            model = _opt.Model,
            prompt = prompt,
            stream = false,
            options = new
            {
                num_predict = 180,   // adjust for faster responses 
                temperature = 0.2
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync(ct);

        // Ollama returns JSON: { response: "...", ... }
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("response", out var r) ? (r.GetString() ?? "") : "";
    }
}
