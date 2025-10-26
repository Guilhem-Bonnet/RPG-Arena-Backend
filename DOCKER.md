# 🐳 Docker Setup - RPG Arena Backend

## 📋 Vue d'ensemble

Configuration Docker complète avec MongoDB, MongoExpress, backups automatisés et intégration Aspire.

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Aspire AppHost                         │
│              (Orchestration .NET)                       │
└────────────┬────────────────────────────────────────────┘
             │
             ├──> 🐳 Docker Compose
             │    ├── MongoDB (27017)
             │    │   ├─ Persistance: mongodb_data
             │    │   ├─ Config: mongod.conf
             │    │   ├─ Init: 01-init-database.js
             │    │   └─ Health checks
             │    │
             │    └── MongoExpress (8081)
             │        └─ Interface admin web
             │
             └──> 🌐 Backend (.NET 9.0)
                  ├─ WebSocket Server (5001)
                  └─ Connection MongoDB via Aspire
```

## 🚀 Démarrage rapide

### Option 1 : Tout-en-un (Recommandé)

```bash
# Lance MongoDB + MongoExpress + Backend
./scripts/start.sh

# Puis dans un autre terminal
dotnet run --project RPG-Arena
```

### Option 2 : Étape par étape

```bash
# 1. Démarrer MongoDB et MongoExpress
docker compose up -d

# 2. Lancer le Backend
dotnet run --project RPG-Arena
```

## 📦 Services disponibles

| Service | URL | Credentials |
|---------|-----|-------------|
| **MongoDB** | `mongodb://localhost:27017` | `rpgarena_user` / `rpgarena_pass` |
| **MongoExpress** | http://localhost:8081 | `admin` / `pass` |
| **Backend WebSocket** | https://localhost:5001 | N/A |
| **Aspire Dashboard** | http://localhost:15888 | N/A |

## 🗄️ MongoDB Configuration

### Credentials

```yaml
# Admin (root)
Username: admin
Password: admin123
Database: admin

# Application User (recommended)
Username: rpgarena_user
Password: rpgarena_pass
Database: RPGArena
AuthSource: RPGArena
```

### Connection Strings

```bash
# Pour l'application (.NET)
mongodb://rpgarena_user:rpgarena_pass@localhost:27017/RPGArena?authSource=RPGArena

# Admin (root)
mongodb://admin:admin123@localhost:27017/RPGArena?authSource=admin

# MongoExpress
mongodb://admin:admin123@mongodb:27017
```

## 📂 Structure des fichiers

```
RPG-Arena/
├── docker-compose.yml              # Configuration Docker
├── .env                           # Variables d'environnement
├── .env.example                   # Template
│
├── docker/
│   └── mongodb/
│       ├── mongod.conf            # Config MongoDB
│       ├── init-scripts/
│       │   └── 01-init-database.js  # Init DB + données test
│       └── backup-scripts/
│           ├── backup.sh          # Script backup
│           └── restore.sh         # Script restore
│
├── scripts/
│   ├── start.sh                   # 🚀 Démarrage complet
│   ├── stop.sh                    # 🛑 Arrêt propre
│   ├── backup.sh                  # 💾 Créer backup
│   └── restore.sh                 # 🔄 Restaurer backup
│
└── backups/                       # Backups MongoDB (auto-créé)
    └── rpgarena_backup_*.tar.gz
```

## 🔧 Scripts disponibles

### `./scripts/start.sh`
Lance MongoDB, MongoExpress avec vérifications de santé.

**Fonctionnalités :**
- ✅ Vérifie Docker
- ✅ Crée `.env` si absent
- ✅ Démarre les containers
- ✅ Attend que MongoDB soit prêt
- ✅ Affiche les logs d'init
- ✅ Vérifie MongoExpress

### `./scripts/stop.sh`
Arrête proprement tous les containers.

### `./scripts/backup.sh`
Crée un backup complet de MongoDB.

**Format :** `rpgarena_backup_YYYYMMDD_HHMMSS.tar.gz`

**Rétention :** 7 jours (configurable)

**Contenu :**
- Toutes les collections
- Indexes
- Compression gzip

### `./scripts/restore.sh`
Restaure un backup avec menu interactif.

**⚠️ Attention :** Écrase les données existantes !

## 💾 Backups

### Créer un backup manuel

```bash
./scripts/backup.sh
```

### Backup automatique (cron)

```bash
# Ajouter au crontab
# Backup tous les jours à 2h du matin
0 2 * * * cd /path/to/RPG-Arena && ./scripts/backup.sh >> /var/log/rpgarena-backup.log 2>&1
```

### Restaurer un backup

```bash
./scripts/restore.sh
# Sélectionner le backup dans le menu interactif
```

### Lister les backups

```bash
ls -lh backups/
```

## 🔍 Monitoring & Logs

### Logs en temps réel

```bash
# Tous les services
docker compose logs -f

# MongoDB uniquement
docker compose logs -f mongodb

# MongoExpress uniquement
docker compose logs -f mongo-express
```

