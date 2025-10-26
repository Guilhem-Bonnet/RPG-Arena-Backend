# 🔒 Configuration HTTPS - RPG Arena Backend

## 📋 Vue d'Ensemble

Le backend RPG Arena supporte HTTPS avec certificats SSL/TLS pour sécuriser les communications en production.

## 🎯 Configuration Actuelle

### Ports
- **HTTP**: `5000` (toujours actif)
- **HTTPS**: `5443` sur l'hôte → `5001` dans le conteneur

**Pourquoi port 5443 ?**
Le port standard 5001 peut être occupé par d'autres services système ou Docker. Nous utilisons 5443 pour éviter les conflits tout en conservant le port 5001 à l'intérieur du conteneur (standard ASP.NET Core).

## 🛠️ Configuration Développement

### 1. Générer Certificat Auto-signé

```bash
./scripts/generate-cert.sh
```

Ce script:
- Génère un certificat auto-signé avec `dotnet dev-certs https`
- Exporte le certificat vers `docker/https/aspnetcore.pfx`
- Utilise le mot de passe défini dans `.env` (`CERTIFICATE_PASSWORD`)

### 2. Vérifier le Certificat

```bash
# Vérifier que le fichier existe
ls -lh docker/https/aspnetcore.pfx

# Inspecter le certificat
openssl pkcs12 -in docker/https/aspnetcore.pfx -nodes -passin pass:devpassword | openssl x509 -noout -text
```

### 3. Démarrer avec HTTPS

```bash
# Avec script
./scripts/start.sh dev

# Ou manuellement
docker compose up -d
```

### 4. Tester HTTPS

```bash
# Test HTTP (toujours disponible)
curl http://localhost:5000/health

# Test HTTPS (ignorer avertissement certificat auto-signé)
curl -k https://localhost:5443/health

# Test HTTPS avec détails certificat
curl -kv https://localhost:5443/health

# Vérifier certificat avec OpenSSL
openssl s_client -connect localhost:5443 -showcerts 2>/dev/null | grep -E "(subject|issuer)"
```

**Résultat attendu**:
```json
{
  "status": "healthy",
  "service": "rpgarena-backend",
  "timestamp": "2025-10-26T00:00:00Z"
}
```

## 🏭 Configuration Production

### Option 1: Certificat Let's Encrypt (Recommandé)

#### A. Avec Reverse Proxy (Nginx/Traefik)

Le plus simple en production est d'utiliser un reverse proxy qui gère HTTPS:

```yaml
# docker-compose.prod.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - /etc/letsencrypt:/etc/letsencrypt:ro
    depends_on:
      - backend
  
  backend:
    # Pas besoin d'exposer les ports, Nginx fait proxy
    ports: []
```

**Avantages**:
- Renouvellement automatique Let's Encrypt
- Centralisation SSL/TLS
- Rate limiting, caching, load balancing

#### B. Certificat Direct sur Backend

1. **Obtenir certificat Let's Encrypt**:
```bash
# Installer certbot
sudo apt-get install certbot

# Obtenir certificat (nécessite domaine valide)
sudo certbot certonly --standalone -d rpgarena.votredomaine.com

# Convertir en PFX
sudo openssl pkcs12 -export \
  -out /etc/letsencrypt/live/rpgarena.votredomaine.com/aspnetcore.pfx \
  -inkey /etc/letsencrypt/live/rpgarena.votredomaine.com/privkey.pem \
  -in /etc/letsencrypt/live/rpgarena.votredomaine.com/fullchain.pem \
  -password pass:VotreMotDePasseSecurise
```

2. **Copier certificat**:
```bash
sudo cp /etc/letsencrypt/live/rpgarena.votredomaine.com/aspnetcore.pfx \
  docker/https/aspnetcore.pfx
```

3. **Configurer `.env.production`**:
```env
CERTIFICATE_PASSWORD=VotreMotDePasseSecurise
ASPNETCORE_URLS=http://+:5000;https://+:5001
```

4. **Démarrer production**:
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Option 2: Certificat Organisationnel

Si vous avez un certificat d'entreprise (`.pfx` ou `.p12`):

