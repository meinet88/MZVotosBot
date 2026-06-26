using System.Net;
using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace MZVotosBot.Services;

/// <summary>
/// Escucha en el puerto configurado los webhooks que envía top-games.net
/// cada vez que un jugador vota. Publica un mensaje en el canal de votos.
/// </summary>
public class VoteWebhookListener : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private HttpListener? _listener;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VoteWebhookListener(DiscordSocketClient client)
    {
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{BotConfig.WebhookPort}/vote/");

        try
        {
            _listener.Start();
            Console.WriteLine($"[Webhook] Escuchando votos en el puerto {BotConfig.WebhookPort}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Webhook] No se pudo iniciar el servidor HTTP: {ex.Message}");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync().WaitAsync(stoppingToken);
                _ = Task.Run(() => HandleRequestAsync(context), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Webhook] Error en listener: {ex.Message}");
            }
        }

        _listener.Stop();
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream);
            var body = await reader.ReadToEndAsync();

            context.Response.StatusCode = 200;
            context.Response.Close();

            var vote = JsonSerializer.Deserialize<VotePayload>(body, JsonOptions);
            if (vote is null || string.IsNullOrWhiteSpace(vote.Playername))
            {
                Console.WriteLine($"[Webhook] Payload no reconocido: {body}");
                return;
            }

            Console.WriteLine($"[Webhook] Voto recibido de {vote.Playername}");
            await NotifyVoteAsync(vote.Playername, vote.Date ?? DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Webhook] Error procesando voto: {ex.Message}");
        }
    }

    private async Task NotifyVoteAsync(string playerName, DateTime date)
    {
        if (_client.GetChannel(BotConfig.VotesChannelId) is not IMessageChannel channel)
        {
            Console.WriteLine("[Webhook] Canal de votos no encontrado. Comprueba VotesChannelId.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("🗳️ ¡Nuevo voto!")
            .WithDescription(
                $"**{playerName}** acaba de votar por el servidor en [top-games.net](https://top-games.net).\n\n" +
                $"¡Gracias por apoyar a la comunidad! 💚\n" +
                $"Vota cada día para subir en el ranking 🏆")
            .WithColor(new Color(34, 197, 94))
            .WithFooter($"{date:dd/MM/yyyy HH:mm} UTC")
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }

    private class VotePayload
    {
        public string? Playername { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
    }
}

