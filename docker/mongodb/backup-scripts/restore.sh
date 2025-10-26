#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de restauration MongoDB - RPG Arena
# Usage: ./restore.sh <backup_file.tar.gz>
# ═══════════════════════════════════════════════════════════════

set -e

if [ $# -eq 0 ]; then
    echo "❌ Erreur: Aucun fichier de backup spécifié"
    echo "Usage: $0 <backup_file.tar.gz>"
    echo ""
    echo "Backups disponibles:"
    ls -1 /backups/rpgarena_backup_*.tar.gz 2>/dev/null || echo "  Aucun backup trouvé"
    exit 1
fi

BACKUP_FILE="$1"
BACKUP_DIR="/backups"
TEMP_DIR="/tmp/mongo_restore_$$"

if [ ! -f "${BACKUP_DIR}/${BACKUP_FILE}" ]; then
    echo "❌ Erreur: Fichier de backup introuvable: ${BACKUP_FILE}"
    exit 1
fi

echo "🔄 Restauration de MongoDB..."
echo "   Backup: ${BACKUP_FILE}"
echo "   Base: ${MONGO_DATABASE}"

# Confirmation
read -p "⚠️  Cette opération va écraser les données existantes. Continuer? (y/N) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Restauration annulée"
    exit 1
fi

# Extraction du backup
mkdir -p "${TEMP_DIR}"
echo "📦 Extraction du backup..."
tar -xzf "${BACKUP_DIR}/${BACKUP_FILE}" -C "${TEMP_DIR}"

# Recherche du répertoire extrait
EXTRACTED_DIR=$(find "${TEMP_DIR}" -maxdepth 1 -type d -name "rpgarena_backup_*" | head -n 1)

if [ -z "${EXTRACTED_DIR}" ]; then
    echo "❌ Erreur: Structure de backup invalide"
    rm -rf "${TEMP_DIR}"
    exit 1
fi

# Restauration avec mongorestore
# Restoration
echo "� Restoration en cours..."
mongorestore \
  --host="${MONGO_HOST}" \
  --port="${MONGO_PORT}" \
  --username="${MONGO_USERNAME}" \
  --password="${MONGO_PASSWORD}" \
  --authenticationDatabase="${MONGO_AUTH_SOURCE}" \
  --db="${MONGO_DATABASE}" \
  --drop \
  "${TEMP_DIR}/${BACKUP_NAME}/${MONGO_DATABASE}"

# Nettoyage
rm -rf "${TEMP_DIR}"

echo "✅ Restauration terminée avec succès!"
echo ""
echo "📊 Vérification des collections:"
mongosh \
  --host="${MONGO_HOST}" \
  --port="${MONGO_PORT}" \
  --username="${MONGO_USERNAME}" \
  --password="${MONGO_PASSWORD}" \
  --authenticationDatabase=admin \
  --eval "
    db = db.getSiblingDB('${MONGO_DATABASE}');
    print('   - Combats: ' + db.combats.countDocuments());
    print('   - Logs: ' + db.combat_logs.countDocuments());
    print('   - Statistiques: ' + db.statistics.countDocuments());
  "
