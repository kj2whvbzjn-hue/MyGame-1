#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickPassiveRecoveryTests
 {
  static readonly PassiveRuntimeSettings PassiveSettings=new PassiveRuntimeSettings{maxPassiveSlots=7};
  static ActionGaugeSettings GaugeSettings()=>new ActionGaugeSettings{maxGauge=100,aiReevaluationRatio=.1,successfulActionConsumeRatio=1,failedExecutionConsumeRatio=.5,agiGaugeCoefficient=1,actionSpeedMultiplier=1};
  static BattleSnapshotSaveRecord Snapshot(int tick){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="SEED",tick=tick};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=50,maxHp=100,mp=10,maxMp=50,speed=0,alive=true});s.fixedActorOrder.Add("A");return s;}
  static PassiveCompileResult Passives()=>PassiveRuntime.Compile(new[]{new PassiveContribution{passiveId="P1",seriesId="S1",property=PassiveRuntime.PeriodicHpRecoveryPercent,value=10,periodicIntervalTicks=13,periodicInitialDelayTicks=13},new PassiveContribution{passiveId="P2",seriesId="S2",property=PassiveRuntime.PeriodicMpRecoveryPercent,value=20,periodicIntervalTicks=13,periodicInitialDelayTicks=13}},PassiveSettings);
  static BattleTickOptions Options()=>new BattleTickOptions{actionGaugeSettings=GaugeSettings(),passiveRuntimeSettings=PassiveSettings,resolvePassives=_=>Passives()};

  [Test] public void Advance_AppliesPeriodicPassiveRecoveryAtConfiguredBoundary()
  {
   var s=Snapshot(13);var r=BattleTickRuntime.Advance(s,Options());Assert.IsTrue(r.ok);Assert.AreEqual(60,s.actors[0].hp);Assert.AreEqual(20,s.actors[0].mp);Assert.AreEqual(20,r.passiveRecoveryAmount);CollectionAssert.AreEqual(new[]{"A:20"},r.passiveRecovered);
  }

  [Test] public void Advance_DoesNotRecoverOutsideConfiguredBoundary()
  {
   var s=Snapshot(12);var r=BattleTickRuntime.Advance(s,Options());Assert.IsTrue(r.ok);Assert.AreEqual(50,s.actors[0].hp);Assert.AreEqual(10,s.actors[0].mp);Assert.AreEqual(0,r.passiveRecoveryAmount);Assert.AreEqual(0,r.passiveRecovered.Count);
  }
 }
}
#endif
