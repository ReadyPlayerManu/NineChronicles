using Nekoyume.EnumType;
using System.Collections.Generic;

namespace Nekoyume.Game
{
    public interface IStats
    {
        int HP { get; }
        int ATK { get; }
        int DEF { get; }
        int CRI { get; }
        int DOG { get; }
        int SPD { get; }
        
        bool HasHP { get; }
        bool HasATK { get; }
        bool HasDEF { get; }
        bool HasCRI { get; }
        bool HasDOG { get; }
        bool HasSPD { get; }

        IEnumerable<(StatType statType, int value)> GetStats(bool ignoreZero = false);
    }
}
