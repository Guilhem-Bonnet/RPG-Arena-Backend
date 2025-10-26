# 🔐 Guide de Sécurité - RPG Arena

## 📋 Vue d'Ensemble

Ce document décrit les pratiques de sécurité pour le déploiement de RPG Arena, particulièrement pour la gestion des secrets et credentials.

## ⚠️ Validation des Secrets en Production

Le backend **refuse de démarrer** en production si des credentials par défaut sont détectés.

### Credentials Validés

Le système vérifie automatiquement :
- ✅ `MONGO_INITDB_ROOT_PASSWORD` ≠ "rootpassword123"
- ✅ `MONGO_PASSWORD` ≠ "rpgarena_pass"
- ✅ `ME_CONFIG_BASICAUTH_PASSWORD` ≠ "pass"
- ✅ `CERTIFICATE_PASSWORD` ≠ "devpassword"

### Exemple d'Erreur

Si des credentials par défaut sont détectés, le backend affiche:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🚨 PRODUCTION SECURITY VALIDATION FAILED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Default credentials detected! Production deployment BLOCKED.

  ❌ MONGO_INITDB_ROOT_PASSWORD: Must be set and not use default value!
  ❌ CERTIFICATE_PASSWORD: Must be set and not use default value!

📝 Action Required:
  1. Copy .env.production.template to .env.production
  2. Set strong passwords (min 20 characters)
  3. Use: docker compose --env-file .env.production up -d

💡 Generate strong passwords:
  openssl rand -base64 32
  pwgen -s 32 1

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🛠️ Configuration Environnements

### Développement (`.env`)

Credentials par défaut acceptés :
```env
MONGO_INITDB_ROOT_PASSWORD=rootpassword123
MONGO_PASSWORD=rpgarena_pass
CERTIFICATE_PASSWORD=devpassword
ASPNETCORE_ENVIRONMENT=Development
```

**⚠️ NE JAMAIS utiliser ces valeurs en production!**

### Production (`.env.production`)

1. **Copier le template**:
```bash
cp .env.production.template .env.production
```

2. **Générer mots de passe forts**:
```bash
# Méthode 1: OpenSSL
openssl rand -base64 32

# Méthode 2: pwgen
pwgen -s 32 1

# Méthode 3: Python
python3 -c "import secrets; print(secrets.token_urlsafe(32))"

# Méthode 4: /dev/urandom
tr -dc 'A-Za-z0-9!@#$%^&*()_+=' < /dev/urandom | head -c 32
```

3. **Éditer `.env.production`**:
```env
MONGO_INITDB_ROOT_PASSWORD=Xk7mP9vL2qR8wN4jB6sT0yU3hG5fD1aZ
MONGO_PASSWORD=Qw9eR7tY5uI3oP1aS0dF8gH6jK4lZ2xC
ME_CONFIG_BASICAUTH_PASSWORD=Mn8bV6cX4zL2kJ0hG9fD7sA5qW3eR1tY
CERTIFICATE_PASSWORD=Pl0oK9iJ8uH7yG6tF5rD4eS3wQ2aZ1xC
ASPNETCORE_ENVIRONMENT=Production
```

4. **Démarrer avec .env.production**:
```bash
docker compose --env-file .env.production up -d
```

5. **Vérifier**:
```bash
docker compose logs backend | grep "Production secrets validation"
# Doit afficher: ✅ Production secrets validation passed
```

## 🔐 Gestion Avancée des Secrets

### Option 1: Azure Key Vault (Recommandé pour Azure)

#### A. Configuration Azure

```bash
# 1. Créer Key Vault
az keyvault create \
  --name rpgarena-keyvault \
  --resource-group rpgarena-rg \
  --location westeurope

# 2. Ajouter secrets
az keyvault secret set \
  --vault-name rpgarena-keyvault \
  --name MongoRootPassword \
  --value "your-strong-password"

az keyvault secret set \
  --vault-name rpgarena-keyvault \
  --name MongoUserPassword \
  --value "your-strong-password"

# 3. Créer Service Principal
az ad sp create-for-rbac \
  --name rpgarena-sp \
  --role "Key Vault Secrets User" \
  --scopes /subscriptions/{subscription-id}/resourceGroups/rpgarena-rg/providers/Microsoft.KeyVault/vaults/rpgarena-keyvault
```

#### B. Configuration Application

