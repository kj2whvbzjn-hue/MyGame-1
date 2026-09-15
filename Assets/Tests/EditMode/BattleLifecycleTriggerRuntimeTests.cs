#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleLifecycleTriggerRuntimeTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void WhileSourceAlive_OnlyQueuesLivingOwnersInFixedOrder(){var s=S();s.actors[1].hp=0;s.actors[1].alive=false;var regs=new[]{new TriggerRegistration{id="A1",ownerId="A",trigger=TriggerEvent.WHILE_SOURCE_ALIVE},new TriggerRegistration{id="B1",ownerId="B",trigger=TriggerEvent.WHILE_SOURCE_ALIVE}};var seen=new List<string>();var r=BattleLifecycleTriggerRuntime.Dispatch(s,TriggerEvent.WHILE_SOURCE_ALIVE,regs,null,new R(),x=>seen.Add(x.registration.ownerId));Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"A"},seen);}
  [Test] public void TurnStart_UsesFormalPriorityOrdering(){var s=S();var regs=new[]{new TriggerRegistration{id="LOW",ownerId="A",trigger=TriggerEvent.ON_TURN_START,priority=1},new TriggerRegistration{id="HIGH",ownerId="B",trigger=TriggerEvent.ON_TURN_START,priority=9}};var seen=new List<string>();var r=BattleLifecycleTriggerRuntime.Dispatch(s,TriggerEvent.ON_TURN_START,regs,null,new R(),x=>seen.Add(x.registration.id));Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"HIGH","LOW"},seen);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
