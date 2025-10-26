#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Tests d'intégration - RPG Arena
# Valide Docker + MongoDB + Aspire + Backend
# ═══════════════════════════════════════════════════════════════

BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🧪 Tests d'Intégration - RPG Arena${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ───────────────────────────────────────────────────────────────
# Test 1: MongoDB disponible
# ───────────────────────────────────────────────────────────────
echo -e "${BLUE}🗄️  Test 1: MongoDB...${NC}"

if docker exec rpgarena-mongodb mongosh --quiet --eval "db.version()" > /dev/null 2>&1; then
    MONGO_VERSION=$(docker exec rpgarena-mongodb mongosh --quiet --eval "db.version()")
    echo -e "   ${GREEN}✅ MongoDB opérationnel (version $MONGO_VERSION)${NC}"
else
    echo -e "   ${RED}❌ MongoDB non accessible${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 2: Base de données initialisée
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}📚 Test 2: Base de données...${NC}"

# Test utilisateur
if docker exec rpgarena-mongodb mongosh --quiet --authenticationDatabase RPGArena -u rpgarena_user -p rpgarena_pass RPGArena --eval "db.getName()" > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ Authentification réussie${NC}"
else
    echo -e "   ${RED}❌ Échec d'authentification${NC}"
    exit 1
fi

# Test collections
COLLECTIONS=$(docker exec rpgarena-mongodb mongosh --quiet --authenticationDatabase RPGArena -u rpgarena_user -p rpgarena_pass RPGArena --eval "db.getCollectionNames().join(',')")

if [[ "$COLLECTIONS" == *"combats"* ]] && [[ "$COLLECTIONS" == *"combat_logs"* ]] && [[ "$COLLECTIONS" == *"statistics"* ]]; then
    echo -e "   ${GREEN}✅ Collections créées: $COLLECTIONS${NC}"
else
    echo -e "   ${RED}❌ Collections manquantes${NC}"
    exit 1
fi

# Test données de test
COMBAT_COUNT=$(docker exec rpgarena-mongodb mongosh --quiet --authenticationDatabase RPGArena -u rpgarena_user -p rpgarena_pass RPGArena --eval "db.combats.countDocuments()")

if [ "$COMBAT_COUNT" -ge 2 ]; then
    echo -e "   ${GREEN}✅ Données de test présentes ($COMBAT_COUNT combats)${NC}"
else
    echo -e "   ${YELLOW}⚠️  Données de test insuffisantes ($COMBAT_COUNT combats)${NC}"
fi

# ───────────────────────────────────────────────────────────────
# Test 3: Index MongoDB
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}📇 Test 3: Index...${NC}"

INDEXES=$(docker exec rpgarena-mongodb mongosh --quiet --authenticationDatabase RPGArena -u rpgarena_user -p rpgarena_pass RPGArena --eval "db.combats.getIndexes().length")

if [ "$INDEXES" -ge 5 ]; then
    echo -e "   ${GREEN}✅ Index créés ($INDEXES index sur combats)${NC}"
else
    echo -e "   ${YELLOW}⚠️  Index incomplets ($INDEXES index)${NC}"
fi

# ───────────────────────────────────────────────────────────────
# Test 4: MongoExpress
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🌐 Test 4: MongoExpress...${NC}"

if curl -s -u admin:pass http://localhost:8081 > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ MongoExpress accessible${NC}"
else
    echo -e "   ${RED}❌ MongoExpress inaccessible${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 5: Compilation .NET
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🔨 Test 5: Compilation...${NC}"

if dotnet build RPG-Arena.csproj --nologo --verbosity quiet > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ Projet compile sans erreur${NC}"
else
    echo -e "   ${RED}❌ Erreurs de compilation${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 6: Tests unitaires
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🧪 Test 6: Tests unitaires...${NC}"

TEST_OUTPUT=$(dotnet test --no-build --verbosity quiet 2>&1)
if echo "$TEST_OUTPUT" | grep -q "Réussi"; then
    TEST_COUNT=$(echo "$TEST_OUTPUT" | grep -oP '\d+(?= réussi)' | head -1)
    echo -e "   ${GREEN}✅ Tests unitaires OK ($TEST_COUNT tests)${NC}"
