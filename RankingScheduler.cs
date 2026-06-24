using MZVotosBot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace MZVotosBot;

/// <summary>
/// Servicio en background que actualiza el ranking en el canal configurado
/// cada <see cref="BotConfig.UpdateIntervalHours"/> horas.
/// Edita el mensaje anterior en lugar de publicar uno nuevo cada vez.
/// </summary>
public class RankingScheduler : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly TopGamesService _topGames;
    private ulong _lastMessageId;

    public RankingScheduler(DiscordSocketClient client, TopGamesService topGames)
    {
        _client = client;
        _topGames = topGames;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Espera a que el bot esté listo antes de empezar
        while (_client.ConnectionState != ConnectionState.Connected && !stoppingToken.IsCancellationRequested)
            await Task.Delay(500, stoppingToken);

        Console.WriteLine($"[Scheduler] Iniciado. Intervalo: {BotConfig.UpdateIntervalHours}h");

        // Primera publicación inmediata al arrancar
        await PostOrUpdateRanking();

        using var timer = new PeriodicTimer(TimeSpan.FromHours(BotConfig.UpdateIntervalHours));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PostOrUpdateRanking();
        }
    }

    private async Task PostOrUpdateRanking()
    {
        try
        {
            if (_client.GetChannel(BotConfig.RankingChannelId) is not IMessageChannel channel)
            {
                Console.WriteLine("[Scheduler] Canal no encontrado. Comprueba RankingChannelId.");
                return;
            }

            var players = await _topGames.GetRankingAsync();
            if (players is null)
            {
                Console.WriteLine("[Scheduler] No se pudo obtener el ranking.");
                return;
            }

            var embed = RankingEmbedBuilder.BuildRankingEmbed(players);

            // Intenta editar el mensaje anterior
            if (_lastMessageId != 0)
            {
                try
                {
                    if (await channel.GetMessageAsync(_lastMessageId) is IUserMessage existing)
                    {
                        await existing.ModifyAsync(m => m.Embed = embed);
                        Console.WriteLine($"[Scheduler] Ranking actualizado — {DateTime.UtcNow:HH:mm} UTC");
                        return;
                    }
                }
                catch
                {
                    _lastMessageId = 0; // mensaje borrado, publicar uno nuevo
                }
            }

            var msg = await channel.SendMessageAsync(embed: embed);
            _lastMessageId = msg.Id;
            Console.WriteLine($"[Scheduler] Ranking publicado (id={msg.Id}) — {DateTime.UtcNow:HH:mm} UTC");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Scheduler] Error inesperado: {ex.Message}");
        }
    }
}
