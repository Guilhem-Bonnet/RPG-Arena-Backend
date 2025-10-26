# 🎮 RPG Arena Backend - Guide de Démarrage Rapide

## 🚀 Démarrage en 3 étapes

### 1️⃣ Installer .NET 9.0

```bash
# Vérifier la version
dotnet --version  # Doit être 9.0.x
```

Si pas installé : https://dotnet.microsoft.com/download/dotnet/9.0

### 2️⃣ Démarrer MongoDB

```bash
# Option A : Via Docker (recommandé)
./scripts/start.sh

# Option B : MongoDB local
# Connection string: mongodb://localhost:27017
```

### 3️⃣ Lancer l'application

```bash
# Démarrer l'AppHost Aspire (Dashboard + Backend)
dotnet run --project RPG-Arena

# Ou backend seul
dotnet run --project RPGArena.Backend
```

## 🌐 Accès rapide

| Service | URL | Login |
|---------|-----|-------|
| **Backend WebSocket** | https://localhost:5001 | - |
| **Aspire Dashboard** | http://localhost:15888 | - |
| **MongoExpress** | http://localhost:8081 | admin / pass |
| **MongoDB** | mongodb://localhost:27017 | rpgarena_user / rpgarena_pass |

## 📚 Documentation complète

- 📘 [**ASPIRE.md**](ASPIRE.md) - Configuration .NET Aspire
- 🐳 [**DOCKER.md**](DOCKER.md) - Setup Docker & MongoDB
- 📊 [**ANALYSE_PROJET.md**](ANALYSE_PROJET.md) - Architecture détaillée

## 🔧 Commandes utiles

```bash
# Tests
dotnet test

# Build
dotnet build

# Clean
dotnet clean

# Backup MongoDB
./scripts/backup.sh

# Arrêter tout
./scripts/stop.sh
```

## 📖 Plus d'informations

Voir [DOCKER.md](DOCKER.md) pour la documentation Docker complète.
