# ═══════════════════════════════════════════════════════════════
# Dockerfile - Tests d'Intégration RPG Arena
# Image complète avec .NET 9 + MongoDB tools + curl
# ═══════════════════════════════════════════════════════════════

FROM mcr.microsoft.com/dotnet/sdk:9.0

# ───────────────────────────────────────────────────────────────
# Installation des outils nécessaires
# ───────────────────────────────────────────────────────────────

RUN apt-get update && apt-get install -y \
    curl \
    wget \
    gnupg \
    && rm -rf /var/lib/apt/lists/*

# Installation MongoDB tools (mongosh)
RUN curl -fsSL https://www.mongodb.org/static/pgp/server-8.0.asc | \
    gpg --dearmor -o /usr/share/keyrings/mongodb-server-8.0.gpg && \
    echo "deb [ signed-by=/usr/share/keyrings/mongodb-server-8.0.gpg ] http://repo.mongodb.org/apt/debian bookworm/mongodb-org/8.0 main" | \
    tee /etc/apt/sources.list.d/mongodb-org-8.0.list && \
    apt-get update && \
    apt-get install -y mongodb-mongosh && \
    rm -rf /var/lib/apt/lists/*

# ───────────────────────────────────────────────────────────────
# Configuration du workspace
# ───────────────────────────────────────────────────────────────

WORKDIR /workspace

# Copier les fichiers de projet pour la restauration des packages
COPY *.sln ./
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do \
    dir=$(echo $file | sed 's/.csproj//'); \
    mkdir -p $dir; \
    mv $file $dir/; \
    done

# Restaurer les dépendances
RUN dotnet restore

# Copier tout le code source
COPY . .

# ───────────────────────────────────────────────────────────────
# Configuration des scripts de tests
# ───────────────────────────────────────────────────────────────

# Rendre les scripts exécutables
RUN chmod +x scripts/*.sh

# ───────────────────────────────────────────────────────────────
# Entrypoint
# ───────────────────────────────────────────────────────────────

# Par défaut, lance les tests d'intégration
CMD ["bash", "/workspace/scripts/test-integration.sh"]
