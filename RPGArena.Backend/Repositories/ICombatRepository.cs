// RPGArena.Backend/Repositories/ICombatRepository.cs

using RPGArena.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Characters;


namespace RPGArena.Backend.Repositories
{
    public interface ICombatRepository
    {
        Task SaveCombatResult(List<ICharacter> participants);
        Task<string> CreateCombatAsync(CombatRecord record);
        Task<CombatRecord?> GetCombatByIdAsync(string id);
        Task<IEnumerable<CombatRecord>> GetAllCombatsAsync();
        Task UpdateCombatAsync(string id, CombatRecord updatedRecord);
        Task DeleteCombatAsync(string id);
    }
}