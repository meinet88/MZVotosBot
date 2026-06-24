using MZVotosBot.Models;
using System.Text.Json;

namespace MZVotosBot.Services;

public class TopGamesService
{
    private readonly HttpClient _http;
    private readonly string _apiUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TopGamesService(HttpClient http)
    {
        _http = http;
        _apiUrl = $"{BotConfig.ApiBaseUrl}/{BotConfig.TopGamesToken}/players-ranking";
    }

    public async Task<List<Player>?> GetRankingAsync()
    {
        try
        {
            var response = await _http.GetAsync(_apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ERROR] API devolvió {(int)response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            // La API puede devolver un array directo o un objeto con campo "players"/"data"
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<Player>>(json, JsonOptions);
            }

            var wrapper = JsonSerializer.Deserialize<PlayerRankingResponse>(json, JsonOptions);
            return wrapper?.Players ?? wrapper?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] TopGamesService: {ex.Message}");
            return null;
        }
    }
}

