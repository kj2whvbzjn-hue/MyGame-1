using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleTriggerContext
    {
        public string actionId,sourceId,actionSourceId,targetId,skillId;
        public int hitIndex;
        public string judgement;
        public int actualHpLoss;
    }

    public sealed class BattleTriggerDispatch
    {
        public TriggerEvent trigger;
        public BattleTriggerContext context;
        public List<TriggerRegistration> registrations=new List<TriggerRegistration>();
    }

    public static class BattleEffectLifecycle
    {
        // GS-20: a successful hit and damage are separate events. A MISS is not ON_HIT_DEALT.
        // Reactive candidates are emitted in event order and are processed for each hit before the next hit.
        public static List<BattleTriggerDispatch> DispatchResolvedHitEvents(
            ResolvedHitSaveRecord hit,IEnumerable<TriggerRegistration> registrations,
            IReadOnlyDictionary<string,int> fixedOrder)
        {
            if(hit==null)throw new ArgumentNullException(nameof(hit));
            var result=new List<BattleTriggerDispatch>();
            if(string.Equals(hit.judgement,"MISS",StringComparison.OrdinalIgnoreCase))return result;

            Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_HIT_DEALT);
            Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_HIT_RECEIVED);
            if(string.Equals(hit.judgement,"CRITICAL",StringComparison.OrdinalIgnoreCase))
                Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_CRITICAL);
            if(hit.actualHpLoss>0)
                Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_DAMAGE_DEALT);
            return result;
        }

        // Compatibility for older callers; returns the first formal event, or an empty ON_HIT_DEALT dispatch.
        public static BattleTriggerDispatch DispatchResolvedHit(
            ResolvedHitSaveRecord hit,IEnumerable<TriggerRegistration> registrations,
            IReadOnlyDictionary<string,int> fixedOrder)
        {
            var all=DispatchResolvedHitEvents(hit,registrations,fixedOrder);
            return all.Count>0?all[0]:new BattleTriggerDispatch{
                trigger=TriggerEvent.ON_HIT_DEALT,
                context=Context(hit)
            };
        }

        static void Add(List<BattleTriggerDispatch> output,ResolvedHitSaveRecord hit,
            IEnumerable<TriggerRegistration> registrations,IReadOnlyDictionary<string,int> fixedOrder,TriggerEvent trigger)
        {
            var rows=TriggerRuntime.Resolve(registrations,trigger,fixedOrder);
            foreach(var r in rows)TriggerRuntime.MarkConsumed(r);
            output.Add(new BattleTriggerDispatch{trigger=trigger,context=Context(hit),registrations=rows});
        }

        static BattleTriggerContext Context(ResolvedHitSaveRecord hit)=>new BattleTriggerContext{
            actionId=hit?.actionId,sourceId=hit?.sourceId,actionSourceId=hit?.sourceId,targetId=hit?.targetId,
            hitIndex=hit?.hitIndex??0,judgement=hit?.judgement,actualHpLoss=hit?.actualHpLoss??0};

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
