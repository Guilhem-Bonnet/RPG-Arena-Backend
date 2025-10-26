#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de restauration - RPG Arena
# Restaure un backup MongoDB
# ═══════════════════════════════════════════════════════════════

set -e

BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🔄 RPG Arena - Restauration MongoDB${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

# Lister les backups disponibles
echo -e "${BLUE}📦 Backups disponibles:${NC}"
BACKUPS=(./backups/rpgarena_backup_*.tar.gz)

if [ ! -f "${BACKUPS[0]}" ]; then
    echo -e "${RED}❌ Aucun backup trouvé dans ./backups/${NC}"
    exit 1
fi

select backup in "${BACKUPS[@]}" "Annuler"; do
    if [ "$backup" = "Annuler" ]; then
        echo -e "${YELLOW}❌ Restauration annulée${NC}"
        exit 0
    elif [ -n "$backup" ]; then
        BACKUP_FILE=$(basename "$backup")
        break
    fi
done

echo ""
echo -e "${YELLOW}⚠️  Backup sélectionné: ${backup}${NC}"
echo -e "${RED}⚠️  Cette opération va ÉCRASER toutes les données actuelles${NC}"
read -p "Continuer? (y/N) " -n 1 -r
echo

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}❌ Restauration annulée${NC}"
    exit 0
fi

# Vérification que MongoDB tourne
if ! docker compose ps mongodb | grep -q "running"; then
    echo -e "${BLUE}Démarrage de MongoDB...${NC}"
    docker compose up -d mongodb
    sleep 5
fi

# Restauration
echo -e "${BLUE}🔄 Restauration en cours...${NC}"

docker compose exec -T mongodb bash -c "
    cd /backups
    export MONGO_HOST=localhost
    export MONGO_PORT=27017
    export MONGO_USERNAME=admin
    export MONGO_PASSWORD=admin123
    export MONGO_DATABASE=RPGArena
    bash /scripts/restore.sh '${BACKUP_FILE}'
"

echo ""
echo -e "${GREEN}✅ Restauration terminée${NC}"
echo ""
