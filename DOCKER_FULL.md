# 🐳 Architecture Docker Complète - RPG Arena

## 📋 Vue d'Ensemble

L'application RPG Arena est entièrement containerisée avec Docker Compose, offrant un environnement **isolé**, **reproductible** et **production-ready**.

## 🏗️ Stack Technique

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│  🎮 Clients (Godot, WebSocket)                     │
│                     │                               │
└─────────────────────┼───────────────────────────────┘
                      │
                      ▼
        ┌─────────────────────────────┐
        │  🚀 Backend (.NET 9)        │
        │  - WebSocket Server         │
        │  - Combat Engine            │
        │  - Health Check             │
        │  Port: 5000 (HTTP)          │
        │  Port: 5443 (HTTPS)         │
        └──────────┬──────────────────┘
                   │
                   ▼
        ┌─────────────────────────────┐
        │  🗄️ MongoDB 8.0              │
        │  - WiredTiger Engine        │
        │  - Authentication           │
        │  - Persistent Volumes       │
        │  Port: 27017 (interne)      │
        └──────────┬──────────────────┘
                   │
      ┌────────────┴────────────┐
      │                         │
      ▼                         ▼
┌──────────────┐      ┌──────────────────┐
│ MongoExpress │      │  Backup Service  │
│ (dev only)   │      │  - Automated     │
│ Port: 8081   │      │  - Retention     │
└──────────────┘      └──────────────────┘
```

## 📦 Services Docker

### 1. **backend** - Application principale
- **Image**: Build depuis `Dockerfile` (multi-stage)
- **Ports**: 
  - `5000` (HTTP)
  - `5443` (HTTPS - mapped from container port 5001)
- **Dépendances**: MongoDB (health check)
- **Variables**:
  - `ConnectionStrings__mongodb`: Connection automatique
  - `ASPNETCORE_ENVIRONMENT`: Development/Production
- **Health Check**: `GET /health` toutes les 30s
- **Note HTTPS**: Port host 5443 utilisé pour éviter conflits avec le port 5001 système

### 2. **mongodb** - Base de données
- **Image**: `mongo:8.0`
- **Port**: 27017 (interne uniquement en production)
- **Volumes**:
  - `mongodb_data`: Données persistantes
  - `mongodb_config`: Configuration
  - Init scripts: `/docker-entrypoint-initdb.d`
- **Features**:
  - Authentification activée
  - Collections avec validation JSON
  - Index automatiques
  - Health check avec `mongosh`

### 3. **mongo-express** - Interface d'administration
- **Image**: `mongo-express`
- **Port**: 8081
- **Mode**: Development seulement (production via profile)
- **Credentials**: admin / pass

### 4. **integration-tests** - Tests (profile: test)
- **Build**: Image custom avec .NET 9 + MongoDB tools
- **Dépendances**: MongoDB + MongoExpress
- **Usage**: `docker compose --profile test run integration-tests`

### 5. **unit-tests** - Tests unitaires (profile: test)
- **Build**: Même image que integration-tests
- **Usage**: `docker compose --profile test run unit-tests`

### 6. **backup-cron** - Backup automatique (production)
- **Image**: `mongo:8.0`
- **Schedule**: Tous les jours à 2h du matin
- **Retention**: 7 jours
- **Volume**: `./backups`

## 🚀 Utilisation

### Démarrage Développement

```bash
# Démarrage complet avec hot-reload
./scripts/start.sh dev

# OU manuellement
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build
```

**Services disponibles**:
- Backend HTTP: http://localhost:5000
- Backend HTTPS: https://localhost:5443 (certificat auto-signé en dev)
- WebSocket: ws://localhost:5000/ws
- MongoDB: mongodb://localhost:27017
- MongoExpress: http://localhost:8081

### Démarrage Production

```bash
# Démarrage optimisé pour la production
./scripts/start.sh prod

# OU manuellement
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

**Différences en production**:
- ✅ Backend optimisé (stage `final`)
- ✅ MongoDB non exposé sur l'hôte
- ✅ MongoExpress désactivé par défaut
- ✅ Backups automatiques via cron
- ✅ Limites de ressources (CPU/RAM)
- ✅ Health checks stricts
- ✅ Restart policy: `always`

### Arrêt

```bash
# Arrêt propre
./scripts/stop.sh

# OU
docker compose down

# Avec suppression des volumes
docker compose down -v
```

## 🔧 Configuration

### Variables d'Environnement

Créer un fichier `.env` depuis `.env.example`:

```bash
cp .env.example .env
```

**Variables principales**:
```bash
# MongoDB
MONGO_INITDB_ROOT_USERNAME=admin
MONGO_INITDB_ROOT_PASSWORD=admin123
MONGO_APP_USERNAME=rpgarena_user
MONGO_APP_PASSWORD=rpgarena_pass

# Backend
ASPNETCORE_ENVIRONMENT=Development
CERTIFICATE_PASSWORD=devpassword

# MongoExpress
ME_CONFIG_BASICAUTH_USERNAME=admin
ME_CONFIG_BASICAUTH_PASSWORD=pass

# Backup
BACKUP_RETENTION_DAYS=7
BACKUP_SCHEDULE="0 2 * * *"
```

### Certificats SSL

**Développement** (auto-signé):
```bash
./scripts/generate-cert.sh
```

