using System.Text.Json.Serialization;

namespace MZVotosBot.Models;

public class PlayerRankingResponse
{
    [JsonPropertyName("players")]
    public List<Player>? Players { get; set; }

    [JsonPropertyName("data")]
    public List<Player>? Data { get; set; }
}

public class Player
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("playername")]
    public string? Playername { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("votes")]
    public int Votes { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    public string DisplayName => Playername ?? Name ?? "Desconocido";
}