```bash
# Copier votre certificat
cp /chemin/vers/votre-cert.pfx docker/https/aspnetcore.pfx

# Configurer mot de passe dans .env
echo "CERTIFICATE_PASSWORD=votre_mot_de_passe" >> .env

# Démarrer
docker compose up -d
```

## 🔧 Configuration Avancée

### Forcer HTTPS (Redirection HTTP → HTTPS)

Modifiez `RPGArena.Backend/Program.cs`:

```csharp
var app = builder.Build();

// Forcer HTTPS en production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseWebSockets();
// ... reste du code
```

### Changer Port Hôte

Si vous voulez utiliser port 5001 au lieu de 5443:

1. **Vérifier disponibilité**:
```bash
sudo lsof -i :5001
# Si vide, le port est libre
```

2. **Modifier `docker-compose.yml`**:
```yaml
services:
  backend:
    ports:
      - "5000:5000"   # HTTP
      - "5001:5001"   # HTTPS (au lieu de 5443:5001)
```

3. **Redémarrer**:
```bash
docker compose down
docker compose up -d
```

### HTTPS uniquement (Désactiver HTTP)

```yaml
# docker-compose.yml
services:
  backend:
    environment:
      ASPNETCORE_URLS: https://+:5001  # Retirer http://+:5000
    ports:
      - "5443:5001"  # HTTPS seulement
```

## 🐛 Troubleshooting

### Erreur: Port already allocated

```bash
# Identifier processus utilisant le port
sudo lsof -i :5443  # ou :5001

# Si c'est docker-proxy d'un ancien conteneur
docker ps -a | grep rpgarena
docker rm -f $(docker ps -aq --filter "name=rpgarena")

# Si c'est un autre processus
sudo kill -9 <PID>
```

### Erreur: Certificate validation failed

En développement avec certificat auto-signé:
```bash
# Option 1: Ignorer validation (dev seulement!)
curl -k https://localhost:5443/health

# Option 2: Installer certificat dans trust store
dotnet dev-certs https --trust
```

En production:
- Vérifier que le certificat est valide et non expiré
- Vérifier que le domaine correspond au certificat
- Vérifier les dates de validité: `openssl x509 -in cert.pem -noout -dates`

### Certificat Expiré

```bash
# Développement: Régénérer
rm docker/https/aspnetcore.pfx
./scripts/generate-cert.sh

# Production: Renouveler Let's Encrypt
sudo certbot renew
# Puis reconvertir en PFX et redémarrer conteneurs
```

### WebSocket sur HTTPS

Pour WebSocket sécurisé (WSS):
```javascript
// Client Godot/JavaScript
const ws = new WebSocket("wss://localhost:5443/ws");
```

**Note**: Le certificat doit être valide pour que WSS fonctionne (pas d'option `-k` comme avec curl).

## 📊 Vérification Santé HTTPS

```bash
# Script de vérification complet
./scripts/validate.sh

# Ou manuellement
echo "✅ HTTP:" && curl -s http://localhost:5000/health | jq .status
echo "✅ HTTPS:" && curl -ks https://localhost:5443/health | jq .status
echo "✅ Certificat:" && openssl s_client -connect localhost:5443 2>/dev/null </dev/null | grep -E "(subject|issuer|Verify return code)"
```

## 🔐 Sécurité Best Practices

### Développement
- ✅ Certificat auto-signé OK
- ✅ Port 5443 pour éviter conflits
- ⚠️ Ne JAMAIS commit les certificats dans Git
- ⚠️ Ne JAMAIS commit les mots de passe en clair

### Production
- ✅ Utiliser Let's Encrypt ou certificat organisationnel
- ✅ Activer HTTPS uniquement (désactiver HTTP)
- ✅ Forcer redirection HTTP → HTTPS
- ✅ Utiliser reverse proxy (Nginx/Traefik) avec rate limiting
- ✅ Renouvellement automatique certificats
- ✅ HSTS (HTTP Strict Transport Security)
- ✅ Secrets externalisés (Azure KeyVault, AWS Secrets Manager)

## 📚 Ressources

- [ASP.NET Core HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [Let's Encrypt](https://letsencrypt.org/)
- [SSL Labs Test](https://www.ssllabs.com/ssltest/)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)

---

**Dernière mise à jour**: 26 octobre 2025  
**Version**: 1.0.0
