using MZVotosBot.Models;
using Discord;
using MZVotosBot;
using System.Numerics;

namespace MZVotosBot.Services;

public static class RankingEmbedBuilder
{
    private static readonly Dictionary<int, string> Medals = new()
    {
        { 1, "🥇" },
        { 2, "🥈" },
        { 3, "🥉" },
    };

    public static Embed BuildRankingEmbed(List<Player> players)
    {
        var builder = new Discord.EmbedBuilder()
            .WithTitle($"🏆 Ranking de Votantes — {BotConfig.ServerName}")
            .WithDescription(
                "Vota cada día en [top-games.net](https://es.top-games.net/dayz/vote/montepinarz-pve-with-pvp-zones-traders-quests) " +
                "para aparecer en esta tabla.\n" +
                "¡Cada voto ayuda al servidor a crecer! 💚\n\u200b")
            .WithColor(new Color(34, 197, 94))   // verde DayZ
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter($"Actualizado • {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC");

        if (players.Count == 0)
        {
            builder.AddField("Sin datos", "Aún no hay votantes registrados.");
            return builder.Build();
        }

        var top = players.Take(25).ToList();

        // ── Top 3 destacado con bloque de código simulando recuadro ──────────
        var p1 = top.Count > 0 ? top[0] : null;
        var p2 = top.Count > 1 ? top[1] : null;
        var p3 = top.Count > 2 ? top[2] : null;

        string podiumText = "```\n";
        if (p1 != null) podiumText += $"  🏆  1º  {p1.DisplayName,-20} {p1.Votes} votos\n";
        if (p2 != null) podiumText += $"  🥈  2º  {p2.DisplayName,-20} {p2.Votes} votos\n";
        if (p3 != null) podiumText += $"  🥉  3º  {p3.DisplayName,-20} {p3.Votes} votos\n";
        podiumText += "```";

        builder.AddField("👑  P O D I O", podiumText, inline: false);

        if (top.Count > 3)
        {
            builder.AddField("\u200b", "─────────────────", inline: false);

            string restText = "```\n";
            for (int i = 3; i < top.Count; i++)
            {
                var p = top[i];
                int pos = p.Position > 0 ? p.Position : i + 1;
                restText += $"  #{pos,-4} {p.DisplayName,-20} {p.Votes} votos\n";
            }
            restText += "```";

            builder.AddField("📋  C L A S I F I C A C I Ó N", restText, inline: false);
        }

        return builder.Build();
    }

    public static Embed BuildPlayerEmbed(Player p, int pos)
    {
        string medal = Medals.TryGetValue(pos, out var m) ? m : $"#{pos}";
        return new Discord.EmbedBuilder()
            .WithTitle($"Estadísticas de {p.DisplayName}")
            .WithColor(new Color(0x5865F2))
            .AddField("Posición", medal, inline: true)
            .AddField("Votos", p.Votes.ToString(), inline: true)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }

    public static Embed BuildErrorEmbed(string message) =>
        new Discord.EmbedBuilder()
            .WithTitle("❌ Error")
            .WithDescription(message)
            .WithColor(Color.Red)
            .Build();
}
