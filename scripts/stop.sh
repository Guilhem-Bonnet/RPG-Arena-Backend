#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script d'arrêt - RPG Arena
# Arrête proprement les containers Docker
# ═══════════════════════════════════════════════════════════════

set -e

BLUE='\033[0;34m'
GREEN='\033[0;32m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🛑 RPG Arena - Arrêt de l'environnement${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

# Arrêt des containers
echo -e "${BLUE}🛑 Arrêt des containers...${NC}"
docker compose down

echo ""
echo -e "${GREEN}✅ Tous les services sont arrêtés${NC}"
echo ""
echo -e "${BLUE}💡 Pour les redémarrer: ${GREEN}./scripts/start.sh${NC}"
echo ""
