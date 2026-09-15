#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickPassiveRecoveryTests
 {
  static BattleSnapshotSaveRecord Snapshot(int tick){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="SEED",tick=tick};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=50,maxHp=100,mp=10,maxMp=50,speed=0,alive=true});s.fixedActorOrder.Add("A");return s;}
  static PassiveCompileResult Passives()=>PassiveRuntime.Compile(new[]{new PassiveContribution{passiveId="P1",seriesId="S1",property=PassiveRuntime.PeriodicHpRecoveryPercent,value=10},new PassiveContribution{passiveId="P2",seriesId="S2",property=PassiveRuntime.PeriodicMpRecoveryPercent,value=20}});

  [Test] public void Advance_AppliesPeriodicPassiveRecoveryBeforeCooldownAndGauge()
  {
   var s=Snapshot(20);var o=new BattleTickOptions{resolvePassives=_=>Passives()};var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok);Assert.AreEqual(60,s.actors[0].hp);Assert.AreEqual(20,s.actors[0].mp);Assert.AreEqual(20,r.passiveRecoveryAmount);CollectionAssert.AreEqual(new[]{"A:20"},r.passiveRecovered);
  }

  [Test] public void Advance_DoesNotRecoverOutsideTwentyTickBoundary()
  {
   var s=Snapshot(19);var o=new BattleTickOptions{resolvePassives=_=>Passives()};var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok);Assert.AreEqual(50,s.actors[0].hp);Assert.AreEqual(10,s.actors[0].mp);Assert.AreEqual(0,r.passiveRecoveryAmount);Assert.AreEqual(0,r.passiveRecovered.Count);
  }
 }
}
#endif
