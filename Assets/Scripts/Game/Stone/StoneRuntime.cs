using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.Stone
{
    [Serializable] public sealed class StoneFile { public string schema_version; public StoneRow[] data; }
    [Serializable] public sealed class StoneRow { public string id,name,status,rarity; public string[] tags,mod_ids; public StoneParams @params; }
    [Serializable] public sealed class StoneParams { public int item_level; public double value; }

    public static class StoneRuntime
    {
        public static Dictionary<string,StoneRow> Load(string json)
        {
            var f=JsonUtility.FromJson<StoneFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("STONE_MASTER_INVALID");
            var r=new Dictionary<string,StoneRow>();
            foreach(var s in f.data){
                if(s==null||string.IsNullOrWhiteSpace(s.id)||s.status=="disabled")continue;
                if(r.ContainsKey(s.id))throw new ArgumentException("STONE_ID_DUPLICATE");
                r[s.id]=s;
            }
            return r;
        }
    }
}
