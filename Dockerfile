# ── Etapa 1: compilar ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar proyecto y restaurar dependencias
COPY MZVotosBot.csproj .
RUN dotnet restore

# Copiar el resto del código y publicar
COPY . .
RUN dotnet publish -c Release -o /app

# ── Etapa 2: imagen final (más ligera, sin SDK) ───────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "MZVotosBot.dll"]
