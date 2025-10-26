// ═══════════════════════════════════════════════════════════════
// Script d'initialisation MongoDB - RPG Arena
// Exécuté automatiquement au premier démarrage du container
// ═══════════════════════════════════════════════════════════════

print('🚀 Initialisation de la base de données RPGArena...');

// Switch vers la base RPGArena
db = db.getSiblingDB('RPGArena');

// ───────────────────────────────────────────────────────────────
// 👤 Création de l'utilisateur applicatif
// ───────────────────────────────────────────────────────────────
db.createUser({
  user: 'rpgarena_user',
  pwd: 'rpgarena_pass',
  roles: [
    {
      role: 'readWrite',
      db: 'RPGArena'
    },
    {
      role: 'dbAdmin',
      db: 'RPGArena'
    },
    {
      role: 'backup',
      db: 'admin'
    }
  ]
});

print('✅ Utilisateur rpgarena_user créé avec succès');

// ───────────────────────────────────────────────────────────────
// 📊 Création des collections
// ───────────────────────────────────────────────────────────────

// Collection des combats
db.createCollection('combats', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['startTime', 'participants'],
      properties: {
        startTime: {
          bsonType: 'date',
          description: 'Date de début du combat'
        },
        endTime: {
          bsonType: ['date', 'null'],
          description: 'Date de fin du combat'
        },
        participants: {
          bsonType: 'array',
          description: 'Liste des participants',
          items: {
            bsonType: 'object',
            required: ['name', 'type'],
            properties: {
              name: { bsonType: 'string' },
              type: { bsonType: 'string' },
              life: { bsonType: ['int', 'double'] },
              isDead: { bsonType: 'bool' },
              isEatable: { bsonType: 'bool' }
            }
          }
        },
        winner: {
          bsonType: ['string', 'null'],
          description: 'Nom du gagnant'
        },
        outcome: {
          bsonType: 'string',
          description: 'Résultat du combat'
        }
      }
    }
  }
});

print('✅ Collection "combats" créée avec validation');

// Collection des logs
db.createCollection('combat_logs', {
  timeseries: {
    timeField: 'timestamp',
    metaField: 'combatId',
    granularity: 'seconds'
  }
});

print('✅ Collection "combat_logs" créée (TimeSeries)');

// Collection des statistiques
db.createCollection('statistics');
print('✅ Collection "statistics" créée');

// ───────────────────────────────────────────────────────────────
// 🔍 Création des index
// ───────────────────────────────────────────────────────────────

// Index pour les combats
db.combats.createIndex({ startTime: -1 }, { name: 'idx_startTime' });
db.combats.createIndex({ winner: 1 }, { name: 'idx_winner' });
db.combats.createIndex({ 'participants.name': 1 }, { name: 'idx_participants_name' });
db.combats.createIndex({ outcome: 1 }, { name: 'idx_outcome' });

print('✅ Index créés sur "combats"');

// Index pour les logs
db.combat_logs.createIndex({ timestamp: -1 }, { name: 'idx_logs_timestamp' });

print('✅ Index créés sur "combat_logs"');

// ───────────────────────────────────────────────────────────────
// 📝 Insertion de données de test (optionnel)
// ───────────────────────────────────────────────────────────────

const testCombats = [
  {
    startTime: new Date('2025-10-25T10:00:00Z'),
    endTime: new Date('2025-10-25T10:05:30Z'),
    participants: [
      { name: 'Guerrier Test', type: 'Guerrier', life: 0, isDead: true, isEatable: false },
      { name: 'Magicien Test', type: 'Magicien', life: 75, isDead: false, isEatable: false }
    ],
    winner: 'Magicien Test',
    outcome: 'Victoire du Magicien'
  },
  {
    startTime: new Date('2025-10-25T11:00:00Z'),
    endTime: new Date('2025-10-25T11:08:15Z'),
    participants: [
      { name: 'Assassin Test', type: 'Assassin', life: 50, isDead: false, isEatable: false },
      { name: 'Paladin Test', type: 'Paladin', life: 0, isDead: true, isEatable: false }
    ],
    winner: 'Assassin Test',
    outcome: 'Victoire de l\'Assassin'
  }
];

db.combats.insertMany(testCombats);

print('✅ Données de test insérées');

// ───────────────────────────────────────────────────────────────
// 📊 Statistiques initiales
// ───────────────────────────────────────────────────────────────

db.statistics.insertOne({
  _id: 'global_stats',
  totalCombats: 2,
  totalCharactersCreated: 4,
  lastUpdated: new Date(),
  characterTypeStats: {
    Guerrier: { played: 1, wins: 0, losses: 1 },
    Magicien: { played: 1, wins: 1, losses: 0 },
    Assassin: { played: 1, wins: 1, losses: 0 },
    Paladin: { played: 1, wins: 0, losses: 1 }
  }
});

print('✅ Statistiques initiales créées');

// ───────────────────────────────────────────────────────────────
// 🎯 Vérification finale
// ───────────────────────────────────────────────────────────────

print('\n📋 État de la base de données :');
print('   - Combats : ' + db.combats.countDocuments());
print('   - Logs : ' + db.combat_logs.countDocuments());
print('   - Statistiques : ' + db.statistics.countDocuments());

print('\n🎉 Initialisation terminée avec succès !');
