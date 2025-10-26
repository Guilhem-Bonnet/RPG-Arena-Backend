#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de backup - RPG Arena
# Crée un backup de la base MongoDB
# ═══════════════════════════════════════════════════════════════

set -e

BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🗄️  RPG Arena - Backup MongoDB${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

# Vérification que MongoDB tourne
if ! docker compose ps mongodb | grep -q "running"; then
    echo -e "${YELLOW}⚠️  MongoDB n'est pas démarré${NC}"
    echo -e "${BLUE}Démarrage de MongoDB...${NC}"
    docker compose up -d mongodb
    sleep 5
fi

echo -e "${BLUE}🚀 Lancement du backup...${NC}"

# Lancement du service de backup
docker compose --profile backup run --rm mongodb-backup

echo ""
echo -e "${GREEN}✅ Backup terminé${NC}"
echo ""
echo -e "${BLUE}📦 Backups disponibles dans: ${GREEN}./backups/${NC}"
ls -lh backups/rpgarena_backup_*.tar.gz 2>/dev/null || echo "  Aucun backup trouvé"
echo ""
