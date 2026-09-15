using System;

namespace GuildAdventure.Game.Battle
{
    public enum DamageType { PHYSICAL, MAGICAL }

    public sealed class HitResolution
    {
        public bool critical,hit;
        public double criticalRatePercent,criticalRoll;
        public double? hitRatePercent,hitRoll;
        public string hitBypass;
        public int rngConsumed;
    }

    public static class HitCritical
    {
        public const double InitialCriticalBonusDamagePercent=50;

        public static double CriticalBaseRate(double weaponCriticalRate,double luk)
            => Math.Max(0,weaponCriticalRate)*(1+Math.Max(0,luk)/100d);

        public static double PhysicalHitRatePercent(double accuracy,double evasion)
            => evasion<=0 ? 100 : Math.Max(0,(Math.Max(0,accuracy)/Math.Max(0,evasion))*100d);

        public static double MagicalHitRatePercent(double magicAccuracy,double magicResistance)
            => magicResistance<=0 ? 100 : Math.Max(0,Math.Min(100,(Math.Max(0,magicAccuracy)/Math.Max(0,magicResistance))*100d));

        public static HitResolution Resolve(
            double criticalRatePercent,DamageType type,double accuracy,double evasion,
            double magicAccuracy,double magicResistance,Func<double> drawCritical,Func<double> drawHit)
        {
            if(drawCritical==null||drawHit==null)throw new ArgumentNullException();
            var cr=Math.Max(0,Math.Min(100,criticalRatePercent));
            var croll=Math.Max(0,Math.Min(1,drawCritical()))*100;
            if(croll<cr)return new HitResolution{critical=true,hit=true,criticalRatePercent=cr,criticalRoll=croll,hitBypass="CRITICAL_GUARANTEED_HIT",rngConsumed=1};
            var hr=type==DamageType.MAGICAL?MagicalHitRatePercent(magicAccuracy,magicResistance):PhysicalHitRatePercent(accuracy,evasion);
            var hroll=Math.Max(0,Math.Min(1,drawHit()))*100;
            return new HitResolution{critical=false,hit=hroll<hr,criticalRatePercent=cr,criticalRoll=croll,hitRatePercent=hr,hitRoll=hroll,rngConsumed=2};
        }
    }
}