Modifiez `Program.cs`:

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsProduction())
    {
        // Charger secrets depuis Azure Key Vault
        var keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL");
        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
            
            // Remplacer variables d'environnement
            Environment.SetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD", 
                secretClient.GetSecret("MongoRootPassword").Value.Value);
            Environment.SetEnvironmentVariable("MONGO_PASSWORD", 
                secretClient.GetSecret("MongoUserPassword").Value.Value);
        }
    }
    
    // ... reste du code
}
```

#### C. Variables d'Environnement

```env
# .env.production
AZURE_KEYVAULT_URL=https://rpgarena-keyvault.vault.azure.net/
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_TENANT_ID=your-tenant-id
```

### Option 2: AWS Secrets Manager

#### A. Configuration AWS

```bash
# 1. Créer secret
aws secretsmanager create-secret \
  --name rpgarena/production/mongodb \
  --description "MongoDB credentials for RPG Arena" \
  --secret-string '{
    "MONGO_INITDB_ROOT_PASSWORD": "your-strong-password",
    "MONGO_PASSWORD": "your-strong-password"
  }' \
  --region eu-west-1

# 2. Créer IAM role pour ECS/EC2
aws iam create-role \
  --role-name rpgarena-secrets-role \
  --assume-role-policy-document file://trust-policy.json

aws iam attach-role-policy \
  --role-name rpgarena-secrets-role \
  --policy-arn arn:aws:iam::aws:policy/SecretsManagerReadWrite
```

#### B. Configuration Application

```csharp
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json.Linq;

public static async Task Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsProduction())
    {
        var secretName = Environment.GetEnvironmentVariable("AWS_SECRET_NAME");
        var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1";
        
        if (!string.IsNullOrEmpty(secretName))
        {
            var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
            var request = new GetSecretValueRequest { SecretId = secretName };
            var response = await client.GetSecretValueAsync(request);
            
            var secrets = JObject.Parse(response.SecretString);
            Environment.SetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD", 
                secrets["MONGO_INITDB_ROOT_PASSWORD"]?.ToString());
            Environment.SetEnvironmentVariable("MONGO_PASSWORD", 
                secrets["MONGO_PASSWORD"]?.ToString());
        }
    }
    
    // ... reste du code
}
```

#### C. Variables d'Environnement

```env
# .env.production
AWS_REGION=eu-west-1
AWS_SECRET_NAME=rpgarena/production/mongodb
```

### Option 3: HashiCorp Vault

```bash
# 1. Écrire secrets
vault kv put secret/rpgarena/production \
  MONGO_INITDB_ROOT_PASSWORD="your-strong-password" \
  MONGO_PASSWORD="your-strong-password"

# 2. Créer policy
vault policy write rpgarena-policy -<<EOF
path "secret/data/rpgarena/production" {
  capabilities = ["read"]
}
EOF

# 3. Créer token
vault token create -policy=rpgarena-policy
```

Configuration `.env.production`:
```env
VAULT_ADDR=https://vault.example.com:8200
VAULT_TOKEN=your-vault-token
VAULT_SECRET_PATH=secret/rpgarena/production
```

## 🔄 Rotation des Credentials

### MongoDB Password Rotation

```bash
# 1. Générer nouveau mot de passe
NEW_PASSWORD=$(openssl rand -base64 32)

# 2. Connexion à MongoDB
docker exec -it rpgarena-mongodb mongosh -u rpgarena_admin -p rootpassword123 --authenticationDatabase admin

# 3. Changer password
use admin
db.updateUser("rpgarena_user", { pwd: "NEW_PASSWORD_HERE" })

# 4. Mettre à jour .env.production
sed -i "s/MONGO_PASSWORD=.*/MONGO_PASSWORD=$NEW_PASSWORD/" .env.production

# 5. Redémarrer services
docker compose --env-file .env.production up -d --force-recreate backend
```

### Automated Rotation (AWS Secrets Manager)

```bash
# Activer rotation automatique tous les 30 jours
aws secretsmanager rotate-secret \
  --secret-id rpgarena/production/mongodb \
  --rotation-lambda-arn arn:aws:lambda:eu-west-1:123456789012:function:SecretsManagerRotation \
  --rotation-rules AutomaticallyAfterDays=30
