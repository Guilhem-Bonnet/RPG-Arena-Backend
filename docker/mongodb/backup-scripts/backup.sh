#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de backup MongoDB - RPG Arena
# Usage: docker compose --profile backup up mongodb-backup
# ═══════════════════════════════════════════════════════════════

set -e

# Configuration
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_DIR="/backups"
BACKUP_NAME="rpgarena_backup_${TIMESTAMP}"
RETENTION_DAYS=7

echo "🗄️  Début du backup MongoDB..."
echo "   Timestamp: ${TIMESTAMP}"
echo "   Destination: ${BACKUP_DIR}/${BACKUP_NAME}"

# Création du répertoire de backup
mkdir -p "${BACKUP_DIR}/${BACKUP_NAME}"

# Backup avec mongodump
mongodump \
  --host="${MONGO_HOST}" \
  --port="${MONGO_PORT}" \
  --username="${MONGO_USERNAME}" \
  --password="${MONGO_PASSWORD}" \
  --authenticationDatabase="${MONGO_AUTH_SOURCE}" \
  --db="${MONGO_DATABASE}" \
  --out="${BACKUP_DIR}/${BACKUP_NAME}" \
  --gzip

# Compression supplémentaire (tar.gz)
cd "${BACKUP_DIR}"
tar -czf "${BACKUP_NAME}.tar.gz" "${BACKUP_NAME}"
rm -rf "${BACKUP_NAME}"

echo "✅ Backup créé: ${BACKUP_NAME}.tar.gz"

# Calcul de la taille
BACKUP_SIZE=$(du -h "${BACKUP_NAME}.tar.gz" | cut -f1)
echo "   Taille: ${BACKUP_SIZE}"

# Nettoyage des anciens backups
echo "🧹 Nettoyage des backups de plus de ${RETENTION_DAYS} jours..."
find "${BACKUP_DIR}" -name "rpgarena_backup_*.tar.gz" -type f -mtime +${RETENTION_DAYS} -delete

# Affichage des backups disponibles
echo ""
echo "📦 Backups disponibles:"
ls -lh "${BACKUP_DIR}"/rpgarena_backup_*.tar.gz 2>/dev/null || echo "   Aucun backup trouvé"

echo ""
echo "🎉 Backup terminé avec succès!"
