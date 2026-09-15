using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Save
{
    [Serializable] public sealed class CharacterSaveRecord
    {
        public string characterId,name,currentJobId;
        public int level=1,skillPoints;
        public CharacterStatsSaveRecord stats=new CharacterStatsSaveRecord();
        public List<GrowthHistorySaveRecord> growthHistory=new List<GrowthHistorySaveRecord>();
        public bool alive=true;
        public List<string> learnedSkillIds=new List<string>();
        public List<string> passiveIds=new List<string>();
        public string aiProgramId;
        public EquipmentLoadoutSaveRecord equipment=new EquipmentLoadoutSaveRecord();
    }

    [Serializable] public sealed class EquipmentLoadoutSaveRecord
    {
        public string weaponStyle="single";
        public string weapon1,weapon2,head,armor,gloves,feet,amulet,ring1,ring2,belt;
    }

    [Serializable] public sealed class EquipmentInstanceSaveRecord
    {
        public string instanceId,equipmentId,ownerId;
    }

    [Serializable] public sealed class RuntimeDomainState
    {
        public List<CharacterSaveRecord> characters=new List<CharacterSaveRecord>();
        public List<EquipmentInstanceSaveRecord> equipmentInstances=new List<EquipmentInstanceSaveRecord>();
    }

    public static class RuntimeDomainValidation
    {
        public static string Validate(RuntimeDomainState s)
        {
            if(s==null)return "RUNTIME_DOMAIN_MISSING";
            var chars=new HashSet<string>();
            foreach(var c in s.characters??new List<CharacterSaveRecord>())
            {
                if(c==null||string.IsNullOrWhiteSpace(c.characterId)||!chars.Add(c.characterId))return "CHARACTER_ID_INVALID_OR_DUPLICATE";
                if(c.level<1)return "CHARACTER_LEVEL_INVALID";
            }
            var instances=new HashSet<string>();
            foreach(var i in s.equipmentInstances??new List<EquipmentInstanceSaveRecord>())
            {
                if(i==null||string.IsNullOrWhiteSpace(i.instanceId)||!instances.Add(i.instanceId))return "EQUIPMENT_INSTANCE_INVALID_OR_DUPLICATE";
                if(!string.IsNullOrWhiteSpace(i.ownerId)&&!chars.Contains(i.ownerId))return "EQUIPMENT_OWNER_UNKNOWN";
            }
            return null;
        }
    }
}
