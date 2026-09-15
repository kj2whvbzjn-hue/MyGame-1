using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleTriggerContext
    {
        public string actionId,sourceId,actionSourceId,targetId,skillId;
        public int hitIndex; public string judgement; public int actualHpLoss;
        public TriggerActionContext actionContext;
    }
    public sealed class BattleTriggerDispatch
    {
        public TriggerEvent trigger; public BattleTriggerContext context;
        public List<TriggerRegistration> registrations=new List<TriggerRegistration>();
    }

    public static class BattleEffectLifecycle
    {
        public static List<BattleTriggerDispatch> DispatchResolvedHitEvents(ResolvedHitSaveRecord hit,
            IEnumerable<TriggerRegistration> registrations,IReadOnlyDictionary<string,int> fixedOrder,
            string skillId=null,TriggerActionContext actionContext=null)
        {
            if(hit==null)throw new ArgumentNullException(nameof(hit));
            var result=new List<BattleTriggerDispatch>();
            if(string.Equals(hit.judgement,"MISS",StringComparison.OrdinalIgnoreCase))return result;
            Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_HIT_DEALT,skillId,actionContext);
            Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_HIT_RECEIVED,skillId,actionContext);
            if(string.Equals(hit.judgement,"CRITICAL",StringComparison.OrdinalIgnoreCase))Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_CRITICAL,skillId,actionContext);
            if(hit.actualHpLoss>0)Add(result,hit,registrations,fixedOrder,TriggerEvent.ON_DAMAGE_DEALT,skillId,actionContext);
            return result;
        }

        public static BattleTriggerDispatch DispatchResolvedHit(ResolvedHitSaveRecord hit,IEnumerable<TriggerRegistration> registrations,
            IReadOnlyDictionary<string,int> fixedOrder)
        {
            var all=DispatchResolvedHitEvents(hit,registrations,fixedOrder);
            return all.Count>0?all[0]:new BattleTriggerDispatch{trigger=TriggerEvent.ON_HIT_DEALT,context=Context(hit)};
        }

        static void Add(List<BattleTriggerDispatch> output,ResolvedHitSaveRecord hit,IEnumerable<TriggerRegistration> registrations,
            IReadOnlyDictionary<string,int> fixedOrder,TriggerEvent trigger,string skillId,TriggerActionContext actionContext)
        {
            // Candidate discovery must not consume once-triggers. Consumption happens only after successful activation.
            output.Add(new BattleTriggerDispatch{trigger=trigger,context=Context(hit,skillId,actionContext),registrations=TriggerRuntime.Resolve(registrations,trigger,fixedOrder)});
        }

        static BattleTriggerContext Context(ResolvedHitSaveRecord hit,string skillId=null,TriggerActionContext actionContext=null)=>new BattleTriggerContext{
            actionId=hit?.actionId,sourceId=hit?.sourceId,actionSourceId=hit?.sourceId,targetId=hit?.targetId,skillId=skillId,
            hitIndex=hit?.hitIndex??0,judgement=hit?.judgement,actualHpLoss=hit?.actualHpLoss??0,actionContext=actionContext};

        public static void TickEffects(BattleActorSaveRecord actor)
        {
            if(actor==null)throw new ArgumentNullException(nameof(actor));
            foreach(var e in actor.appliedEffects??new List<AppliedEffectSaveRecord>())if(!e.consumed&&e.remainingTicks>0)e.remainingTicks--;
            actor.appliedEffects?.RemoveAll(x=>x==null||x.consumed||x.remainingTicks<=0);
        }
        public static double PassiveProperty(IEnumerable<PassiveContribution> equipped,string property)
        {var c=PassiveRuntime.Compile(equipped);if(!c.ok)throw new InvalidOperationException(c.reason);return PassiveRuntime.SumProperty(c,property);}
        public static IReadOnlyDictionary<string,int> BuildFixedOrder(BattleSnapshotSaveRecord snapshot)
        {var d=new Dictionary<string,int>(StringComparer.Ordinal);if(snapshot?.fixedActorOrder==null)return d;for(var i=0;i<snapshot.fixedActorOrder.Count;i++)d[snapshot.fixedActorOrder[i]]=i;return d;}
    }
}
