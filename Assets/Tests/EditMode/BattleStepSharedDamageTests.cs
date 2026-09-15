#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleStepSharedDamageTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void HitDepletesSnapshotBarrierAndLifecycleTogether(){var s=S();var t=s.actors[1];EffectLifecycleRuntime.ApplyBarrier(t,new EffectApplyRequest{instanceId="SH",sourceId="B",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=9},3);var r=BattleStepExecutor.ExecuteAttack(s,P(5,0),new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(2,r.resolvedHit.actualHpLoss);Assert.AreEqual(0,t.barrierLayers.Count);Assert.AreEqual(0,t.appliedEffects.Count);Assert.AreEqual(8,t.hp);}
  [Test] public void ReflectionUsesHpDamageOnlyAndDoesNotConsumeSourceBarrier(){var s=S();var source=s.actors[0];EffectLifecycleRuntime.ApplyBarrier(source,new EffectApplyRequest{instanceId="SRC_SH",sourceId="A",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=9},9);var r=BattleStepExecutor.ExecuteAttack(s,P(10,1),new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(10,r.reflectedDamage);Assert.AreEqual(0,source.hp);Assert.IsFalse(source.alive);Assert.AreEqual(9,source.barrierLayers[0].remaining);}
  static BattleAttackProposal P(double damage,double reflect)=>new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",hitCount=1,damageType=DamageType.PHYSICAL,accuracy=100,evasion=0,criticalRatePercent=0,baseDamage=damage,reflectionRate=reflect};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
