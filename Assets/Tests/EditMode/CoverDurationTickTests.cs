#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CoverDurationTickTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",hp=100,maxHp=100,alive=true});s.fixedActorOrder.Add("A");return s;}
  [Test] public void DurationCoverAdvancesOncePerBattleTick(){var s=Snapshot();var c=new CoverContract{id="CV",protectorId="A",protectedActorId="A",lifetime=CoverLifetimeKind.DURATION,active=true,remainingTicks=2};var options=new BattleTickOptions{coverContracts=new[]{c}};var first=BattleTickRuntime.Advance(s,options);Assert.IsTrue(first.ok,first.reason);Assert.AreEqual(1,c.remainingTicks);Assert.IsTrue(c.active);var second=BattleTickRuntime.Advance(s,options);Assert.IsTrue(second.ok,second.reason);Assert.AreEqual(0,c.remainingTicks);Assert.IsFalse(c.active);}
  [Test] public void PersistentAndUsesDoNotDecayByTick(){var s=Snapshot();var persistent=new CoverContract{id="P",lifetime=CoverLifetimeKind.PERSISTENT,active=true,remainingTicks=9};var uses=new CoverContract{id="U",lifetime=CoverLifetimeKind.USES,active=true,remainingUses=2,remainingTicks=9};var r=BattleTickRuntime.Advance(s,new BattleTickOptions{coverContracts=new[]{persistent,uses}});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(9,persistent.remainingTicks);Assert.IsTrue(persistent.active);Assert.AreEqual(2,uses.remainingUses);Assert.AreEqual(9,uses.remainingTicks);Assert.IsTrue(uses.active);}
 }
}
#endif
