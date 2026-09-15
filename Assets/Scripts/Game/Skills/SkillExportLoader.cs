using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.Skills
{
    [Serializable] public sealed class SkillExportFile { public string schema_version,data_version; public SkillExportRow[] data; }
    [Serializable] public sealed class SkillExportRow {
        public int schemaVersion,skillLevel; public string id,name,status,description;
        public SkillAbilityCondition[] abilityConditions; public SkillTrigger trigger; public SkillTarget target;
        public SkillEffect[] effects; public SkillResource resource;
    }
    [Serializable] public sealed class SkillAbilityCondition { public string stat; public int min; }
    [Serializable] public sealed class SkillTrigger { public string type,scope; }
    [Serializable] public sealed class SkillTarget { public string side,range; public int randomCount; public bool excludeSelf; }
    [Serializable] public sealed class SkillResource { public int mpCost,cooldown,activationPriority,castTime; }
    [Serializable] public sealed class SkillEffect {
        public string type,damageType,kind,statusId,property;
        public double power,value; public int duration; public SkillElement[] elements;
    }
    [Serializable] public sealed class SkillElement { public string element; public double sharePercent; }

    public static class SkillExportLoader
    {
        public static List<SkillExportRow> Load(string json)
        {
            if(string.IsNullOrWhiteSpace(json))throw new ArgumentException("skills.json empty");
            var f=JsonUtility.FromJson<SkillExportFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("skills.json data missing");
            var result=new List<SkillExportRow>();
            foreach(var row in f.data)
            {
                if(row==null||row.schemaVersion!=1||string.IsNullOrWhiteSpace(row.id))continue;
                result.Add(row);
            }
            return result;
        }

        public static SkillDefinition ToRuntime(SkillExportRow row)
        {
            if(row==null)throw new ArgumentNullException(nameof(row));
            var target=SkillTargetKind.ENEMY;
            if(row.target!=null)
            {
                if(row.target.side=="SELF")target=SkillTargetKind.SELF;
                else if(row.target.side=="ALLY")target=row.target.range=="ALL"?SkillTargetKind.ALL_ALLIES:SkillTargetKind.ALLY;
                else if(row.target.side=="ENEMY")target=row.target.range=="ALL"?SkillTargetKind.ALL_ENEMIES:SkillTargetKind.ENEMY;
            }
            return new SkillDefinition{
                id=row.id,name=row.name,target=target,
                resource=SkillResourceKind.MP,resourceCost=row.resource?.mpCost??0,
                castTicks=row.resource?.castTime??0,cooldownTicks=row.resource?.cooldown??0,
                enabled=true
            };
        }
    }
}
