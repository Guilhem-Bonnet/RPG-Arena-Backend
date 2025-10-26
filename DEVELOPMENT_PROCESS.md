# 🔄 Processus de Développement Agile - RPG Arena

## 📋 Vue d'ensemble

Ce document définit le processus de développement agile pour le projet RPG Arena, avec des checklists de validation pour chaque amélioration.

## 🎯 Cycle de Développement

Chaque amélioration suit ce cycle :

```
1. Planification → 2. Implémentation → 3. Tests Unitaires → 4. Tests Intégration → 5. Validation → 6. Commit Git → 7. Suivant
```

---

## ✅ Checklist Universelle

Cette checklist s'applique à **TOUTES** les améliorations :

### Avant de Commencer
- [ ] Lire et comprendre le ticket/amélioration
- [ ] Vérifier les dépendances avec autres tickets
- [ ] Créer une branche si nécessaire : `git checkout -b feature/nom-amelioration`

### Pendant le Développement
- [ ] Écrire le code avec commentaires clairs
- [ ] Respecter les conventions de nommage C#
- [ ] Écrire les tests unitaires **en parallèle** du code
- [ ] Compiler sans erreurs : `dotnet build`
- [ ] Compiler sans warnings (objectif : 0 warning)

### Tests Unitaires
- [ ] Créer/mettre à jour fichiers de tests dans `RPGArena.Tests/`
- [ ] Minimum 80% de couverture pour le nouveau code
- [ ] Exécuter : `dotnet test --verbosity normal`
- [ ] Tous les tests passent : `100% success`
- [ ] Temps d'exécution raisonnable (< 5s pour tests unitaires)

### Tests d'Intégration
- [ ] Vérifier avec Docker : `./scripts/test-integration.sh`
- [ ] Valider les services : `./scripts/validate.sh`
- [ ] Health checks OK : `curl http://localhost:5000/health`
- [ ] Logs sans erreurs : `docker compose logs backend`

### Documentation
- [ ] Mettre à jour README.md si API publique modifiée
- [ ] Ajouter exemples si nouvelle fonctionnalité
- [ ] Documenter les breaking changes
- [ ] Mettre à jour CHANGELOG.md (si existe)

### Validation Finale
- [ ] Code review (auto-review si solo)
- [ ] Pas de code dupliqué
- [ ] Pas de secrets en dur
- [ ] Performance acceptable
- [ ] Sécurité vérifiée

### Git Commit
- [ ] Stage les fichiers : `git add <fichiers>`
- [ ] Commit avec message descriptif : `git commit -m "type(scope): description"`
- [ ] Push : `git push origin <branche>`
- [ ] Créer PR si workflow collaboratif

---

## 🔴 Haute Priorité

### 1. Corriger Warnings C# (Issue #5)

**Objectif** : Passer de 2 warnings à 0 warning en compilation

**Fichiers concernés** :
- `RPGArena.Backend/Program.cs`

**Problèmes identifiés** :
1. **CS1998** : Méthode async sans opérateur await
2. **CS8618** : Propriété non-nullable `ConnectionStrings` non initialisée

#### Checklist Spécifique

**Planification**
- [ ] Analyser warning CS1998 : Ligne avec `async` inutile
- [ ] Analyser warning CS8618 : Propriété `ConnectionStrings`
- [ ] Déterminer si async nécessaire ou à retirer
- [ ] Déterminer si nullable `?` ou initialisation required

**Implémentation**
- [ ] CS1998 : Retirer `async` ou ajouter `await` si nécessaire
- [ ] CS8618 : Ajouter `?` (nullable) ou `required` ou initialiser dans constructeur
- [ ] Compiler : `dotnet build RPGArena.Backend/RPGArena.Backend.csproj`
- [ ] Vérifier 0 warnings

**Tests**
- [ ] Tests unitaires existants passent : `dotnet test`
- [ ] Créer test pour propriété nullable si applicable
- [ ] Tester Backend en local : `dotnet run --project RPGArena.Backend`
- [ ] Vérifier health check : `curl http://localhost:5000/health`

**Validation Docker**
- [ ] Build Docker : `docker compose build backend`
- [ ] Start services : `./scripts/start.sh dev`
- [ ] Valider : `./scripts/validate.sh`
- [ ] Logs propres : `docker compose logs backend | grep -i error`

