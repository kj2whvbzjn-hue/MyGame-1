using System;

namespace GuildAdventure.Game.Equipment
{
    public enum EquipmentRarity { NORMAL, MAGIC, RARE, UNIQUE, LEGENDARY, MYTHIC }
    public enum ModSlotKind { ATTACK, DEFENSE, ABILITY }

    [Serializable] public sealed class EquipmentModDefinition {
        public string id,schema_version,balance_ref,category;
        public ModSlotKind slot_kind;
        public string[] required_tags,any_tags,forbidden_tags;
    }

    public static class ModRarityContracts
    {
        public static bool IsNormalRouteBlocked(EquipmentRarity rarity)
            => rarity==EquipmentRarity.UNIQUE || rarity==EquipmentRarity.LEGENDARY || rarity==EquipmentRarity.MYTHIC;

        public static bool TagsCompatible(EquipmentModDefinition mod,string[] itemTags)
        {
            if(mod==null) return false;
            var set=new System.Collections.Generic.HashSet<string>(itemTags??Array.Empty<string>());
            foreach(var t in mod.required_tags??Array.Empty<string>()) if(!set.Contains(t)) return false;
            var any=mod.any_tags??Array.Empty<string>();
            if(any.Length>0){bool hit=false;foreach(var t in any)if(set.Contains(t)){hit=true;break;}if(!hit)return false;}
            foreach(var t in mod.forbidden_tags??Array.Empty<string>()) if(set.Contains(t)) return false;
            return true;
        }
    }
}
