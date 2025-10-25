# RPG Arena - Backend

Backend WebSocket pour le système de combat RPG en temps réel.

## 🎮 Architecture

Le projet est organisé en 3 couches :

- **RPGArena.Backend** : Serveur WebSocket ASP.NET Core
- **RPGArena.CombatEngine** : Moteur de combat avec 12+ classes de personnages
- **RPGArena.Tests** : Tests unitaires xUnit

## 📋 Prérequis

- **.NET SDK 8.0** (LTS)
- **MongoDB** (version 4.0+)
- **Système d'exploitation** : Linux, Windows, macOS

### Installation de .NET 8.0

```bash
# Linux (Ubuntu/Debian)
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Windows
# Télécharger depuis https://dotnet.microsoft.com/download/dotnet/8.0

# macOS
brew install dotnet@8
```

### Installation de MongoDB

```bash
# Linux (Ubuntu/Debian)
sudo apt-get install mongodb

# Windows
# Télécharger depuis https://www.mongodb.com/try/download/community

# macOS
brew install mongodb-community
```

## 🚀 Démarrage rapide

### 1. Cloner le repository

```bash
git clone <url-du-repo>
cd RPG-Arena
```

### 2. Restaurer les dépendances

```bash
dotnet restore
```

### 3. Configuration MongoDB

Le serveur utilise par défaut `mongodb://localhost:27017`. Pour changer la configuration :

**RPGArena.Backend/appsettings.json** :
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://votre-host:27017"
  }
}
```

### 4. Lancer le serveur

```bash
cd RPGArena.Backend
dotnet run
```

Le serveur démarre sur `http://localhost:5000`  
Endpoint WebSocket : **`ws://localhost:5000/ws`**

## 🧪 Tests

```bash
cd RPGArena.Tests
dotnet test
```

## 🎭 Classes de personnages disponibles

| Classe | Caractéristiques |
|--------|-----------------|
| **Guerrier** | Tank, haute défense |
| **Magicien** | Dégâts magiques, faible PV |
| **Assassin** | Critiques, haute vitesse |
| **Paladin** | Tank/Soigneur |
| **Prêtre** | Soigneur principal |
| **Berserker** | Dégâts élevés, vie basse |
| **Alchimiste** | Potions, états |
| **Nécromancien** | Invocations |
| **Vampire** | Vol de vie |
| **Zombie** | Régénération |
| **Robot** | Résistances |
| **Illusionniste** | Évasion |

## 📡 Utilisation de l'API WebSocket

### Connexion

```javascript
const ws = new WebSocket('ws://localhost:5000/ws');
```

### Format des messages

Envoyer une liste de noms de personnages (JSON) :

```json
["Arthas", "Thrall", "Sylvanas"]
```

Le serveur va :
1. Créer les personnages
2. Lancer le combat
3. Envoyer les logs en temps réel via WebSocket
4. Sauvegarder le résultat dans MongoDB

### Exemple complet (JavaScript)

```javascript
const ws = new WebSocket('ws://localhost:5000/ws');

ws.onopen = () => {
    console.log('✅ Connecté au serveur');
    ws.send(JSON.stringify(["Guerrier1", "Magicien1"]));
};

ws.onmessage = (event) => {
    console.log('📨 Log combat:', event.data);
};

ws.onclose = () => {
    console.log('🔌 Connexion fermée');
};
```

## 🔧 Mode développement

### Mode test (combat automatique)

```bash
cd RPGArena.Backend
dotnet run --test
```

Ceci lance un combat de test sans WebSocket.

### Structure des logs

Le système utilise un **MultiLogger** qui envoie simultanément vers :
- **ConsoleLogger** : Console système (debug)
- **MongoDbLogger** : Base de données MongoDB
- **WebSocketLogger** : Client WebSocket connecté

## 📁 Structure du projet

```
RPG-Arena/
├── RPGArena.Backend/           # Serveur WebSocket
│   ├── Program.cs              # Point d'entrée, DI
│   ├── Services/
│   │   ├── WebSocketHandler.cs
│   │   └── BattleManager.cs
│   ├── Loggers/
│   │   ├── MongoDbLogger.cs
│   │   └── WebSocketLoggerFactory.cs
│   ├── Repositories/
│   │   ├── ICombatRepository.cs
│   │   └── MongoCombatRepository.cs
│   └── Models/
│       └── CombatRecord.cs
│
├── RPGArena.CombatEngine/      # Moteur de combat
│   ├── Characters/             # 12 classes
│   ├── Skills/                 # Compétences
│   ├── States/                 # États (poison, étourdissement...)
│   ├── Core/
│   │   └── BattleArena.cs      # Orchestrateur de combat
│   ├── Services/
│   │   └── FightService.cs     # Logique de combat
│   └── Logging/
│       └── ILogger.cs
│
└── RPGArena.Tests/             # Tests unitaires
    └── CharacterFactoryTests.cs
```

## 🐛 Dépannage

### Erreur : "La génération a échoué"

```bash
dotnet clean
dotnet restore
dotnet build
```

### MongoDB non accessible

Vérifier que MongoDB est démarré :

```bash
# Linux
sudo systemctl status mongodb

# Démarrer si arrêté
sudo systemctl start mongodb
```

### Port 5000 déjà utilisé

Modifier le fichier `RPGArena.Backend/Properties/launchSettings.json` :

```json
{
  "applicationUrl": "http://localhost:5001"
}
```

## 📊 Base de données MongoDB

### Collection : `CombatRecords`

Stockage automatique des combats avec :
- Participants (nom, type, PV)
- Vainqueur
- Timestamp début/fin
- Logs d'actions

### Exemple de requête

```javascript
// Connexion à MongoDB
use RPGArena

// Tous les combats
db.CombatRecords.find()

// Dernier combat
db.CombatRecords.find().sort({StartTime: -1}).limit(1)
```

## 🤝 Contribution

1. Fork le projet
2. Créer une branche (`git checkout -b feature/amelioration`)
3. Commit (`git commit -m 'feat: ajout nouvelle classe'`)
4. Push (`git push origin feature/amelioration`)
5. Ouvrir une Pull Request

## 📝 Convention de commits

- `feat:` Nouvelle fonctionnalité
- `fix:` Correction de bug
- `refactor:` Refactoring sans changement fonctionnel
- `test:` Ajout/modification de tests
- `docs:` Documentation

## 📜 Licence

MIT

## 👥 Auteurs

Projet développé dans le cadre d'un serious game RPG.

---

**Version** : 1.0.0  
**Framework** : .NET 8.0 LTS  
**Base de données** : MongoDB 4.0+
