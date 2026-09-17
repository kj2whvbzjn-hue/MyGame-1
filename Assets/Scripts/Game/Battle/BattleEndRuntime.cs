using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class BattleEndResult
 {
  public bool ok;
  public string reason;
  public BattleLifecycleTriggerResult triggerResult;
 }
 public static class BattleEndRuntime
 {
  public static BattleEndResult Resolve(BattleSnapshotSaveRecord snapshot,IEnumerable<TriggerRegistration> registrations,TriggerActionContext actionContext,IRandomSource passiveTriggerRng,Action<ReactiveTriggerRequest> executeReactive)
  {
   if(snapshot==null)return new BattleEndResult{ok=false,reason="BATTLE_END_SNAPSHOT_MISSING"};
   var trigger=BattleLifecycleTriggerRuntime.Dispatch(snapshot,TriggerEvent.ON_BATTLE_END,registrations,actionContext,passiveTriggerRng,executeReactive);
   if(!trigger.ok)return new BattleEndResult{ok=false,reason=trigger.reason,triggerResult=trigger};
   EffectLifecycleRuntime.CleanupBattleEnd(snapshot);
   return new BattleEndResult{ok=true,triggerResult=trigger};
  }
 }
}