Le script génère un certificat auto-signé pour le développement local. Le backend expose HTTPS sur le port **5443** (mappé depuis le port interne 5001) pour éviter les conflits avec d'autres services système.

**Tester HTTPS**:
```bash
# HTTP
curl http://localhost:5000/health

# HTTPS (avec -k pour ignorer certificat auto-signé)
curl -k https://localhost:5443/health
```

**Production** (Let's Encrypt recommandé):
```bash
# Placer le certificat dans docker/https/aspnetcore.pfx
# Configurer CERTIFICATE_PASSWORD dans .env
```

**Note**: Si vous voulez utiliser le port 5001 sur l'hôte au lieu de 5443, modifiez `docker-compose.yml` ligne 35:
```yaml
ports:
  - "5001:5001"  # Au lieu de "5443:5001"
```


## 📊 Monitoring

### Logs

```bash
# Tous les services
docker compose logs -f

# Service spécifique
docker compose logs -f backend
docker compose logs -f mongodb

# Dernières 100 lignes
docker compose logs --tail 100 backend
```

### État des Services

```bash
# Liste des containers
docker compose ps

# Utilisation des ressources
docker stats

# Health checks
docker compose ps | grep "(healthy)"
```

### Health Check Backend

```bash
curl http://localhost:5000/health

# Résultat attendu:
{
  "status": "healthy",
  "service": "rpgarena-backend",
  "timestamp": "2025-10-25T20:00:00Z"
}
```

## 💾 Backup & Restore

### Backup Manuel

```bash
./scripts/backup.sh
```

### Backup Automatique (Production)

En mode production, un cron job backup automatiquement tous les jours à 2h:

```bash
# Vérifier les backups planifiés
docker compose -f docker-compose.yml -f docker-compose.prod.yml exec backup-cron crontab -l

# Logs du service backup
docker compose logs backup-cron
```

### Restore

```bash
./scripts/restore.sh

# OU manuellement
docker compose --profile backup run --rm mongodb-backup bash /scripts/restore.sh <backup_file>
```

## 🧪 Tests

### Tests Unitaires

```bash
# Via script
./scripts/run-tests.sh unit

# OU directement
docker compose --profile test run --rm unit-tests
```

### Tests d'Intégration

```bash
# Via script
./scripts/run-tests.sh integration

# OU directement
docker compose --profile test run --rm integration-tests
```

### Tous les Tests

```bash
./scripts/run-tests.sh all
```

## 🔒 Sécurité

### Bonnes Pratiques Production

1. **Credentials**:
   - ⚠️ Changer tous les mots de passe par défaut
   - 🔐 Utiliser des secrets Docker ou variables d'environnement sécurisées

2. **Réseau**:
   - ✅ Ne pas exposer MongoDB sur l'hôte
   - ✅ Utiliser un reverse proxy (Nginx/Traefik) avec SSL/TLS
   - ✅ Configurer un firewall

3. **Images**:
   - ✅ Scanner les vulnérabilités: `docker scan rpgarena-backend`
   - ✅ Mettre à jour régulièrement les images de base

4. **Certificats**:
   - ✅ Utiliser Let's Encrypt pour les certificats SSL
   - ✅ Renouvellement automatique

5. **Monitoring**:
   - ✅ Configurer Prometheus + Grafana
   - ✅ Alerting sur les health checks

## 📈 Performance

### Optimisations Docker

1. **Multi-stage Build**:
   - Stage `build`: SDK complet (.NET 9)
   - Stage `publish`: Compilation optimisée
   - Stage `final`: Runtime minimal (aspnet:9.0)
   - **Résultat**: Image finale ~200MB vs ~1GB

2. **Volumes**:
   - Named volumes pour meilleures performances
   - Volumes anonymes pour exclure bin/obj en dev

3. **Health Checks**:
   - Évite les requêtes vers services non-prêts
   - Restart automatique si unhealthy

### Limites de Ressources

En production (docker-compose.prod.yml):

```yaml
deploy:
  resources:
    limits:
      cpus: '2.0'
      memory: 1G
    reservations:
      cpus: '0.5'
      memory: 512M
```

## 🐛 Troubleshooting

### MongoDB ne démarre pas

```bash
# Vérifier les logs
docker compose logs mongodb

# Problème commun: port 27017 déjà utilisé
sudo lsof -i :27017
sudo kill -9 <PID>

# Recréer les volumes
docker compose down -v
docker compose up -d
```

### Backend ne connecte pas à MongoDB

```bash
# Vérifier la connection string
docker compose exec backend env | grep ConnectionStrings

# Tester depuis le backend
docker compose exec backend curl http://localhost:5000/health

# Vérifier que MongoDB est accessible
docker compose exec backend nc -zv mongodb 27017
```

### Certificat SSL invalide

```bash
# Regénérer le certificat
rm docker/https/aspnetcore.pfx
./scripts/generate-cert.sh

# Redémarrer le backend
docker compose restart backend
```

## 📚 Références

- [Dockerfile Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [Docker Compose](https://docs.docker.com/compose/)
- [MongoDB Docker](https://hub.docker.com/_/mongo)
- [.NET in Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Aspire Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview)

---

**Mis à jour**: 25 octobre 2025  
**Version**: 1.0.0  
**Stack**: .NET 9 + MongoDB 8.0 + Docker Compose
