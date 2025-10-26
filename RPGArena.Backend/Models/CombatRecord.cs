// RPGArena.Backend/Models/CombatRecord.cs
using RPGArena.CombatEngine.Enums;
using System;
using System.Collections.Generic;

namespace RPGArena.Backend.Models
{
    public class CombatRecord
    {
        public string Id { get; set; } = string.Empty; // MongoDB's _id
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        public List<CombatantInfo> Participants { get; set; } = new();
        public string? Winner { get; set; }
        public string? Outcome { get; set; } // e.g., "Humains ont gagné", "MortVivants seuls survivants", etc.
    }

    public class CombatantInfo
    {
        public string Name { get; set; } = default!;
        public TypePersonnage Type { get; set; }
        public int Life { get; set; }
        public bool IsDead { get; set; }
        public bool IsEatable { get; set; }
    }
}