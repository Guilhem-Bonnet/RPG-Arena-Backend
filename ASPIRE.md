# 🚀 Configuration Aspire - RPG-Arena Backend

## 📋 Vue d'ensemble

Le projet utilise **.NET Aspire** pour orchestrer le backend et MongoDB. Aspire simplifie la gestion des microservices, la découverte de services et la configuration distribuée.

## 🏗️ Architecture Aspire

```
RPG-Arena (AppHost)
├── MongoDB Container (avec MongoExpress)
└── RPGArena.Backend (WebSocket Server)
    └── Connection automatique à MongoDB via Aspire
```

## 📦 Packages Aspire installés

### AppHost (RPG-Arena)
- `Aspire.Hosting.AppHost` v9.2.0 - Orchestration
- `Aspire.Hosting.MongoDB` v9.2.0 - Support MongoDB

### Backend (RPGArena.Backend)
- `Aspire.MongoDB.Driver` v9.2.0 - Client MongoDB avec configuration automatique
- `Microsoft.Extensions.ServiceDiscovery` v9.2.0 - Découverte de services

## 🚀 Lancer le projet avec Aspire

### Option 1 : Lancer via l'AppHost (Recommandé)

```bash
cd /path/to/RPG-Arena
dotnet run
```

Ceci va :
1. ✅ Démarrer un container MongoDB (port auto-assigné)
2. ✅ Lancer MongoExpress pour l'interface web (http://localhost:8081)
3. ✅ Démarrer le Backend sur https://localhost:5001
4. ✅ Ouvrir le Dashboard Aspire (http://localhost:15888)

### Option 2 : Lancer le Backend seul (Dev/Debug)

```bash
cd RPGArena.Backend
dotnet run
```

⚠️ **Important** : MongoDB doit être accessible localement sur `mongodb://localhost:27017`

## 🎛️ Dashboard Aspire

Le Dashboard Aspire fournit :
- 📊 Monitoring en temps réel des services
- 📝 Logs centralisés
- 🔍 Traces distribuées
- 📈 Métriques de performance
- 🌐 État des containers

Accès : **http://localhost:15888** (démarré automatiquement avec l'AppHost)

## 🔧 Configuration

### AppHost (Program.cs)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// MongoDB + MongoExpress
var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress()
    .AddDatabase("RPGArena");

// Backend avec référence MongoDB
var backend = builder.AddProject<Projects.RPGArena_Backend>("backend")
    .WithReference(mongodb)
    .WithHttpsEndpoint(port: 5001, name: "websocket");

builder.Build().Run();
```

### Backend (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Connection MongoDB via Aspire (auto-configuré depuis l'AppHost)
builder.AddMongoDBClient("mongodb");

// Le reste de la configuration...
```

## 🔗 Connection Strings

Aspire gère automatiquement les connection strings :

- **Depuis AppHost** : `mongodb` (nom du service)
- **Connection string générée** : Aspire injecte automatiquement la connection string correcte

Pas besoin de configurer manuellement les URLs !

## 🐳 MongoDB avec Aspire

### Avantages

- ✅ Pas de MongoDB local requis
- ✅ Container géré automatiquement
- ✅ MongoExpress inclus pour l'administration
- ✅ Données persistées entre les redémarrages
- ✅ Isolation complète par environnement

### MongoExpress

Interface web : **http://localhost:8081**

Credentials par défaut (configurables dans l'AppHost) :
- Username: `admin`
- Password: `pass`

## 📚 Ressources

- [Documentation Aspire](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire MongoDB Integration](https://learn.microsoft.com/dotnet/aspire/database/mongodb-integration)
- [Aspire Dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard)

## 🛠️ Commandes utiles

```bash
# Lancer l'AppHost avec le Dashboard
dotnet run --project RPG-Arena

# Lancer uniquement le Backend
dotnet run --project RPGArena.Backend

# Rebuilder tout
dotnet build

# Tests
dotnet test

# Voir les packages Aspire installés
dotnet list package | grep Aspire
```

## ✨ Fonctionnalités Aspire activées

- [x] Orchestration des services
- [x] Service Discovery automatique
- [x] Configuration distribuée
- [x] MongoDB containerisé
- [x] MongoExpress UI
- [x] Dashboard de monitoring
- [x] Logs centralisés
- [x] Traces distribuées
- [x] Métriques OpenTelemetry

## 🔄 Migration depuis MongoDB classique

Avant (sans Aspire) :
```csharp
var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("RPGArena");
builder.Services.AddSingleton(database);
```

Après (avec Aspire) :
```csharp
builder.AddMongoDBClient("mongodb");
// Aspire injecte automatiquement IMongoClient
```

## 🎯 Prochaines étapes

- [ ] Ajouter Redis pour le caching (via `Aspire.Hosting.Redis`)
- [ ] Ajouter observabilité avec Application Insights
- [ ] Configurer des environnements multiples (dev/staging/prod)
- [ ] Ajouter tests d'intégration avec Aspire.Testing

---

**Note** : Aspire nécessite Docker pour les containers (MongoDB). Assurez-vous que Docker est installé et en cours d'exécution.
