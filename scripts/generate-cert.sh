#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# Script de génération de certificat SSL pour développement
# NE PAS UTILISER EN PRODUCTION
# ═══════════════════════════════════════════════════════════════

set -e

CERT_DIR="./docker/https"
CERT_FILE="$CERT_DIR/aspnetcore.pfx"
CERT_PASSWORD="${CERTIFICATE_PASSWORD:-devpassword}"

echo "🔐 Génération du certificat SSL de développement..."

# Créer le répertoire si nécessaire
mkdir -p "$CERT_DIR"

# Générer le certificat auto-signé
dotnet dev-certs https -ep "$CERT_FILE" -p "$CERT_PASSWORD" --trust

echo "✅ Certificat généré: $CERT_FILE"
echo "🔑 Mot de passe: $CERT_PASSWORD"
echo ""
echo "⚠️  ATTENTION: Ce certificat est pour le développement uniquement!"
echo "    En production, utilisez un certificat valide (Let's Encrypt, etc.)"
