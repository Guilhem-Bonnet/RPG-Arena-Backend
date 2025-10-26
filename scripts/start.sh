#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de démarrage - RPG Arena
# Lance la stack complète: Backend + MongoDB + MongoExpress
# ═══════════════════════════════════════════════════════════════

set -e

# Couleurs pour l'affichage
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Mode d'exécution (dev ou prod)
MODE="${1:-dev}"

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🚀 RPG Arena - Démarrage de l'environnement ($MODE)${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

# Vérification de Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker n'est pas installé ou pas dans le PATH${NC}"
    exit 1
fi

if ! docker info &> /dev/null; then
    echo -e "${RED}❌ Docker daemon n'est pas démarré${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Docker est disponible${NC}"

# Création du fichier .env s'il n'existe pas
if [ ! -f .env ]; then
    echo -e "${YELLOW}⚠️  Fichier .env introuvable, copie depuis .env.example${NC}"
    cp .env.example .env
    echo -e "${GREEN}✅ Fichier .env créé${NC}"
fi

# Création des répertoires nécessaires
echo -e "${BLUE}📁 Création des répertoires...${NC}"
mkdir -p backups
mkdir -p logs
mkdir -p docker/mongodb/init-scripts
mkdir -p docker/mongodb/backup-scripts
mkdir -p docker/https

# Génération du certificat SSL si nécessaire
if [ ! -f "docker/https/aspnetcore.pfx" ]; then
    echo -e "${YELLOW}🔐 Génération du certificat SSL de développement...${NC}"
    if [ "$MODE" = "dev" ]; then
        bash scripts/generate-cert.sh || echo -e "${YELLOW}⚠️  Certificat non généré, HTTPS désactivé${NC}"
    fi
fi

# Arrêt propre des containers existants
echo -e "${BLUE}🛑 Arrêt des containers existants...${NC}"
docker compose down 2>/dev/null || true

# Démarrage des services selon le mode
echo -e "${BLUE}🚀 Démarrage des services Docker...${NC}"
if [ "$MODE" = "prod" ]; then
    docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
else
    docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build
fi

# Attente que MongoDB soit prêt
echo -e "${BLUE}⏳ Attente de MongoDB...${NC}"
for i in {1..30}; do
    if docker compose exec -T mongodb mongosh --eval "db.adminCommand('ping')" --quiet &>/dev/null; then
        echo -e "${GREEN}✅ MongoDB est prêt !${NC}"
        break
    fi
    
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ Timeout: MongoDB n'a pas démarré${NC}"
        docker compose logs mongodb
        exit 1
    fi
    
    echo -n "."
    sleep 1
done

# Affichage des logs d'initialisation
echo ""
echo -e "${BLUE}📋 Logs d'initialisation MongoDB:${NC}"
docker compose logs mongodb | grep -E "(🚀|✅|📋|🎉)" || true

# Vérification du Backend
echo ""
echo -e "${BLUE}⏳ Attente du Backend...${NC}"
for i in {1..40}; do
    if curl -s http://localhost:5000/health >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Backend est prêt !${NC}"
        break
    fi
    
    if [ $i -eq 40 ]; then
        echo -e "${RED}❌ Backend n'a pas démarré${NC}"
        docker compose logs backend
        exit 1
    fi
    
    echo -n "."
    sleep 1
done

# Vérification de MongoExpress (optionnel en prod)
if [ "$MODE" = "dev" ]; then
    echo ""
    echo -e "${BLUE}⏳ Attente de MongoExpress...${NC}"
    for i in {1..15}; do
        if curl -s http://localhost:8081 >/dev/null 2>&1; then
            echo -e "${GREEN}✅ MongoExpress est prêt !${NC}"
            break
        fi
        
        if [ $i -eq 15 ]; then
            echo -e "${YELLOW}⚠️  MongoExpress n'a pas démarré (non-bloquant)${NC}"
        fi
        
        echo -n "."
        sleep 1
    done
fi

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ Environnement démarré avec succès !${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${BLUE}📊 Services disponibles:${NC}"
echo -e "   🚀 Backend HTTP:     ${GREEN}http://localhost:5000${NC}"
echo -e "   🔒 Backend HTTPS:    ${GREEN}https://localhost:5001${NC}"
echo -e "   📡 WebSocket:        ${GREEN}ws://localhost:5000/ws${NC}"
echo -e "   🏥 Health Check:     ${GREEN}http://localhost:5000/health${NC}"
echo -e "   🗄️  MongoDB:         ${GREEN}mongodb://localhost:27017${NC}"

if [ "$MODE" = "dev" ]; then
    echo -e "   🌐 MongoExpress:     ${GREEN}http://localhost:8081${NC}"
    echo -e "       └─ User:         ${YELLOW}admin${NC}"
    echo -e "       └─ Password:     ${YELLOW}pass${NC}"
fi

echo ""
echo -e "${BLUE}🔐 Credentials MongoDB:${NC}"
echo -e "   Admin:    ${YELLOW}admin / admin123${NC}"
echo -e "   App User: ${YELLOW}rpgarena_user / rpgarena_pass${NC}"
echo ""
echo -e "${BLUE}📝 Commandes utiles:${NC}"
echo -e "   ${GREEN}docker compose logs -f backend${NC}   # Logs du backend"
echo -e "   ${GREEN}docker compose logs -f mongodb${NC}   # Logs MongoDB"
echo -e "   ${GREEN}docker compose ps${NC}                # État des services"
echo -e "   ${GREEN}docker compose restart backend${NC}   # Redémarrer le backend"
echo -e "   ${GREEN}./scripts/backup.sh${NC}              # Créer un backup"
echo -e "   ${GREEN}./scripts/stop.sh${NC}                # Arrêter proprement"
echo ""
echo -e "${GREEN}🎉 La stack complète est opérationnelle !${NC}"
echo -e "${BLUE}   Mode: ${YELLOW}$MODE${NC}"
echo ""
