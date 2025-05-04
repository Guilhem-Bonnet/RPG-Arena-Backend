using MongoDB.Driver;
using RPGArena.Backend.Models;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.Backend.Repositories;

public class MongoCombatRepository : ICombatRepository
{
    private readonly IMongoCollection<CombatRecord> _collection;

    public MongoCombatRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CombatRecord>("CombatRecords");
    }

    public async Task SaveCombatResult(List<ICharacter> participants)
    {
        var survivors = participants.Where(p => p.Life > 0).ToList();

        var record = new CombatRecord
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            Participants = participants.Select(p => new CombatantInfo
            {
                Name = p.Name,
                Type = p.TypeDuPersonnage,
                Life = p.Life,
                IsDead = p.IsDead,
                IsEatable = p.IsEatable
            }).ToList(),
            Winner = survivors.Count == 1 ? survivors[0].Name : null,
            Outcome = ResolveOutcome(survivors)
        };

        await _collection.InsertOneAsync(record);
    }

    private static string ResolveOutcome(List<ICharacter> survivors)
    {
        if (survivors.Count == 0) return "Tous les combattants sont morts";
        if (survivors.All(s => s.TypeDuPersonnage == TypePersonnage.MortVivant)) return "MortVivants seuls survivants";
        if (survivors.All(s => s.TypeDuPersonnage == TypePersonnage.Humain)) return "Humains ont gagné";
        return "Survivants mixtes";
    }

    // --- Interface complète ---
    public async Task<string> CreateCombatAsync(CombatRecord record)
    {
        await _collection.InsertOneAsync(record);
        return record.Id;
    }

    public async Task<IEnumerable<CombatRecord>> GetAllCombatsAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<CombatRecord?> GetCombatByIdAsync(string id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateCombatAsync(string id, CombatRecord updated)
    {
        await _collection.ReplaceOneAsync(r => r.Id == id, updated);
    }

    public async Task DeleteCombatAsync(string id)
    {
        await _collection.DeleteOneAsync(r => r.Id == id);
    }
}
