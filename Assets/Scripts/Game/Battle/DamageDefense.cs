using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum Element { FIRE, ICE, LIGHTNING, WIND }
    [Serializable] public sealed class ElementShare { public Element element; public double? share; }
    [Serializable] public sealed class BarrierLayer { public string id; public int remaining; }
    public sealed class FinalDamageResult { public double beforeFloorDamage; public int finalDamage; public double criticalMultiplier; }
    public sealed class BlockResult { public bool blocked; public int damage; }
    public sealed class BarrierResult { public int absorbed,hpDamageCandidate; public List<BarrierLayer> layers=new List<BarrierLayer>(); }
    public sealed class HpCommitResult { public int hpBefore,hpAfter,actualHpLoss; public bool fatalCandidate,fatalPrevented; }

    public static class DamageDefense
    {
        public static double ClampResistance(double value)=>Math.Max(0,Math.Min(75,value));

        public static List<ElementShare> NormalizeShares(IEnumerable<ElementShare> source)
        {
            if(source==null)return null;
            var rows=source.ToList(); if(rows.Count==0)throw new ArgumentException("element shares empty");
            if(rows.Select(x=>x.element).Distinct().Count()!=rows.Count)throw new ArgumentException("duplicate element");
            double specified=rows.Where(x=>x.share.HasValue).Sum(x=>x.share.Value);
            if(specified>100+1e-9)throw new ArgumentException("element shares overflow");
            int omitted=rows.Count(x=>!x.share.HasValue);
            if(omitted==0&&Math.Abs(specified-100)>1e-9)throw new ArgumentException("element shares must total 100");
            double each=omitted==0?0:(100-specified)/omitted;
            return rows.Select(x=>new ElementShare{element=x.element,share=x.share??each}).ToList();
        }

        public static FinalDamageResult ResolveFinalDamage(
            double baseDamage,double damageResistance,bool critical,double criticalBonusDamagePercent,
            double criticalBonusReduction,IEnumerable<ElementShare> shares,
            IDictionary<Element,double> elementResistances,double formationMultiplier,double randomMultiplier)
        {
            var resisted=Math.Max(0,baseDamage)*(1-ClampResistance(damageResistance)/100d);
            var critBonus=critical?Math.Max(0,criticalBonusDamagePercent)/100d*(1-Math.Max(0,Math.Min(1,criticalBonusReduction))):0;
            var critMult=1+critBonus; var pre=resisted*critMult;
            var normalized=NormalizeShares(shares);
            double afterElement=pre;
            if(normalized!=null)
            {
                afterElement=0;
                foreach(var s in normalized)
                {
                    double resistance=0;
                    if(elementResistances!=null)elementResistances.TryGetValue(s.element,out resistance);
                    afterElement += pre*s.share.Value/100d*(1-ClampResistance(resistance)/100d);
                }
            }
            var before=afterElement*Math.Max(0,formationMultiplier)*Math.Max(0,randomMultiplier);
            return new FinalDamageResult{beforeFloorDamage=before,finalDamage=Math.Max(0,(int)Math.Floor(before)),criticalMultiplier=critMult};
        }

        public static BlockResult ResolveBlock(int finalDamage,bool eligible,double blockRate,double cutRate,double? roll)
        {
            var raw=Math.Max(0,finalDamage); var rate=Math.Max(0,Math.Min(1,blockRate)); var cut=Math.Max(0,Math.Min(1,cutRate));
            bool blocked=eligible&&rate>0&&roll.HasValue&&roll.Value<rate;
            return new BlockResult{blocked=blocked,damage=blocked?(int)Math.Floor(raw*(1-cut)):raw};
        }

        public static BarrierResult ConsumeBarrierFifo(int damage,IEnumerable<BarrierLayer> source)
        {
            int remaining=Math.Max(0,damage),absorbed=0; var next=new List<BarrierLayer>();
            foreach(var s in source??Array.Empty<BarrierLayer>())
            {
                int available=Math.Max(0,s.remaining),use=Math.Min(available,remaining);
                available-=use;remaining-=use;absorbed+=use;
                if(available>0)next.Add(new BarrierLayer{id=s.id,remaining=available});
            }
            return new BarrierResult{absorbed=absorbed,hpDamageCandidate=remaining,layers=next};
        }

        // The resolver is the synchronous ON_FATAL_DAMAGE interrupt boundary. HP is not committed until it returns.
        public static HpCommitResult CommitHp(int hp,int candidate,Func<int,int,int?> fatalResolver=null)
        {
            int before=Math.Max(0,hp),projected=before-Math.Max(0,candidate),after=Math.Max(0,projected);
            bool fatal=projected<=0;
            if(fatal&&fatalResolver!=null)
            {
                var resolved=fatalResolver(before,projected);
                if(resolved.HasValue)after=Math.Max(0,resolved.Value);
            }
            return new HpCommitResult{
                hpBefore=before,hpAfter=after,actualHpLoss=Math.Max(0,before-after),
                fatalCandidate=fatal,fatalPrevented=fatal&&after>0
            };
        }
    }
}
