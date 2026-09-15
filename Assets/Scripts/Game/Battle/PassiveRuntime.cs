using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;

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
        public const int MaxPassiveSlots=5;
        public const int PeriodicRecoveryIntervalTicks=20;
        public const string PeriodicHpRecoveryPercent="PERIODIC_HP_RECOVERY_PERCENT";
        public const string PeriodicMpRecoveryPercent="PERIODIC_MP_RECOVERY_PERCENT";
        public const string LowHpThresholdPercent="LOW_HP_THRESHOLD_PERCENT";
        public const string LowHpAttackBoostPercent="LOW_HP_ATTACK_BOOST_PERCENT";
        public const string LowHpDefenseBoostPercent="LOW_HP_DEFENSE_BOOST_PERCENT";

        public static PassiveCompileResult Compile(IEnumerable<PassiveContribution> equipped)
        {
            var rows=(equipped??Array.Empty<PassiveContribution>()).ToList();
            if(rows.Count>MaxPassiveSlots)return Fail("PASSIVE_SLOT_LIMIT_EXCEEDED");
            if(rows.Any(x=>x==null||string.IsNullOrWhiteSpace(x.passiveId)))return Fail("PASSIVE_ID_MISSING");
            if(rows.Any(x=>string.IsNullOrWhiteSpace(x.seriesId)))return Fail("PASSIVE_SERIES_ID_MISSING");
            var duplicateSeries=rows.GroupBy(x=>x.seriesId,StringComparer.Ordinal).FirstOrDefault(g=>g.Count()>1);
            if(duplicateSeries!=null)return Fail("PASSIVE_SERIES_DUPLICATE:"+duplicateSeries.Key);
            var r=new PassiveCompileResult{ok=true,contributions=rows};
            foreach(var x in rows)if(!string.IsNullOrWhiteSpace(x.capability))r.capabilities.Add(x.capability);
            return r;
        }

        public static double SumProperty(PassiveCompileResult compiled,string property)
            => compiled==null||!compiled.ok?0:compiled.contributions.Where(x=>x.property==property).Sum(x=>x.value);

        public static bool IsPeriodicRecoveryTick(int tick)
            => tick>0&&tick%PeriodicRecoveryIntervalTicks==0;

        public static int RecoverPeriodic(BattleActorSaveRecord actor,PassiveCompileResult compiled,int tick)
        {
            if(actor==null||compiled==null||!compiled.ok||!actor.alive||actor.hp<=0||!IsPeriodicRecoveryTick(tick))return 0;
            var hpAmount=(int)Math.Floor(Math.Max(0,actor.maxHp)*Math.Max(0,SumProperty(compiled,PeriodicHpRecoveryPercent))/100d);
            var mpAmount=(int)Math.Floor(Math.Max(0,actor.maxMp)*Math.Max(0,SumProperty(compiled,PeriodicMpRecoveryPercent))/100d);
            var hpBefore=actor.hp;
            var mpBefore=actor.mp;
            actor.hp=Math.Min(actor.maxHp,actor.hp+Math.Max(0,hpAmount));
            actor.mp=Math.Min(actor.maxMp,actor.mp+Math.Max(0,mpAmount));
            return (actor.hp-hpBefore)+(actor.mp-mpBefore);
        }

        public static bool IsLowHp(BattleActorSaveRecord actor,PassiveCompileResult compiled)
        {
            if(actor==null||compiled==null||!compiled.ok||!actor.alive||actor.hp<=0||actor.maxHp<=0)return false;
            var threshold=SumProperty(compiled,LowHpThresholdPercent);
            if(threshold<=0)return false;
            threshold=Math.Max(0,Math.Min(100,threshold));
            return actor.hp*100d/actor.maxHp<=threshold;
        }

        public static double LowHpProperty(PassiveCompileResult compiled,BattleActorSaveRecord actor,string property)
            => IsLowHp(actor,compiled)?SumProperty(compiled,property):0;

        static PassiveCompileResult Fail(string reason)=>new PassiveCompileResult{ok=false,reason=reason};
    }
}