### État des services

```bash
docker compose ps
```

### Health check MongoDB

```bash
docker compose exec mongodb mongosh --eval "db.adminCommand('ping')"
```

## 🗃️ Initialisation de la base

Au premier démarrage, le script `01-init-database.js` :

1. ✅ Crée l'utilisateur `rpgarena_user`
2. ✅ Crée les collections :
   - `combats` (avec validation de schéma)
   - `combat_logs` (TimeSeries)
   - `statistics`
3. ✅ Crée les index optimisés
4. ✅ Insert des données de test (2 combats)
5. ✅ Initialise les statistiques

### Réinitialiser la base

```bash
# Supprimer les volumes
docker compose down -v

# Redémarrer (réexécute init-scripts)
docker compose up -d
```

## 🎛️ Configuration avancée

### Variables d'environnement (.env)

```bash
# MongoDB
MONGO_INITDB_ROOT_USERNAME=admin
MONGO_INITDB_ROOT_PASSWORD=admin123
MONGO_APP_USERNAME=rpgarena_user
MONGO_APP_PASSWORD=rpgarena_pass

# MongoExpress
ME_CONFIG_BASICAUTH_USERNAME=admin
ME_CONFIG_BASICAUTH_PASSWORD=pass

# Backup
BACKUP_RETENTION_DAYS=7

# Application
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL=Information
```

### Personnaliser MongoDB

Éditer `docker/mongodb/mongod.conf` :

```yaml
storage:
  wiredTiger:
    engineConfig:
      cacheSizeGB: 2  # Augmenter le cache
```

## 🐳 Commandes Docker utiles

```bash
# Démarrer
docker compose up -d

# Arrêter
docker compose down

# Redémarrer un service
docker compose restart mongodb

# Reconstruire les images
docker compose build

# Supprimer tout (⚠️ données incluses)
docker compose down -v

# Exécuter une commande dans MongoDB
docker compose exec mongodb mongosh

# Shell dans le container MongoDB
docker compose exec mongodb bash

# Voir l'utilisation des ressources
docker stats
```

## 🔗 Intégration Aspire

L'AppHost détecte automatiquement Docker Compose :

```csharp
var useDockerCompose = File.Exists("docker-compose.yml");

if (useDockerCompose) {
    // Utilise MongoDB depuis Docker Compose
    mongodb = builder.AddConnectionString("mongodb", 
        "mongodb://rpgarena_user:rpgarena_pass@localhost:27017/RPGArena");
} else {
    // Crée un container MongoDB via Aspire
    mongodb = builder.AddMongoDB("mongodb")
        .WithMongoExpress()
        .AddDatabase("RPGArena");
}
```

### Forcer l'utilisation de Docker Compose

```bash
export USE_DOCKER_COMPOSE=true
dotnet run --project RPG-Arena
```

## 🚨 Troubleshooting

### MongoDB ne démarre pas

```bash
# Vérifier les logs
docker compose logs mongodb

# Vérifier les permissions des volumes
ls -la ~/.local/share/docker/volumes/

# Supprimer et recréer les volumes
docker compose down -v
docker compose up -d
```

### MongoExpress ne se connecte pas

```bash
# Vérifier que MongoDB est prêt
docker compose exec mongodb mongosh --eval "db.adminCommand('ping')"

# Redémarrer MongoExpress
docker compose restart mongo-express
```

### Erreur "port 27017 already in use"

```bash
# Trouver le processus utilisant le port
lsof -i :27017

# Arrêter le service MongoDB local
sudo systemctl stop mongod
```

### Backup échoue

```bash
# Vérifier l'espace disque
df -h

# Vérifier les permissions
ls -la backups/

# Exécuter manuellement
docker compose --profile backup run --rm mongodb-backup
```

## 📊 Performances

### Recommandations

- **RAM** : Minimum 2GB pour MongoDB
- **CPU** : 2 cores recommandés
- **Disque** : SSD recommandé pour les performances

### Optimisation

```yaml
# docker-compose.yml
services:
  mongodb:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          memory: 1G
```

## 🔐 Sécurité

### Production

**⚠️ NE PAS utiliser les credentials par défaut en production !**

```bash
# Générer des mots de passe forts
openssl rand -base64 32

# Mettre à jour .env
MONGO_INITDB_ROOT_PASSWORD=<password-fort>
MONGO_APP_PASSWORD=<password-fort>
```

### Réseau

En production, ne pas exposer MongoDB sur 0.0.0.0 :

```yaml
# docker-compose.yml
services:
  mongodb:
    ports:
      - "127.0.0.1:27017:27017"  # Seulement localhost
```

## 📚 Ressources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [MongoDB Docker Image](https://hub.docker.com/_/mongo)
- [MongoExpress](https://github.com/mongo-express/mongo-express)
- [MongoDB Configuration](https://www.mongodb.com/docs/manual/reference/configuration-options/)

---

**🎉 Configuration Docker terminée ! Votre environnement est prêt pour le développement.**
