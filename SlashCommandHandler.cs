using Discord;
using Discord.WebSocket;
using MZVotosBot.Services;

namespace MZVotosBot;

public class SlashCommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly TopGamesService _topGames;

    public SlashCommandHandler(DiscordSocketClient client, TopGamesService topGames)
    {
        _client = client;
        _topGames = topGames;
        _client.SlashCommandExecuted += HandleSlashCommand;
    }

    public async Task RegisterCommandsAsync()
    {
        var ranking = new SlashCommandBuilder()
            .WithName("ranking")
            .WithDescription("Muestra el ranking de votantes del servidor DayZ")
            .Build();

        var mivoto = new SlashCommandBuilder()
            .WithName("mivoto")
            .WithDescription("Busca tu posición en el ranking de votantes")
            .AddOption("jugador", ApplicationCommandOptionType.String,
                       "Nombre del jugador a buscar", isRequired: true)
            .Build();

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(ranking);
            await _client.CreateGlobalApplicationCommandAsync(mivoto);
            Console.WriteLine("[Commands] Comandos slash registrados.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Commands] Error al registrar comandos: {ex.Message}");
        }
    }

    private async Task HandleSlashCommand(SocketSlashCommand cmd)
    {
        switch (cmd.CommandName)
        {
            case "ranking":
                await HandleRanking(cmd);
                break;
            case "mivoto":
                await HandleMiVoto(cmd);
                break;
        }
    }

    // ── /ranking ──────────────────────────────────────────────────────────────

    private async Task HandleRanking(SocketSlashCommand cmd)
    {
        await cmd.DeferAsync();

        var players = await _topGames.GetRankingAsync();
        if (players is null)
        {
            await cmd.FollowupAsync(embed: RankingEmbedBuilder.BuildErrorEmbed(
                "No se pudo obtener el ranking. Inténtalo más tarde."));
            return;
        }

        await cmd.FollowupAsync(embed: RankingEmbedBuilder.BuildRankingEmbed(players));
    }

    // ── /mivoto ───────────────────────────────────────────────────────────────

    private async Task HandleMiVoto(SocketSlashCommand cmd)
    {
        await cmd.DeferAsync();

        var query = cmd.Data.Options.FirstOrDefault()?.Value?.ToString() ?? "";
        var players = await _topGames.GetRankingAsync();

        if (players is null)
        {
            await cmd.FollowupAsync(embed: RankingEmbedBuilder.BuildErrorEmbed(
                "No se pudo obtener el ranking."));
            return;
        }

        var match = players.FirstOrDefault(p =>
            p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            await cmd.FollowupAsync(embed: RankingEmbedBuilder.BuildErrorEmbed(
                $"No se encontró al jugador **{query}** en el ranking."));
            return;
        }

        int pos = match.Position > 0 ? match.Position : players.IndexOf(match) + 1;
        await cmd.FollowupAsync(embed: RankingEmbedBuilder.BuildPlayerEmbed(match, pos));
    }
}

