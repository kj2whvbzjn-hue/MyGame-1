using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    [Serializable]
    public sealed class PassiveContribution
    {
        public string passiveId,seriesId,property;
        public double value;
        public string capability;
    }

    public sealed class PassiveCompileResult
    {
        public bool ok; public string reason;
        public List<PassiveContribution> contributions=new List<PassiveContribution>();
        public HashSet<string> capabilities=new HashSet<string>();
    }

    public static class PassiveRuntime
    {
        public static PassiveCompileResult Compile(IEnumerable<PassiveContribution> equipped)
        {
            var rows=(equipped??Array.Empty<PassiveContribution>()).ToList();
            var duplicateSeries=rows.Where(x=>!string.IsNullOrWhiteSpace(x.seriesId))
                .GroupBy(x=>x.seriesId).FirstOrDefault(g=>g.Count()>1);
            if(duplicateSeries!=null)
                return new PassiveCompileResult{ok=false,reason="PASSIVE_SERIES_DUPLICATE:"+duplicateSeries.Key};

            var r=new PassiveCompileResult{ok=true,contributions=rows};
            foreach(var x in rows)
                if(!string.IsNullOrWhiteSpace(x.capability))r.capabilities.Add(x.capability);
            return r;
        }

        public static double SumProperty(PassiveCompileResult compiled,string property)
            => compiled==null||!compiled.ok?0:
               compiled.contributions.Where(x=>x.property==property).Sum(x=>x.value);
    }
}
