using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Character
{
    [Serializable]
    public sealed class JobGrowth
    {
        public int STR, VIT, AGI, DEX, INT, MND, LUK;

        public Dictionary<CharacterStat, int> ToDictionary() => new Dictionary<CharacterStat, int>
        {
            { CharacterStat.STR, STR }, { CharacterStat.VIT, VIT }, { CharacterStat.AGI, AGI },
            { CharacterStat.DEX, DEX }, { CharacterStat.INT, INT }, { CharacterStat.MND, MND },
            { CharacterStat.LUK, LUK }
        };

        public int Total => STR + VIT + AGI + DEX + INT + MND + LUK;
    }

    [Serializable]
    public sealed class JobDefinition
    {
        public string id;
        public string name;
        public JobGrowth growth;
    }
}
