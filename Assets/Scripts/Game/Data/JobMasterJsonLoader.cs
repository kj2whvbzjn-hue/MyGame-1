using System;
using System.Collections.Generic;
using UnityEngine;
using GuildAdventure.Game.Character;

namespace GuildAdventure.Game.Data
{
    [Serializable] public sealed class JobMasterFile { public string schema_version; public string data_version; public JobMasterRow[] data; }
    [Serializable] public sealed class JobMasterRow { public string id; public string name; public string status; public JobMasterParams @params; }
    [Serializable] public sealed class JobMasterParams { public JobMasterAptitudes aptitudes; }
    [Serializable] public sealed class JobMasterAptitudes { public int STR,VIT,AGI,DEX,INT,MND,LUK; }

    public static class JobMasterJsonLoader
    {
        public static JobCatalog LoadCatalog(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("jobs.json is empty.");
            var file=JsonUtility.FromJson<JobMasterFile>(json);
            if (file==null || file.data==null) throw new ArgumentException("jobs.json data is missing.");
            var jobs=new List<JobDefinition>();
            foreach(var row in file.data)
            {
                if(row==null || row.status!="active" || row.@params?.aptitudes==null) continue;
                var a=row.@params.aptitudes;
                jobs.Add(new JobDefinition {
                    id=row.id,name=row.name,
                    growth=new JobGrowth{STR=a.STR,VIT=a.VIT,AGI=a.AGI,DEX=a.DEX,INT=a.INT,MND=a.MND,LUK=a.LUK}
                });
            }
            return new JobCatalog(jobs);
        }
    }
}
