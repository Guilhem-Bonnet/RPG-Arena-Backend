# ═══════════════════════════════════════════════════════════════
# Dockerfile - RPG Arena Backend
# Image de production pour le WebSocket Backend
# ═══════════════════════════════════════════════════════════════

# ───────────────────────────────────────────────────────────────
# Stage 1: Build
# ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier les fichiers de solution et projets
COPY *.sln ./
COPY RPGArena.Backend/*.csproj ./RPGArena.Backend/
COPY RPGArena.CombatEngine/*.csproj ./RPGArena.CombatEngine/

# Restaurer les dépendances
RUN dotnet restore RPGArena.Backend/RPGArena.Backend.csproj

# Copier tout le code source
COPY RPGArena.Backend/ ./RPGArena.Backend/
COPY RPGArena.CombatEngine/ ./RPGArena.CombatEngine/

# Build du projet
WORKDIR /src/RPGArena.Backend
RUN dotnet build -c Release -o /app/build

# ───────────────────────────────────────────────────────────────
# Stage 2: Publish
# ───────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ───────────────────────────────────────────────────────────────
# Stage 3: Runtime
# ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Créer un utilisateur non-root pour la sécurité
RUN groupadd -r rpgarena && useradd -r -g rpgarena rpgarena
RUN chown -R rpgarena:rpgarena /app

# Copier les fichiers publiés
COPY --from=publish /app/publish .

# Configuration des variables d'environnement
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Exposer les ports
EXPOSE 5000
EXPOSE 5001

# Utiliser l'utilisateur non-root
USER rpgarena

# Point d'entrée
ENTRYPOINT ["dotnet", "RPGArena.Backend.dll"]
