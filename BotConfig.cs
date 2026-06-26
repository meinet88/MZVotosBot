namespace MZVotosBot
{
    public static class BotConfig
    {
        // Los valores se leen de variables de entorno (configuradas en Railway)
        // Si no existe la variable, usa el valor por defecto indicado.

        /// <summary>Token del bot de Discord</summary>
        public static string DiscordToken =>
            Environment.GetEnvironmentVariable("DISCORD_TOKEN")
            ?? throw new Exception("Falta la variable de entorno DISCORD_TOKEN");

        /// <summary>Token de tu servidor en top-games.net</summary>
        public static string TopGamesToken =>
            Environment.GetEnvironmentVariable("TOPGAMES_TOKEN")
            ?? throw new Exception("Falta la variable de entorno TOPGAMES_TOKEN");

        /// <summary>ID del canal donde se publicará el ranking automático</summary>
        public static ulong RankingChannelId =>
            ulong.Parse(Environment.GetEnvironmentVariable("RANKING_CHANNEL_ID")
            ?? throw new Exception("Falta la variable de entorno RANKING_CHANNEL_ID"));

        /// <summary>ID del canal donde se anunciarán los votos en tiempo real</summary>
        public static ulong VotesChannelId =>
            ulong.Parse(Environment.GetEnvironmentVariable("VOTES_CHANNEL_ID")
            ?? throw new Exception("Falta la variable de entorno VOTES_CHANNEL_ID"));

        /// <summary>Nombre del servidor que aparece en el embed</summary>
        public static string ServerName =>
            Environment.GetEnvironmentVariable("SERVER_NAME") ?? "MontepinarZ";

        /// <summary>Intervalo de actualización automática en horas</summary>
        public static int UpdateIntervalHours =>
            int.TryParse(Environment.GetEnvironmentVariable("UPDATE_INTERVAL_HOURS"), out var h) ? h : 2;

        /// <summary>Puerto en el que escucha el webhook</summary>
        public static int WebhookPort =>
            int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var p) ? p : 8080;

        // ─────────────────────────────────────────────
        //  No tocar
        // ─────────────────────────────────────────────
        public const string ApiBaseUrl = "https://api.top-games.net/v1/servers";
    }
}
