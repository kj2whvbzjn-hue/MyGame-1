#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PassivePeriodicLowHpTests
 {
  static PassiveCompileResult Compile(params PassiveContribution[] rows)=>PassiveRuntime.Compile(rows);

  [Test] public void PeriodicRecovery_RunsOnlyEveryTwentyTicks_AndCapsResources()
  {
   var c=Compile(
    new PassiveContribution{passiveId="P1",seriesId="S1",property=PassiveRuntime.PeriodicHpRecoveryPercent,value=10},
    new PassiveContribution{passiveId="P2",seriesId="S2",property=PassiveRuntime.PeriodicMpRecoveryPercent,value=20});
   var a=new BattleActorSaveRecord{actorId="A",hp=80,maxHp=100,mp=45,maxMp=50,alive=true};
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,19));
   Assert.AreEqual(15,PassiveRuntime.RecoverPeriodic(a,c,20));
   Assert.AreEqual(90,a.hp); Assert.AreEqual(50,a.mp);
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,21));
  }

  [Test] public void LowHpProperties_ActivateAtConfiguredThreshold()
  {
   var c=Compile(
    new PassiveContribution{passiveId="P1",seriesId="S1",property=PassiveRuntime.LowHpThresholdPercent,value=30},
    new PassiveContribution{passiveId="P2",seriesId="S2",property=PassiveRuntime.LowHpAttackBoostPercent,value=25});
   var a=new BattleActorSaveRecord{actorId="A",hp=30,maxHp=100,alive=true};
   Assert.IsTrue(PassiveRuntime.IsLowHp(a,c));
   Assert.AreEqual(25,PassiveRuntime.LowHpProperty(c,a,PassiveRuntime.LowHpAttackBoostPercent));
   a.hp=31;
   Assert.IsFalse(PassiveRuntime.IsLowHp(a,c));
   Assert.AreEqual(0,PassiveRuntime.LowHpProperty(c,a,PassiveRuntime.LowHpAttackBoostPercent));
  }
 }
}
#endif
