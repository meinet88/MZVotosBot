# 🧟 DayZVoteBot — C# / Discord.Net

Bot de Discord en C# que publica automáticamente cada 2 horas el ranking de votantes
de tu servidor DayZ registrado en [top-games.net](https://top-games.net).

---

## 📋 Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Cuenta en [Discord Developer Portal](https://discord.com/developers/applications)
- Servidor registrado en [top-games.net](https://top-games.net)

---

## ⚙️ Configuración

### 1. Crea el bot en Discord

1. Ve a https://discord.com/developers/applications → **New Application**
2. Sección **Bot** → copia el **Token**
3. En **Privileged Gateway Intents** activa: `SERVER MEMBERS INTENT` (opcional, no es necesario para este bot)
4. En **OAuth2 → URL Generator**, selecciona los scopes:
   - `bot`
   - `applications.commands`
5. Permisos de bot: `Send Messages`, `Embed Links`, `Read Message History`
6. Usa la URL generada para invitar el bot a tu servidor de Discord

### 2. Obtén tu token de top-games.net

1. Entra en https://top-games.net con tu cuenta
2. Ve al panel de gestión de tu servidor DayZ
3. Busca la sección **API** o **Voting plugin** → copia el **Token**

### 3. Edita `BotConfig.cs`

```csharp
public const string DiscordToken    = "TU_TOKEN_DE_DISCORD";
public const string TopGamesToken   = "TU_TOKEN_DE_TOP_GAMES";
public const ulong  RankingChannelId = 123456789012345678;   // ID del canal
public const string ServerName      = "Mi Servidor DayZ";
public const int    UpdateIntervalHours = 2;                 // cada 2 horas
```

> **¿Cómo obtener el ID del canal?**
> Activa el Modo desarrollador en Discord (Ajustes → Avanzado → Modo desarrollador),
> luego haz clic derecho en el canal → **Copiar ID del canal**.

---

## 🚀 Compilar y ejecutar

```bash
# Desde la carpeta del proyecto
dotnet restore
dotnet run
```

Para publicar un ejecutable:

```bash
dotnet publish -c Release -r win-x64 --self-contained
# El ejecutable estará en: bin/Release/net8.0/win-x64/publish/
```

---

## 🤖 Comandos disponibles

| Comando | Descripción |
|---------|-------------|
| `/ranking` | Muestra el ranking completo de votantes |
| `/mivoto <jugador>` | Busca a un jugador y muestra su posición y votos |

> Los comandos slash pueden tardar hasta 1 hora en aparecer la primera vez
> (propagación global de Discord). Para pruebas más rápidas puedes registrarlos
> a nivel de guild en lugar de global.

---

## 🔄 Actualización automática

Al arrancar el bot publica el ranking inmediatamente y luego cada 2 horas.
En lugar de enviar un mensaje nuevo, **edita el mensaje anterior**,
así el canal no se llena de mensajes repetidos.

Cambia el intervalo en `BotConfig.cs`:

```csharp
public const int UpdateIntervalHours = 2; // ← ajusta aquí
```

---

## 📁 Estructura del proyecto

```
DayZVoteBot/
├── BotConfig.cs              # Variables de configuración
├── Program.cs                # Punto de entrada
├── RankingScheduler.cs       # Tarea periódica (cada 2 h)
├── SlashCommandHandler.cs    # Comandos /ranking y /mivoto
├── Models/
│   └── Player.cs             # Modelos de la API
├── Services/
│   ├── TopGamesService.cs    # Cliente de la API top-games.net
│   └── EmbedBuilderService.cs# Construcción de embeds de Discord
└── DayZVoteBot.csproj
```
