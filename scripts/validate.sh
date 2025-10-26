#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de validation - RPG Arena Docker Setup
# Teste la configuration complète
# ═══════════════════════════════════════════════════════════════

set -e

BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

ERRORS=0

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🧪 RPG Arena - Tests de validation${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ───────────────────────────────────────────────────────────────
# Test 1: Fichiers requis
# ───────────────────────────────────────────────────────────────
echo -e "${BLUE}📁 Test 1: Vérification des fichiers...${NC}"

FILES=(
    "docker-compose.yml"
    ".env.example"
    "docker/mongodb/mongod.conf"
    "docker/mongodb/init-scripts/01-init-database.js"
    "docker/mongodb/backup-scripts/backup.sh"
    "docker/mongodb/backup-scripts/restore.sh"
    "scripts/start.sh"
    "scripts/stop.sh"
    "scripts/backup.sh"
    "scripts/restore.sh"
)

for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        echo -e "   ${GREEN}✅ $file${NC}"
    else
        echo -e "   ${RED}❌ $file (manquant)${NC}"
        ((ERRORS++))
    fi
done

# ───────────────────────────────────────────────────────────────
# Test 2: Permissions des scripts
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🔐 Test 2: Permissions des scripts...${NC}"

SCRIPTS=(
    "docker/mongodb/backup-scripts/backup.sh"
    "docker/mongodb/backup-scripts/restore.sh"
    "scripts/start.sh"
    "scripts/stop.sh"
    "scripts/backup.sh"
    "scripts/restore.sh"
)

for script in "${SCRIPTS[@]}"; do
    if [ -x "$script" ]; then
        echo -e "   ${GREEN}✅ $script (exécutable)${NC}"
    else
        echo -e "   ${YELLOW}⚠️  $script (non exécutable)${NC}"
        chmod +x "$script"
        echo -e "      → Permissions corrigées"
    fi
done

# ───────────────────────────────────────────────────────────────
# Test 3: Docker
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🐳 Test 3: Docker...${NC}"

if command -v docker &> /dev/null; then
    echo -e "   ${GREEN}✅ Docker installé${NC}"
    
    if docker info &> /dev/null; then
        echo -e "   ${GREEN}✅ Docker daemon actif${NC}"
    else
        echo -e "   ${RED}❌ Docker daemon non démarré${NC}"
        ((ERRORS++))
    fi
else
    echo -e "   ${RED}❌ Docker non installé${NC}"
    ((ERRORS++))
fi

# ───────────────────────────────────────────────────────────────
# Test 4: .NET
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}⚙️  Test 4: .NET SDK...${NC}"

if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "   ${GREEN}✅ .NET installé (version $DOTNET_VERSION)${NC}"
    
    if [[ "$DOTNET_VERSION" == 9.* ]]; then
        echo -e "   ${GREEN}✅ .NET 9.x détecté${NC}"
    else
        echo -e "   ${YELLOW}⚠️  Version .NET: $DOTNET_VERSION (9.x recommandé)${NC}"
    fi
else
    echo -e "   ${RED}❌ .NET non installé${NC}"
    ((ERRORS++))
fi

# ───────────────────────────────────────────────────────────────
# Test 5: Compilation
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🔨 Test 5: Compilation du projet...${NC}"

if dotnet build RPG-Arena.csproj > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ Projet compile sans erreur${NC}"
else
    echo -e "   ${RED}❌ Erreurs de compilation${NC}"
    ((ERRORS++))
fi

# ───────────────────────────────────────────────────────────────
# Test 6: Tests unitaires
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🧪 Test 6: Tests unitaires...${NC}"

if dotnet test --no-build --verbosity quiet > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ Tous les tests passent${NC}"
else
    echo -e "   ${RED}❌ Tests en échec${NC}"
    ((ERRORS++))
fi

# ───────────────────────────────────────────────────────────────
# Test 7: Structure Docker Compose
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}📋 Test 7: Validation docker-compose.yml...${NC}"

if docker compose config > /dev/null 2>&1; then
    echo -e "   ${GREEN}✅ docker-compose.yml valide${NC}"
else
    echo -e "   ${RED}❌ docker-compose.yml invalide${NC}"
    ((ERRORS++))
fi

# ───────────────────────────────────────────────────────────────
# Test 8: Fichier .env
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}🔑 Test 8: Configuration .env...${NC}"

if [ -f ".env" ]; then
    echo -e "   ${GREEN}✅ .env existe${NC}"
else
    echo -e "   ${YELLOW}⚠️  .env manquant (sera créé au démarrage)${NC}"
fi

# ───────────────────────────────────────────────────────────────
# Résumé
# ───────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}✅ Tous les tests passent ! Configuration valide.${NC}"
    echo ""
    echo -e "${BLUE}🚀 Vous pouvez démarrer l'application:${NC}"
    echo -e "   ${GREEN}./scripts/start.sh${NC}"
    echo ""
    exit 0
else
    echo -e "${RED}❌ $ERRORS erreur(s) détectée(s)${NC}"
    echo ""
    echo -e "${YELLOW}📝 Actions recommandées:${NC}"
    echo -e "   1. Installer Docker: https://docs.docker.com/get-docker/"
    echo -e "   2. Installer .NET 9: https://dotnet.microsoft.com/download/dotnet/9.0"
    echo -e "   3. Vérifier les fichiers manquants"
    echo ""
    exit 1
fi
