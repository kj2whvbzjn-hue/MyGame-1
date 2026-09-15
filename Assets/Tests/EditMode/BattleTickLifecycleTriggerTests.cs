#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickLifecycleTriggerTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void FirstTick_DispatchesBattleStartTurnStartAliveTurnEndInOrder(){var s=S();var regs=new[]{Reg("BS",TriggerEvent.ON_BATTLE_START),Reg("TS",TriggerEvent.ON_TURN_START),Reg("AL",TriggerEvent.WHILE_SOURCE_ALIVE),Reg("TE",TriggerEvent.ON_TURN_END)};var seen=new List<string>();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{triggerRegistrations=regs,passiveTriggerRng=new R(),executeReactive=x=>seen.Add(x.registration.id)});Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"BS","TS","AL","TE"},seen);}
  [Test] public void LaterTick_DoesNotRepeatBattleStart(){var s=S();s.tick=1;var regs=new[]{Reg("BS",TriggerEvent.ON_BATTLE_START),Reg("TS",TriggerEvent.ON_TURN_START),Reg("TE",TriggerEvent.ON_TURN_END)};var seen=new List<string>();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{triggerRegistrations=regs,passiveTriggerRng=new R(),executeReactive=x=>seen.Add(x.registration.id)});Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"TS","TE"},seen);}
  static TriggerRegistration Reg(string id,TriggerEvent e)=>new TriggerRegistration{id=id,ownerId="A",trigger=e,activationChance=1};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=10,maxMp=10,alive=true});return s;}
 }
}
#endif
