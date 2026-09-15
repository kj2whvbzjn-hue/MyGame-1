using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class BattleLifecycleTriggerResult
 {
  public bool ok=true;
  public string reason;
  public List<BattleTriggerDispatch> dispatches=new List<BattleTriggerDispatch>();
  public int reactiveExecuted,reactiveSkipped;
 }
 public static class BattleLifecycleTriggerRuntime
 {
  public static BattleLifecycleTriggerResult Dispatch(BattleSnapshotSaveRecord snapshot,TriggerEvent trigger,IEnumerable<TriggerRegistration> registrations,TriggerActionContext actionContext,IRandomSource passiveTriggerRng,Action<ReactiveTriggerRequest> executeReactive)
  {
   if(snapshot==null)return Fail("BATTLE_LIFECYCLE_SNAPSHOT_MISSING");
   if(trigger!=TriggerEvent.ON_BATTLE_START&&trigger!=TriggerEvent.ON_TURN_START&&trigger!=TriggerEvent.ON_TURN_END&&trigger!=TriggerEvent.WHILE_SOURCE_ALIVE)return Fail("BATTLE_LIFECYCLE_TRIGGER_INVALID");
   var context=actionContext??new TriggerActionContext{actionId=trigger+":"+snapshot.tick};
   if(string.IsNullOrWhiteSpace(context.actionId))context.actionId=trigger+":"+snapshot.tick;
   var fixedOrder=BattleEffectLifecycle.BuildFixedOrder(snapshot);
   var result=new BattleLifecycleTriggerResult();
   if(trigger==TriggerEvent.WHILE_SOURCE_ALIVE)
   {
    foreach(var actorId in snapshot.fixedActorOrder)
    {
     var actor=snapshot.actors.Find(x=>x.actorId==actorId);
     if(actor==null||!actor.alive||actor.hp<=0)continue;
     var dispatch=BattleEffectLifecycle.DispatchEvent(trigger,actorId,actorId,registrations,fixedOrder,context.actionId,null,context,actorId);
     dispatch.registrations.RemoveAll(x=>x.ownerId!=actorId);
     result.dispatches.Add(dispatch);
    }
   }
   else
   {
    result.dispatches.Add(BattleEffectLifecycle.DispatchEvent(trigger,null,null,registrations,fixedOrder,context.actionId,null,context,null));
   }
   TriggerActivationRuntime.EnqueueBatch(context,result.dispatches);
   var drain=TriggerActivationRuntime.Drain(context,passiveTriggerRng,executeReactive);
   result.reactiveExecuted=drain.executed;result.reactiveSkipped=drain.skipped;
   if(!drain.ok){result.ok=false;result.reason=drain.reason;}
   return result;
  }
  static BattleLifecycleTriggerResult Fail(string reason)=>new BattleLifecycleTriggerResult{ok=false,reason=reason};
 }
}
