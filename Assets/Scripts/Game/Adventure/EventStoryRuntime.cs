using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuildAdventure.Game.Adventure
{
    [Serializable] public sealed class EventMasterFile { public string schema_version; public EventRow[] data; }
    [Serializable] public sealed class EventRow { public string id,name,status,type; public string[] tags; public EventOutcome[] outcomes; }
    [Serializable] public sealed class EventOutcome { public string id,result; public int weight=1; }

    public static class EventStoryRuntime
    {
        public static Dictionary<string,EventRow> LoadEvents(string json)
        {
            var f=JsonUtility.FromJson<EventMasterFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("EVENT_MASTER_INVALID");
            var r=new Dictionary<string,EventRow>();
            foreach(var e in f.data){
                if(e==null||string.IsNullOrWhiteSpace(e.id)||e.status=="disabled")continue;
                if(r.ContainsKey(e.id))throw new ArgumentException("EVENT_ID_DUPLICATE");
                r[e.id]=e;
            }
            return r;
        }

        public static EventOutcome PickOutcome(EventRow e,Func<double> random)
        {
            if(e==null||random==null)throw new ArgumentNullException();
            var rows=(e.outcomes??Array.Empty<EventOutcome>()).Where(x=>x!=null&&x.weight>0).ToList();
            if(rows.Count==0)return null;
            int total=rows.Sum(x=>x.weight); double roll=Math.Max(0,Math.Min(.999999999,random()))*total;
            foreach(var x in rows){roll-=x.weight;if(roll<0)return x;}
            return rows[rows.Count-1];
        }
    }
}
