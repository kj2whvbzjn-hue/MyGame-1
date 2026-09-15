using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuildAdventure.Game.Reward
{
    [Serializable] public sealed class DropTableFile { public string schema_version; public DropTableRow[] data; }
    [Serializable] public sealed class DropTableRow { public string id,status; public DropEntry[] entries; }
    [Serializable] public sealed class DropEntry { public string kind,ref_id; public int weight=1,min=1,max=1; }
    public sealed class RewardItem { public string kind,refId; public int amount; }

    public static class RewardDropRuntime
    {
        public static Dictionary<string,DropTableRow> Load(string json)
        {
            var f=JsonUtility.FromJson<DropTableFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("DROP_TABLE_INVALID");
            return f.data.Where(x=>x!=null&&!string.IsNullOrWhiteSpace(x.id)&&x.status!="disabled")
                .ToDictionary(x=>x.id,x=>x);
        }

        public static RewardItem Roll(DropTableRow table,Func<double> random)
        {
            if(table==null||random==null)throw new ArgumentNullException();
            var rows=(table.entries??Array.Empty<DropEntry>()).Where(x=>x.weight>0).ToList();
            if(rows.Count==0)return null;
            int total=rows.Sum(x=>x.weight);double roll=Math.Max(0,Math.Min(.999999999,random()))*total;
            var picked=rows[rows.Count-1];foreach(var x in rows){roll-=x.weight;if(roll<0){picked=x;break;}}
            int lo=Math.Min(picked.min,picked.max),hi=Math.Max(picked.min,picked.max);
            int amount=lo+(int)Math.Floor(Math.Max(0,Math.Min(.999999999,random()))*(hi-lo+1));
            return new RewardItem{kind=picked.kind,refId=picked.ref_id,amount=amount};
        }
    }
}
