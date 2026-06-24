using MZVotosBot;
using MZVotosBot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Cliente de Discord
        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.Guilds
        }));

        // HttpClient para la API de top-games.net
        services.AddHttpClient<TopGamesService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "DayZVoteBot/1.0");
        });

        services.AddSingleton<SlashCommandHandler>();

        // Tarea periódica que publica el ranking cada N horas
        services.AddHostedService<RankingScheduler>();
    })
    .Build();

var client = host.Services.GetRequiredService<DiscordSocketClient>();
var handler = host.Services.GetRequiredService<SlashCommandHandler>();

// Logging de Discord.Net a consola
client.Log += msg =>
{
    Console.WriteLine($"[Discord] {msg.Severity,-8} {msg.Source,-12} {msg.Message}");
    return Task.CompletedTask;
};

// Cuando el bot esté listo: registrar comandos slash
client.Ready += async () =>
{
    Console.WriteLine($"[Bot] Conectado como {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
    await handler.RegisterCommandsAsync();
};

await client.LoginAsync(TokenType.Bot, BotConfig.DiscordToken);
await client.StartAsync();

await host.RunAsync();