**Git**
- [ ] `git add RPGArena.Backend/Program.cs`
- [ ] `git commit -m "fix(backend): Resolve CS1998 and CS8618 warnings"`
- [ ] `git push origin master`

**Critères de Succès** : ✅ 0 warnings en compilation

---

### 2. Résoudre Port HTTPS 5001 (Issue #1)

**Objectif** : Activer HTTPS sur port 5001 pour sécurisation production

**Fichiers concernés** :
- `docker-compose.yml` (ligne 19 commentée)
- `appsettings.json` / `appsettings.Development.json`
- `docker/https/aspnetcore.pfx` (certificat)

#### Checklist Spécifique

**Planification**
- [ ] Identifier processus utilisant port 5001 : `lsof -i :5001` ou `netstat -tulpn | grep 5001`
- [ ] Décider : changer de port ou libérer 5001
- [ ] Vérifier certificat existant : `ls -lh docker/https/aspnetcore.pfx`
- [ ] Vérifier mot de passe certificat dans `.env`

**Implémentation**
- [ ] Libérer port 5001 si occupé
- [ ] Décommenter ligne 19 dans `docker-compose.yml` : `- "5001:5001"`
- [ ] Mettre à jour `appsettings.json` :
  ```json
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:5000" },
      "Https": { "Url": "https://+:5001" }
    }
  }
  ```
- [ ] Vérifier/régénérer certificat : `./scripts/generate-cert.sh`

**Tests**
- [ ] Compiler : `dotnet build`
- [ ] Tests unitaires : `dotnet test`
- [ ] Start local : `dotnet run --project RPGArena.Backend`
- [ ] Tester HTTP : `curl http://localhost:5000/health`
- [ ] Tester HTTPS : `curl -k https://localhost:5001/health`

**Validation Docker**
- [ ] Build : `docker compose build backend`
- [ ] Start : `./scripts/start.sh dev`
- [ ] Valider HTTP : `curl http://localhost:5000/health`
- [ ] Valider HTTPS : `curl -k https://localhost:5001/health`
- [ ] Vérifier certificat : `openssl s_client -connect localhost:5001 -showcerts`
- [ ] Tests intégration : `./scripts/test-integration.sh`

