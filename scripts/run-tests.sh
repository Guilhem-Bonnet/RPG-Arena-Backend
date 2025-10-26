#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de tests complets - RPG Arena
# Lance les tests unitaires et d'intégration via Docker
# ═══════════════════════════════════════════════════════════════

BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🧪 RPG Arena - Suite de Tests Complète${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ───────────────────────────────────────────────────────────────
# Arguments
# ───────────────────────────────────────────────────────────────
TEST_TYPE="${1:-all}"  # all, unit, integration

# ───────────────────────────────────────────────────────────────
# Fonction: Build de l'image de tests
# ───────────────────────────────────────────────────────────────
build_test_image() {
    echo -e "${BLUE}🔨 Build de l'image de tests...${NC}"
    if docker compose build integration-tests > /dev/null 2>&1; then
        echo -e "   ${GREEN}✅ Image de tests construite${NC}"
        return 0
    else
        echo -e "   ${RED}❌ Échec du build${NC}"
        return 1
    fi
}

# ───────────────────────────────────────────────────────────────
# Fonction: Tests unitaires
# ───────────────────────────────────────────────────────────────
run_unit_tests() {
    echo ""
    echo -e "${BLUE}🧪 Tests Unitaires...${NC}"
    
    if docker compose --profile test run --rm unit-tests; then
        echo -e "${GREEN}✅ Tests unitaires réussis${NC}"
        return 0
    else
        echo -e "${RED}❌ Tests unitaires échoués${NC}"
        return 1
    fi
}

# ───────────────────────────────────────────────────────────────
# Fonction: Tests d'intégration
# ───────────────────────────────────────────────────────────────
run_integration_tests() {
    echo ""
    echo -e "${BLUE}🔌 Tests d'Intégration...${NC}"
    
    # S'assurer que MongoDB est démarré
    if ! docker ps | grep -q rpgarena-mongodb; then
        echo -e "   ${YELLOW}⚠️  MongoDB non démarré, lancement...${NC}"
        docker compose up -d mongodb mongo-express
        
        # Attendre que MongoDB soit prêt
        echo -e "   ${BLUE}⏳ Attente de MongoDB...${NC}"
        for i in {1..30}; do
            if docker exec rpgarena-mongodb mongosh --quiet --eval "db.version()" > /dev/null 2>&1; then
                echo -e "   ${GREEN}✅ MongoDB prêt${NC}"
                break
            fi
            sleep 1
        done
    fi
    
    if docker compose --profile test run --rm integration-tests; then
        echo -e "${GREEN}✅ Tests d'intégration réussis${NC}"
        return 0
    else
        echo -e "${RED}❌ Tests d'intégration échoués${NC}"
        return 1
    fi
}

# ───────────────────────────────────────────────────────────────
# Fonction: Nettoyage
# ───────────────────────────────────────────────────────────────
cleanup() {
    echo ""
    echo -e "${BLUE}🧹 Nettoyage...${NC}"
    docker compose --profile test down > /dev/null 2>&1
    echo -e "   ${GREEN}✅ Nettoyage terminé${NC}"
}

# ───────────────────────────────────────────────────────────────
# Exécution
# ───────────────────────────────────────────────────────────────

# Build de l'image
if ! build_test_image; then
    exit 1
fi

FAILED=0

# Lancer les tests selon le type demandé
case "$TEST_TYPE" in
    unit)
        run_unit_tests || FAILED=1
        ;;
    integration)
        run_integration_tests || FAILED=1
        ;;
    all)
        run_unit_tests || FAILED=1
        run_integration_tests || FAILED=1
        ;;
    *)
        echo -e "${RED}❌ Type de test invalide: $TEST_TYPE${NC}"
        echo -e "   Usage: $0 [all|unit|integration]"
        exit 1
        ;;
esac

# Résumé
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✅ Tous les tests sont passés avec succès !${NC}"
    echo ""
    echo -e "${BLUE}📊 Commandes disponibles:${NC}"
    echo -e "   ${GREEN}$0 unit${NC}        - Tests unitaires uniquement"
    echo -e "   ${GREEN}$0 integration${NC}  - Tests d'intégration uniquement"
    echo -e "   ${GREEN}$0 all${NC}          - Tous les tests (défaut)"
else
    echo -e "${RED}❌ Certains tests ont échoué${NC}"
    echo ""
    echo -e "${YELLOW}💡 Vérifiez les logs ci-dessus pour plus de détails${NC}"
fi
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

exit $FAILED
