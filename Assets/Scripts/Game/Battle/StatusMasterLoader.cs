using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.Battle
{
    [Serializable] public sealed class StatusMasterFile { public string schema_version,data_version; public StatusMasterRow[] data; }
    [Serializable] public sealed class StatusMasterRow {
        public string id,name,status,description,lifecycle_kind,stack_policy,dispel_category;
        public int max_stacks,duration;
        public double resistance_cap_percent=-1d;
        public bool removable=true,protected_effect,normal_cleanse_eligible,action_disabled;
        public string[] tags;
    }

    public static class StatusMasterLoader
    {
        public static Dictionary<string,StatusMasterRow> Load(string json)
        {
            if(string.IsNullOrWhiteSpace(json))throw new ArgumentException("statuses.json empty");
            var f=JsonUtility.FromJson<StatusMasterFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("statuses.json data missing");
            var result=new Dictionary<string,StatusMasterRow>(StringComparer.Ordinal);
            foreach(var row in f.data)
            {
                if(row==null||string.IsNullOrWhiteSpace(row.id)||row.status=="disabled")continue;
                if(result.ContainsKey(row.id))throw new ArgumentException("STATUS_ID_DUPLICATE:"+row.id);
                if(!Enum.TryParse((row.lifecycle_kind??"").Trim(),true,out EffectLifecycleKind kind))throw new ArgumentException("STATUS_LIFECYCLE_KIND_INVALID:"+row.id);
                if(kind==EffectLifecycleKind.STATUS&&row.resistance_cap_percent<0)throw new ArgumentException("STATUS_RESISTANCE_CAP_MISSING:"+row.id);
                if((kind==EffectLifecycleKind.DOT||kind==EffectLifecycleKind.BUFF||kind==EffectLifecycleKind.DEBUFF)&&row.max_stacks<=0)throw new ArgumentException("STATUS_MAX_STACKS_MISSING:"+row.id);
                result[row.id]=row;
            }
            return result;
        }
    }
}