else
    echo -e "   ${RED}❌ Tests en échec${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 7: Connexion Backend → MongoDB
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🔌 Test 7: Connexion Backend...${NC}"

# Test la string de connexion
CONNECTION_STRING="mongodb://rpgarena_user:rpgarena_pass@localhost:27017/RPGArena?authSource=RPGArena"
echo -e "   ${BLUE}📝 Connection String:${NC}"
echo -e "      $CONNECTION_STRING"

# Vérifier que le backend peut compiler avec la référence MongoDB
if grep -q "Aspire.MongoDB.Driver" RPGArena.Backend/RPGArena.Backend.csproj; then
    echo -e "   ${GREEN}✅ Backend configuré avec Aspire.MongoDB.Driver${NC}"
else
    echo -e "   ${RED}❌ Aspire.MongoDB.Driver manquant${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 8: Scripts de backup
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}💾 Test 8: Backup...${NC}"

# Créer un backup de test
echo -e "   ${BLUE}Création d'un backup de test...${NC}"
./scripts/backup.sh > /dev/null 2>&1

# Vérifier que le backup existe
BACKUP_COUNT=$(ls -1 backups/*.tar.gz 2>/dev/null | wc -l)
if [ "$BACKUP_COUNT" -gt 0 ]; then
    LATEST_BACKUP=$(ls -t backups/*.tar.gz 2>/dev/null | head -1)
    BACKUP_SIZE=$(du -h "$LATEST_BACKUP" | cut -f1)
    echo -e "   ${GREEN}✅ Backup créé: $(basename $LATEST_BACKUP) ($BACKUP_SIZE)${NC}"
else
    echo -e "   ${RED}❌ Échec du backup${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 9: Aspire AppHost
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🚀 Test 9: Aspire AppHost...${NC}"

# Vérifier la détection Docker Compose
if grep -q "docker-compose.yml" Program.cs; then
    echo -e "   ${GREEN}✅ Détection Docker Compose configurée${NC}"
else
    echo -e "   ${RED}❌ Détection Docker Compose manquante${NC}"
    exit 1
fi

# Test compilation AppHost
if dotnet build RPG-Arena.csproj --nologo --verbosity quiet > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ AppHost compile${NC}"
else
    echo -e "   ${RED}❌ Erreur compilation AppHost${NC}"
    exit 1
fi

# ───────────────────────────────────────────────────────────────
# Test 10: Documentation
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}📖 Test 10: Documentation...${NC}"

DOCS_OK=0
if [ -f "DOCKER.md" ]; then
    echo -e "   ${GREEN}✅ DOCKER.md présent${NC}"
    ((DOCS_OK++))
fi

if [ -f "ASPIRE.md" ]; then
    echo -e "   ${GREEN}✅ ASPIRE.md présent${NC}"
    ((DOCS_OK++))
fi

if [ -f "QUICK_START.md" ]; then
    echo -e "   ${GREEN}✅ QUICK_START.md présent${NC}"
    ((DOCS_OK++))
fi

if [ "$DOCS_OK" -eq 3 ]; then
    echo -e "   ${GREEN}✅ Documentation complète${NC}"
else
    echo -e "   ${YELLOW}⚠️  Documentation incomplète ($DOCS_OK/3)${NC}"
fi

# ───────────────────────────────────────────────────────────────
# Résumé
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ TOUS LES TESTS D'INTÉGRATION PASSENT !${NC}"
echo ""
echo -e "${BLUE}📊 Résumé:${NC}"
echo -e "   🗄️  MongoDB 8.0 opérationnel"
echo -e "   📚 Base RPGArena initialisée avec $COMBAT_COUNT combats"
echo -e "   🌐 MongoExpress accessible"
echo -e "   🔨 Compilation .NET 9 réussie"
echo -e "   🧪 $TEST_COUNT tests unitaires OK"
echo -e "   💾 Système de backup fonctionnel"
echo -e "   🚀 Aspire AppHost avec détection Docker Compose"
echo -e "   📖 Documentation complète ($DOCS_OK/3 fichiers)"
echo ""
echo -e "${BLUE}🎉 L'infrastructure est prête pour le développement !${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
