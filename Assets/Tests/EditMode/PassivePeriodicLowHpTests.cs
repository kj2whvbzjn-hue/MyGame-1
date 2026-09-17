#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PassivePeriodicLowHpTests
 {
  static PassiveRuntimeSettings Settings(int slots=5,int interval=20)=>new PassiveRuntimeSettings{maxPassiveSlots=slots,periodicRecoveryIntervalTicks=interval};
  static PassiveCompileResult Compile(PassiveRuntimeSettings settings,params PassiveContribution[] rows)=>PassiveRuntime.Compile(rows,settings);

  [Test] public void PeriodicRecovery_UsesConfiguredInterval_AndCapsResources()
  {
   var settings=Settings(7,13);
   var c=Compile(settings,
    new PassiveContribution{passiveId="P1",seriesId="S1",property=PassiveRuntime.PeriodicHpRecoveryPercent,value=10,periodicIntervalTicks=13,periodicInitialDelayTicks=13},
    new PassiveContribution{passiveId="P2",seriesId="S2",property=PassiveRuntime.PeriodicMpRecoveryPercent,value=20,periodicIntervalTicks=13,periodicInitialDelayTicks=13});
   var a=new BattleActorSaveRecord{actorId="A",hp=80,maxHp=100,mp=45,maxMp=50,alive=true};
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,12,settings));
   Assert.AreEqual(15,PassiveRuntime.RecoverPeriodic(a,c,13,settings));
   Assert.AreEqual(90,a.hp); Assert.AreEqual(50,a.mp);
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,14,settings));
  }

  [Test] public void Compile_UsesConfiguredSlotLimit()
  {
   var rows=new[]{
    new PassiveContribution{passiveId="P1",seriesId="S1"},
    new PassiveContribution{passiveId="P2",seriesId="S2"},
    new PassiveContribution{passiveId="P3",seriesId="S3"}};
   Assert.IsTrue(Compile(Settings(3,20),rows).ok);
   Assert.AreEqual("PASSIVE_SLOT_LIMIT_EXCEEDED",Compile(Settings(2,20),rows).reason);
  }

  [Test] public void Compile_RejectsMissingBalanceSettings()
  {
   Assert.AreEqual("PASSIVE_SETTINGS_MISSING",PassiveRuntime.Compile(new PassiveContribution[0],null).reason);
  }

  [Test] public void LowHpProperties_ActivateAtConfiguredThreshold()
  {
   var c=Compile(Settings(),
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