**Documentation**
- [ ] Mettre à jour DOCKER_FULL.md avec section HTTPS
- [ ] Documenter génération certificat production (Let's Encrypt)
- [ ] Ajouter troubleshooting pour erreurs certificat

**Git**
- [ ] `git add docker-compose.yml appsettings.json DOCKER_FULL.md`
- [ ] `git commit -m "feat(security): Enable HTTPS on port 5001 with SSL certificates"`
- [ ] `git push origin master`

**Critères de Succès** : ✅ Port 5001 accessible en HTTPS sans erreur

---

### 3. Externaliser Secrets (Issue #2)

**Objectif** : Éliminer credentials par défaut, exiger secrets externes en production

**Fichiers concernés** :
- `docker-compose.yml`
- `.env.example`
- `RPGArena.Backend/Program.cs` (validation startup)

#### Checklist Spécifique

**Planification**
- [ ] Lister tous les secrets : MONGO_INITDB_ROOT_PASSWORD, ME_CONFIG_BASICAUTH_PASSWORD, etc.
- [ ] Décider stratégie : `.env` obligatoire ou variables d'environnement
- [ ] Documenter intégration Azure KeyVault / AWS Secrets Manager

**Implémentation Phase 1 : Validation**
- [ ] Créer fichier `.env.production.template` avec placeholders
- [ ] Ajouter validation dans `Program.cs` :
  ```csharp
  var mongoPassword = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");
  if (mongoPassword == "rootpassword123" || string.IsNullOrEmpty(mongoPassword))
  {
      throw new InvalidOperationException("Default MongoDB password detected! Set MONGO_INITDB_ROOT_PASSWORD in .env");
  }
  ```
- [ ] Mettre à jour `.gitignore` : Ajouter `.env.production`

**Implémentation Phase 2 : Documentation**
- [ ] Créer `SECURITY.md` avec guide secrets management
- [ ] Documenter Azure KeyVault integration :
  ```bash
  # Exemple Azure CLI
  az keyvault secret set --vault-name myKeyVault --name MongoPassword --value "complex_password"
  ```
- [ ] Documenter AWS Secrets Manager integration
- [ ] Ajouter section dans DOCKER_FULL.md

**Tests**
- [ ] Test avec default password : Doit échouer au démarrage
- [ ] Test avec `.env.production` valide : Doit démarrer
- [ ] Test avec variable d'environnement : `MONGO_INITDB_ROOT_PASSWORD=test123 dotnet run`
- [ ] Vérifier logs : Message clair si secret invalide

**Validation Docker**
- [ ] Start sans `.env` : Doit échouer avec message clair
- [ ] Start avec `.env.production` : OK
- [ ] Tester rotation de credentials : Changer password, vérifier connexion

**Documentation**
- [ ] SECURITY.md créé
- [ ] README.md mis à jour avec lien vers SECURITY.md
- [ ] DOCKER_FULL.md mis à jour

**Git**
- [ ] `git add Program.cs .env.production.template SECURITY.md .gitignore`
- [ ] `git commit -m "feat(security): Enforce external secrets management and prevent default credentials"`
- [ ] `git push origin master`

**Critères de Succès** : ✅ Impossible de démarrer avec credentials par défaut

---

### 4. Désactiver MongoExpress en Production (Issue #3)

**Objectif** : Cacher interface admin en production, accessible uniquement avec profil

**Fichiers concernés** :
- `docker-compose.prod.yml`
- `DOCKER_FULL.md`

#### Checklist Spécifique

**Planification**
- [ ] Vérifier comportement actuel : MongoExpress exposé sur port 8081
- [ ] Décider : profile `admin` ou désactivation complète

**Implémentation**
- [ ] Modifier `docker-compose.prod.yml` :
  ```yaml
  mongo-express:
    profiles:
      - admin
    # Pas de ports exposés par défaut
  ```
- [ ] Ajouter commentaire dans fichier
- [ ] Tester démarrage sans profile : MongoExpress ne doit PAS démarrer
- [ ] Tester démarrage avec profile : `docker compose --profile admin up`

**Tests**
- [ ] Build : `docker compose -f docker-compose.yml -f docker-compose.prod.yml build`
- [ ] Start prod sans profile : `./scripts/start.sh prod`
- [ ] Vérifier MongoExpress absent : `docker compose ps | grep mongo-express` (vide)
- [ ] Port 8081 inaccessible : `curl http://localhost:8081` (échec)
- [ ] Start avec profile : `docker compose --profile admin -f docker-compose.yml -f docker-compose.prod.yml up -d`
- [ ] Vérifier MongoExpress présent : `curl http://localhost:8081` (OK)

**Documentation**
- [ ] Ajouter warning dans DOCKER_FULL.md :
  ```markdown
  ⚠️ **PRODUCTION** : MongoExpress désactivé par défaut.
  Pour activer temporairement : `docker compose --profile admin up`
  ```
- [ ] Mettre à jour README.md section sécurité

**Git**
- [ ] `git add docker-compose.prod.yml DOCKER_FULL.md README.md`
- [ ] `git commit -m "security(production): Disable MongoExpress by default, require admin profile"`
- [ ] `git push origin master`

**Critères de Succès** : ✅ MongoExpress absent en production par défaut

---

## 🟠 Moyenne Priorité

### 5. Étendre Tests Unitaires (Issue #4)

**Objectif** : Passer de 6 tests à 30+ tests, couvrir BattleArena, Characters, Skills

**Fichiers concernés** :
- `RPGArena.Tests/` (nouveau fichiers)

#### Checklist Spécifique

**Planification**
- [ ] Analyser couverture actuelle : Seulement `CharacterFactoryTests.cs`
- [ ] Identifier classes critiques sans tests :
  - `BattleArena.cs`
  - Classes de personnages (`Guerrier.cs`, `Magicien.cs`, etc.)
  - Compétences (`Skills/`)
  - États (`States/`)
- [ ] Prioriser : BattleArena > Characters > Skills > States

**Implémentation Phase 1 : BattleArena**
- [ ] Créer `RPGArena.Tests/BattleArenaTests.cs`
- [ ] Tests à créer (minimum 8 tests) :
  - [ ] `InitializeArena_Should_CreateEmptyBattle`
  - [ ] `AddCharacter_Should_AddToBattleList`
  - [ ] `StartBattle_Should_InitializeTurnOrder`
  - [ ] `ExecuteTurn_Should_ProcessAction`
  - [ ] `CheckVictory_Should_DetectWinner`
  - [ ] `Battle_Should_EndWhenOneTeamDefeated`
  - [ ] `TurnOrder_Should_BeBasedOnSpeed`
  - [ ] `MultipleCharacters_Should_HandleCorrectly`
- [ ] Exécuter : `dotnet test --filter BattleArenaTests`
- [ ] Vérifier tous passent

**Implémentation Phase 2 : Characters**
- [ ] Créer `RPGArena.Tests/CharacterTests.cs`
- [ ] Tests à créer (minimum 10 tests) :
  - [ ] `Guerrier_Should_HaveHighHealth`
  - [ ] `Magicien_Should_HaveHighMana`
  - [ ] `Assassin_Should_HaveHighSpeed`
  - [ ] `Character_TakeDamage_Should_ReduceHealth`
  - [ ] `Character_Heal_Should_IncreaseHealth_NotExceedMax`
  - [ ] `Character_Death_Should_SetIsAlive_False`
  - [ ] `Character_UseSkill_Should_ConsumeMana`
  - [ ] `Character_NotEnoughMana_Should_FailSkill`
  - [ ] `Character_Stats_Should_BePositive`
  - [ ] `Character_SpecialAbility_Should_Work`
- [ ] Exécuter : `dotnet test --filter CharacterTests`

**Implémentation Phase 3 : Skills**
- [ ] Créer `RPGArena.Tests/SkillTests.cs`
- [ ] Tests à créer (minimum 8 tests) :
  - [ ] `AttackSkill_Should_DealDamage`
  - [ ] `HealSkill_Should_RestoreHealth`
  - [ ] `BuffSkill_Should_IncreaseStats`
  - [ ] `DebuffSkill_Should_DecreaseStats`
  - [ ] `AOESkill_Should_AffectMultipleTargets`
  - [ ] `SkillCooldown_Should_PreventImmediateReuse`
  - [ ] `CriticalHit_Should_DoubleDamage`
  - [ ] `Miss_Should_DealZeroDamage`
- [ ] Exécuter : `dotnet test --filter SkillTests`

**Implémentation Phase 4 : States**
- [ ] Créer `RPGArena.Tests/StateTests.cs` (optionnel si time permet)
- [ ] Tests états : Poison, Stun, Shield, Regen, etc.

**Tests Globaux**
- [ ] Exécuter tous les tests : `dotnet test --verbosity normal`
- [ ] Vérifier total >= 30 tests
- [ ] Vérifier 100% succès
- [ ] Temps exécution < 10s
- [ ] Générer rapport couverture : `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Vérifier couverture >= 60% globale (objectif 80%+)

**Documentation**
- [ ] Mettre à jour TEST_RESULTS.md avec nouveaux tests
- [ ] Ajouter section "Comment écrire des tests" dans README.md

**Git (commit par phase)**
- [ ] Phase 1 : `git commit -m "test(battlearena): Add 8 unit tests for BattleArena class"`
- [ ] Phase 2 : `git commit -m "test(characters): Add 10 unit tests for character classes"`
- [ ] Phase 3 : `git commit -m "test(skills): Add 8 unit tests for skill system"`
- [ ] Push final : `git push origin master`

**Critères de Succès** : ✅ 30+ tests, 100% succès, couverture >= 60%

---

## 🟡 Basse Priorité

### 6. Configurer CI/CD Pipeline (Issue #8)

**Objectif** : Automatiser build, tests, Docker build, scan sécurité avec GitHub Actions

**Fichiers concernés** :
- `.github/workflows/dotnet.yml` (à créer)

#### Checklist Spécifique

**Planification**
- [ ] Décider stratégie branches : `main` / `develop` / feature branches
- [ ] Lister steps pipeline : build → test → docker → security scan → deploy
- [ ] Choisir registry Docker : Docker Hub, GitHub Container Registry, ou autre

**Implémentation**
- [ ] Créer `.github/workflows/dotnet.yml` :
  ```yaml
  name: .NET Build and Test
  
  on:
    push:
      branches: [ master, develop ]
    pull_request:
      branches: [ master ]
  
  jobs:
    build:
      runs-on: ubuntu-latest
      
      steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET 9
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Test
        run: dotnet test --no-build --verbosity normal --configuration Release
      
      - name: Docker Build
        run: docker build -t rpgarena-backend:${{ github.sha }} .
      
      - name: Security Scan with Trivy
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: rpgarena-backend:${{ github.sha }}
          format: 'sarif'
          output: 'trivy-results.sarif'
      
      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: 'trivy-results.sarif'
  ```

**Tests**
- [ ] Push vers GitHub : Déclenche workflow
- [ ] Vérifier exécution : GitHub Actions tab
- [ ] Vérifier chaque step passe
- [ ] Vérifier durée totale < 5 minutes
- [ ] Tester avec échec intentionnel (test qui fail) : Workflow doit échouer

**Documentation**
- [ ] Créer `CI_CD.md` avec explication workflow
- [ ] Ajouter badge dans README.md : `![CI](https://github.com/.../workflows/.NET%20Build%20and%20Test/badge.svg)`
- [ ] Documenter comment ajouter secrets GitHub (DOCKER_USERNAME, etc.)

**Git**
- [ ] `git add .github/workflows/dotnet.yml CI_CD.md README.md`
- [ ] `git commit -m "ci(github-actions): Add CI/CD pipeline with build, test, docker, and security scan"`
- [ ] `git push origin master`

**Critères de Succès** : ✅ Pipeline exécuté automatiquement sur chaque push

---

## 📊 Métriques de Qualité

Objectifs globaux du projet :

| Métrique | Objectif | Actuel |
|----------|----------|--------|
| **Erreurs de compilation** | 0 | ✅ 0 |
| **Warnings de compilation** | 0 | ⚠️ 2 |
| **Tests unitaires** | 30+ | 🔴 6 |
| **Succès tests** | 100% | ✅ 100% |
| **Couverture de code** | 80%+ | 🔴 ~20% |
| **Build time** | < 5s | ✅ 2s |
| **Test time** | < 10s | ✅ 0.6s |
| **Docker build time** | < 2min | ✅ ~1min |
| **Vulnérabilités sécurité** | 0 critical | ⏳ TBD |

---

## 🔄 Cycle de Release

### Semantic Versioning

Format : `MAJOR.MINOR.PATCH` (ex: `1.2.3`)

- **MAJOR** : Breaking changes (incompatibilité API)
- **MINOR** : Nouvelles fonctionnalités (rétrocompatible)
- **PATCH** : Corrections de bugs

### Types de Commits

Suivre [Conventional Commits](https://www.conventionalcommits.org/) :

- `feat(scope): description` - Nouvelle fonctionnalité
- `fix(scope): description` - Correction de bug
- `docs(scope): description` - Documentation seule
- `style(scope): description` - Formatage (pas de changement code)
- `refactor(scope): description` - Refactoring (pas de nouvelle feature)
- `test(scope): description` - Ajout/modification tests
- `chore(scope): description` - Maintenance (build, deps, etc.)
- `perf(scope): description` - Amélioration performance
- `ci(scope): description` - CI/CD
- `security(scope): description` - Correctifs sécurité

**Exemples** :
```bash
git commit -m "feat(backend): Add WebSocket reconnection with exponential backoff"
git commit -m "fix(combat): Resolve initiative calculation for tied speeds"
git commit -m "test(characters): Add unit tests for Vampire life steal ability"
git commit -m "security(docker): Enforce non-default credentials for MongoDB"
```

---

## 🛠️ Outils Recommandés

### Analyse de Code
- **SonarQube/SonarCloud** : Analyse qualité et sécurité
- **dotnet format** : Formatage automatique
- **ReSharper** : Analyse statique avancée (si disponible)

### Tests
- **xUnit** : Framework tests (déjà intégré)
- **Moq** : Mocking pour tests unitaires
- **Coverlet** : Couverture de code
- **k6** ou **Artillery** : Tests de charge

### Sécurité
- **Trivy** : Scan vulnérabilités Docker
- **OWASP Dependency-Check** : Scan dépendances
- **dotnet list package --vulnerable** : Vérification packages

### Monitoring
- **Aspire Dashboard** : Monitoring intégré .NET Aspire
- **Seq** : Agrégation logs
- **Prometheus + Grafana** : Métriques et alertes

---

## 📝 Conclusion

Ce processus garantit :
- ✅ Code de qualité avec 0 erreur/warning
- ✅ Tests complets et maintenables
- ✅ Sécurité renforcée
- ✅ Documentation à jour
- ✅ Historique Git propre et traçable

**Rappel** : Toujours exécuter la checklist universelle + la checklist spécifique pour chaque amélioration.

---

**Dernière mise à jour** : 25 octobre 2025  
**Version** : 1.0.0
