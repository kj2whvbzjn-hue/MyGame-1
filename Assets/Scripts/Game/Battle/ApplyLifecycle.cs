using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum ApplyKind { STATUS, DOT, BUFF, DEBUFF, SHIELD }

    [Serializable]
    public sealed class AppliedEffect
    {
        public string instanceId,sourceId,effectId;
        public ApplyKind kind;
        public int remainingTicks,appliedTick,sequence;
        public double value;
        public bool consumed;
        public bool removable=true,protectedEffect,normalCleanseEligible,actionDisabled;
    }

    public sealed class ApplyResult
    {
        public bool ok; public string reason;
        public List<AppliedEffect> next;
    }

    public static class ApplyLifecycle
    {
        public static ApplyResult Apply(
            IReadOnlyCollection<AppliedEffect> current,AppliedEffect effect)
        {
            if(current==null||effect==null)throw new ArgumentNullException();
            if(string.IsNullOrWhiteSpace(effect.instanceId)||string.IsNullOrWhiteSpace(effect.effectId))
                return Fail("APPLY_EFFECT_INVALID");
            if(current.Any(x=>x.instanceId==effect.instanceId))
                return Fail("APPLY_INSTANCE_DUPLICATE");
            var next=current.Select(Clone).ToList();next.Add(Clone(effect));
            return new ApplyResult{ok=true,next=next};
        }

        public static List<AppliedEffect> AdvanceAndExpire(IReadOnlyCollection<AppliedEffect> current)
        {
            if(current==null)throw new ArgumentNullException(nameof(current));
            var next=new List<AppliedEffect>();
            foreach(var x in current)
            {
                var c=Clone(x);
                if(c.consumed)continue;
                if(c.remainingTicks>0)c.remainingTicks--;
                if(c.remainingTicks!=0)next.Add(c);
            }
            return next;
        }

        public static List<AppliedEffect> Consume(IReadOnlyCollection<AppliedEffect> current,string instanceId)
        {
            if(current==null)throw new ArgumentNullException(nameof(current));
            return current.Where(x=>x.instanceId!=instanceId).Select(Clone).ToList();
        }

        public static double EffectiveSum(IReadOnlyCollection<AppliedEffect> current,ApplyKind kind)
            => current==null?0:current.Where(x=>!x.consumed&&x.kind==kind).Sum(x=>x.value);

        private static AppliedEffect Clone(AppliedEffect x)=>new AppliedEffect{
            instanceId=x.instanceId,sourceId=x.sourceId,effectId=x.effectId,kind=x.kind,
            remainingTicks=x.remainingTicks,appliedTick=x.appliedTick,sequence=x.sequence,
            value=x.value,consumed=x.consumed,removable=x.removable,protectedEffect=x.protectedEffect,
            normalCleanseEligible=x.normalCleanseEligible,actionDisabled=x.actionDisabled};
        private static ApplyResult Fail(string r)=>new ApplyResult{ok=false,reason=r};
    }
}
