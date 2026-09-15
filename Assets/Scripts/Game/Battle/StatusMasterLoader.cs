using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.Battle
{
    [Serializable] public sealed class StatusMasterFile { public string schema_version,data_version; public StatusMasterRow[] data; }
    [Serializable] public sealed class StatusMasterRow {
        public string id,name,status,description,stack_policy;
        public int max_stacks,duration;
        public string[] tags;
    }

    public static class StatusMasterLoader
    {
        public static Dictionary<string,StatusMasterRow> Load(string json)
        {
            if(string.IsNullOrWhiteSpace(json))throw new ArgumentException("statuses.json empty");
            var f=JsonUtility.FromJson<StatusMasterFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("statuses.json data missing");
            var result=new Dictionary<string,StatusMasterRow>();
            foreach(var row in f.data)
            {
                if(row==null||string.IsNullOrWhiteSpace(row.id)||row.status=="disabled")continue;
                if(result.ContainsKey(row.id))throw new ArgumentException("STATUS_ID_DUPLICATE:"+row.id);
                result[row.id]=row;
            }
            return result;
        }
    }
}
