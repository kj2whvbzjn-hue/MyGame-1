#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleLifecycleTriggerOwnerTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="SEED"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=0,maxHp=10,alive=false});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.fixedActorOrder.Add("C");return s;}
  static List<TriggerRegistration> Registrations(TriggerEvent e)=>new List<TriggerRegistration>{new TriggerRegistration{id="RA",ownerId="A",trigger=e},new TriggerRegistration{id="RB",ownerId="B",trigger=e},new TriggerRegistration{id="RC",ownerId="C",trigger=e}};
  [TestCase(TriggerEvent.ON_BATTLE_START)][TestCase(TriggerEvent.ON_TURN_START)][TestCase(TriggerEvent.ON_TURN_END)][TestCase(TriggerEvent.WHILE_SOURCE_ALIVE)]
  public void LifecycleEvents_DispatchPerLivingOwnerOnly(TriggerEvent e)
  {
   var r=BattleLifecycleTriggerRuntime.Dispatch(Snapshot(),e,Registrations(e),new TriggerActionContext{actionId="X"},null,null);
   Assert.IsTrue(r.ok);Assert.AreEqual(2,r.dispatches.Count);
   Assert.AreEqual("A",r.dispatches[0].context.sourceId);Assert.AreEqual("A",r.dispatches[0].registrations[0].ownerId);
   Assert.AreEqual("C",r.dispatches[1].context.sourceId);Assert.AreEqual("C",r.dispatches[1].registrations[0].ownerId);
  }
 }
}
#endif
