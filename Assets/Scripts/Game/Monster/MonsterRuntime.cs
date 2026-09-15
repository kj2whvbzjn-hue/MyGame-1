using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.Monster
{
    [Serializable] public sealed class MonsterFile { public string schema_version; public MonsterRow[] data; }
    [Serializable] public sealed class MonsterRow { public string id,name,status,default_formation_position; public bool enabled=true; public MonsterParams @params; public MonsterAiBinding formalAiBinding; }
    [Serializable] public sealed class MonsterAiBinding { public string program_id,layout_id; }
    [Serializable] public sealed class MonsterParams {
        public double maxHp,attack,agi,str,vit,dex,intel,mnd,luk,accuracy,enemy_budget_cost,spawn_weight;
        public string job_id; public int level; public double maxMp; public SpawnTags spawn_tags; public string[] drop_table_ids;
    }
    [Serializable] public sealed class SpawnTags { public string[] any,all,none; }

    public static class MonsterRuntime
    {
        public static Dictionary<string,MonsterRow> Load(string json)
        {
            var f=JsonUtility.FromJson<MonsterFile>(json);if(f==null||f.data==null)throw new ArgumentException("MONSTER_MASTER_INVALID");
            var r=new Dictionary<string,MonsterRow>();
            foreach(var m in f.data){if(m==null||m.status!="active"||!m.enabled)continue;if(string.IsNullOrWhiteSpace(m.id)||m.@params==null)continue;if(m.@params.enemy_budget_cost<=0)throw new ArgumentException("MONSTER_BUDGET_INVALID");r[m.id]=m;}
            return r;
        }

        public static bool SpawnTagsMatch(MonsterRow m,ISet<string> tags)
        {
            var s=m?.@params?.spawn_tags;if(s==null)return true;tags=tags??new HashSet<string>();
            foreach(var x in s.none??Array.Empty<string>())if(tags.Contains(x))return false;
            foreach(var x in s.all??Array.Empty<string>())if(!tags.Contains(x))return false;
            if((s.any?.Length??0)>0){bool hit=false;foreach(var x in s.any)if(tags.Contains(x)){hit=true;break;}if(!hit)return false;}
            return true;
        }
    }
}
