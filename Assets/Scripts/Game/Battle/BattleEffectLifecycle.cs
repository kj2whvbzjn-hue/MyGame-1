using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleTriggerContext
    {
        public string actionId,sourceId,targetId;
        public int hitIndex;
        public string judgement;
        public double actualHpLoss;
    }

    public sealed class BattleTriggerDispatch
    {
        public TriggerEvent trigger;
        public BattleTriggerContext context;
        public List<TriggerRegistration> registrations=new List<TriggerRegistration>();
    }

    public static class BattleEffectLifecycle
    {
        public static BattleTriggerDispatch DispatchResolvedHit(
            ResolvedHitSaveRecord hit,IEnumerable<TriggerRegistration> registrations,
            IReadOnlyDictionary<string,int> fixedOrder)
        {
            if(hit==null)throw new ArgumentNullException(nameof(hit));
            var trigger=hit.actualHpLoss>0?TriggerEvent.ON_DAMAGE:TriggerEvent.ON_HIT;
            var ctx=new BattleTriggerContext{
                actionId=hit.actionId,sourceId=hit.sourceId,targetId=hit.targetId,
                hitIndex=hit.hitIndex,judgement=hit.judgement,actualHpLoss=hit.actualHpLoss};
            var rows=TriggerRuntime.Resolve(registrations,trigger,fixedOrder);
            foreach(var r in rows)TriggerRuntime.MarkConsumed(r);
            return new BattleTriggerDispatch{trigger=trigger,context=ctx,registrations=rows};
        }

        public static void TickEffects(BattleActorSaveRecord actor)
        {
            if(actor==null)throw new ArgumentNullException(nameof(actor));
            foreach(var e in actor.appliedEffects??new List<AppliedEffectSaveRecord>())
                if(!e.consumed&&e.remainingTicks>0)e.remainingTicks--;
            actor.appliedEffects?.RemoveAll(x=>x==null||x.consumed||x.remainingTicks<=0);
        }

        public static double PassiveProperty(IEnumerable<PassiveContribution> equipped,string property)
        {
            var c=PassiveRuntime.Compile(equipped);
            if(!c.ok)throw new InvalidOperationException(c.reason);
            return PassiveRuntime.SumProperty(c,property);
        }

        public static IReadOnlyDictionary<string,int> BuildFixedOrder(BattleSnapshotSaveRecord snapshot)
        {
            var d=new Dictionary<string,int>(StringComparer.Ordinal);
            if(snapshot?.fixedActorOrder==null)return d;
            for(var i=0;i<snapshot.fixedActorOrder.Count;i++)d[snapshot.fixedActorOrder[i]]=i;
            return d;
        }
    }
}