```

## 🛡️ Best Practices Sécurité

### 1. Mots de Passe

✅ **DO**:
- Utiliser minimum 20 caractères
- Mélanger majuscules, minuscules, chiffres, caractères spéciaux
- Utiliser générateur sécurisé (OpenSSL, pwgen, secrets.token_urlsafe)
- Rotation régulière (tous les 90 jours)
- Différent pour chaque service

❌ **DON'T**:
- Mots du dictionnaire
- Dates, prénoms
- Séquences simples (12345, azerty)
- Réutiliser entre environnements
- Stocker en clair dans Git

### 2. Fichiers de Configuration

✅ **DO**:
```bash
# .gitignore
.env.production
.env.local
*.pfx
*.pem
*.key
secrets/
```

❌ **DON'T**:
```bash
# ❌ JAMAIS commit ces fichiers!
git add .env.production  # ❌
git add docker/https/*.pfx  # ❌
```

### 3. Permissions Fichiers

```bash
# Fichiers secrets: lecture seule pour owner
chmod 600 .env.production
chmod 600 docker/https/aspnetcore.pfx

# Répertoires secrets: accessible owner seulement
chmod 700 docker/https
chmod 700 backups
```

### 4. Accès MongoDB

```bash
# Production: Désactiver exposition port MongoDB
# docker-compose.prod.yml
services:
  mongodb:
    ports: []  # Pas de port exposé, seulement réseau interne
```

### 5. MongoExpress

```bash
# Production: Désactiver complètement ou profil admin uniquement
# docker-compose.prod.yml
services:
  mongo-express:
    profiles:
      - admin  # Activation manuelle uniquement
```

### 6. Logs

```bash
# Ne JAMAIS logger les mots de passe
Console.WriteLine($"MongoDB connection: mongodb://user:****@host");  # ✅
Console.WriteLine($"MongoDB connection: {connectionString}");  # ❌
```

### 7. HTTPS

```bash
# Production: HTTPS uniquement
ASPNETCORE_URLS=https://+:5001  # Pas de http://+:5000
```

## 🔍 Audit Sécurité

### Checklist Pré-Déploiement

```bash
# 1. Vérifier secrets
grep -r "rootpassword123\|rpgarena_pass\|devpassword" .env* docker-compose*.yml
# Résultat attendu: Aucune occurrence

# 2. Vérifier .gitignore
cat .gitignore | grep ".env.production"
# Résultat attendu: .env.production présent

# 3. Vérifier permissions
ls -la .env.production docker/https/aspnetcore.pfx
# Résultat attendu: -rw------- (600)

# 4. Tester démarrage production
ASPNETCORE_ENVIRONMENT=Production docker compose up backend
# Doit afficher: ✅ Production secrets validation passed
```

### Scan Vulnérabilités

```bash
# Docker images
docker scan rpg-arena-backend

# Trivy (sécurité conteneurs)
trivy image rpg-arena-backend

# Secrets dans Git history
git secrets --scan-history

# OWASP Dependency Check
dependency-check.sh --project RPGArena --scan .
```

## 📊 Monitoring Sécurité

### Alertes

Configurer alertes pour:
- ✅ Tentatives connexion MongoDB échouées (>10/min)
- ✅ Utilisation credentials par défaut détectée
- ✅ Certificats SSL expirant (<30 jours)
- ✅ Accès MongoExpress en production
- ✅ Changements fichiers secrets (.env.production)

### Logs Audit

```bash
# Logs connexions MongoDB
docker compose logs mongodb | grep "authentication failed"

# Logs validation secrets backend
docker compose logs backend | grep "PRODUCTION SECURITY"

# Logs accès MongoExpress
docker compose logs mongo-express | grep "GET /"
```

## 🚨 Incident Response

### Credentials Compromis

1. **Immédiatement**:
```bash
# Arrêter services
docker compose down

# Changer tous les mots de passe
./scripts/rotate-credentials.sh

# Redémarrer avec nouveaux credentials
docker compose --env-file .env.production up -d
```

2. **Analyser**:
```bash
# Logs accès
docker compose logs --since 24h | grep -E "(authentication|login|access)"

# Connexions MongoDB
docker exec rpgarena-mongodb mongosh --eval "db.currentOp()"
```

3. **Documenter**:
- Date/heure compromission
- Credentials affectés
- Actions prises
- Leçons apprises

## 📚 Ressources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [AWS Secrets Manager Best Practices](https://docs.aws.amazon.com/secretsmanager/latest/userguide/best-practices.html)
- [MongoDB Security Checklist](https://www.mongodb.com/docs/manual/administration/security-checklist/)

---

**Dernière mise à jour**: 26 octobre 2025  
**Version**: 1.0.0
