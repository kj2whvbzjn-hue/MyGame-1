using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Battle
{
    public enum CombatCapability { DUAL_WIELD }

    public static class CombatCapabilityResolver
    {
        public static HashSet<CombatCapability> Normalize(IEnumerable<string> values)
        {
            var set=new HashSet<CombatCapability>();
            if(values==null)return set;
            foreach(var raw in values)
            {
                if(!Enum.TryParse((raw??"").Trim(),true,out CombatCapability cap))
                    throw new ArgumentException("Unknown combat capability: "+raw);
                if(!set.Add(cap)) throw new ArgumentException("Duplicate combat capability: "+raw);
            }
            return set;
        }
    }
}
