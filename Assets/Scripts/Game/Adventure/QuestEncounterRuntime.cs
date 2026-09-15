using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuildAdventure.Game.Monster;

namespace GuildAdventure.Game.Adventure
{
    [Serializable] public sealed class QuestFile { public string schema_version; public QuestRow[] data; }
    [Serializable] public sealed class QuestRow { public string id,name,type,status; public int adventure_duration_seconds; public double enemy_budget,base_enemy_budget; public QuestContext context; public QuestBox[] boxes; }
    [Serializable] public sealed class QuestContext { public string map_id; public string[] environment_tags; public int difficulty; }
    [Serializable] public sealed class QuestBox { public string box_id,name; public int order; public QuestEventSlot[] event_zone_before_pre,event_zone_pre_to_mid,event_zone_mid_to_post,event_zone_after_post; }
    [Serializable] public sealed class QuestEventSlot { public string kind,event_id,failure_policy; public int order; public EncounterOverride encounter_override; }
    [Serializable] public sealed class EncounterOverride { public string mode; public RequiredMonster[] required_monsters,formation; }
    [Serializable] public sealed class RequiredMonster { public string monster_id,formation_position; public int count; }
    public sealed class FormationRow { public string monsterId; public int count; }

    public static class QuestEncounterRuntime
    {
        public static List<QuestRow> Load(string json)
        {
            var f=JsonUtility.FromJson<QuestFile>(json);if(f==null||f.data==null)throw new ArgumentException("QUEST_MASTER_INVALID");
            return f.data.Where(x=>x!=null&&!string.IsNullOrWhiteSpace(x.id)).ToList();
        }

        public static List<FormationRow> ResolveRequired(EncounterOverride o,IReadOnlyDictionary<string,MonsterRow> monsters)
        {
            if(o==null||o.mode!="required_monsters")throw new ArgumentException("ENCOUNTER_OVERRIDE_MODE");
            var result=new List<FormationRow>();
            foreach(var r in o.required_monsters??Array.Empty<RequiredMonster>())
            {
                if(r.count<1||!monsters.ContainsKey(r.monster_id))throw new ArgumentException("REQUIRED_MONSTER_INVALID");
                result.Add(new FormationRow{monsterId=r.monster_id,count=r.count});
            }
            return result;
        }

        public static List<FormationRow> GenerateBudget(double budget,IEnumerable<MonsterRow> master,ISet<string> tags,Func<double> random,int maxUnits)
        {
            if(random==null)throw new ArgumentNullException(nameof(random));
            double remaining=Math.Max(0,budget);var rows=new List<FormationRow>();
            int guard=0;
            while(remaining>0&&guard++<maxUnits)
            {
                var eligible=(master??Array.Empty<MonsterRow>()).Where(m=>MonsterRuntime.SpawnTagsMatch(m,tags)&&m.@params.enemy_budget_cost<=remaining&&m.@params.spawn_weight>0).ToList();
                if(eligible.Count==0)break;
                double total=eligible.Sum(x=>x.@params.spawn_weight),roll=random()*total;MonsterRow chosen=eligible[eligible.Count-1];
                foreach(var m in eligible){roll-=m.@params.spawn_weight;if(roll<0){chosen=m;break;}}
                remaining-=chosen.@params.enemy_budget_cost;
                var found=rows.FirstOrDefault(x=>x.monsterId==chosen.id);if(found==null)rows.Add(new FormationRow{monsterId=chosen.id,count=1});else found.count++;
            }
            return rows;
        }
    }
}
