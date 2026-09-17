#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleStepLowHpPassiveTests
 {
  sealed class FixedRng:IRandomSource{readonly double v;public FixedRng(double x){v=x;}public double Next01(string purpose)=>v;}
  static readonly PassiveRuntimeSettings PassiveSettings=new PassiveRuntimeSettings{maxPassiveSlots=7,periodicRecoveryIntervalTicks=13};
  static BattleSnapshotSaveRecord Snapshot(int sourceHp,int targetHp){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="SEED"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=sourceHp,maxHp=100,mp=0,maxMp=0,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=targetHp,maxHp=100,mp=0,maxMp=0,alive=true});s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");return s;}
  static PassiveCompileResult Low(string series,string property,double value)=>PassiveRuntime.Compile(new[]{new PassiveContribution{passiveId="T",seriesId=series,property=PassiveRuntime.LowHpThresholdPercent,value=30},new PassiveContribution{passiveId="P",seriesId=series+"2",property=property,value=value}},PassiveSettings);
  static BattleAttackProposal Proposal(PassiveCompileResult source=null,PassiveCompileResult target=null)=>new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",hitCount=1,damageType=DamageType.Physical,criticalRatePercent=0,accuracy=100,evasion=0,baseDamage=40,damageResistance=0,sourcePassives=source,targetPassives=target};

  [Test] public void LowHpAttackBoost_IncreasesCommittedDamage()
  {
   var s=Snapshot(30,100);var r=BattleStepExecutor.ExecuteAttack(s,Proposal(Low("SA",PassiveRuntime.LowHpAttackBoostPercent,50)),new FixedRng(.9),new FixedRng(0),null);Assert.IsTrue(r.ok);Assert.AreEqual(60,r.resolvedHit.actualHpLoss);
  }

  [Test] public void LowHpDefenseBoost_AddsDamageResistance()
  {
   var s=Snapshot(100,30);var r=BattleStepExecutor.ExecuteAttack(s,Proposal(null,Low("SD",PassiveRuntime.LowHpDefenseBoostPercent,25)),new FixedRng(.9),new FixedRng(0),null);Assert.IsTrue(r.ok);Assert.AreEqual(30,r.resolvedHit.actualHpLoss);
  }

  [Test] public void LowHpBoost_IsInactiveAboveThreshold()
  {
   var s=Snapshot(31,100);var r=BattleStepExecutor.ExecuteAttack(s,Proposal(Low("SA",PassiveRuntime.LowHpAttackBoostPercent,50)),new FixedRng(.9),new FixedRng(0),null);Assert.IsTrue(r.ok);Assert.AreEqual(40,r.resolvedHit.actualHpLoss);
  }
 }
}
#endif
