#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleDamageRuntimeTests
 {
  [Test] public void BarrierDepletion_RemovesMatchingLifecycleEffect(){var a=A();EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="B",sourceId="S",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=9},5);var r=BattleDamageRuntime.Commit(a,7);Assert.AreEqual(5,r.barrierAbsorbed);Assert.AreEqual(2,r.actualHpLoss);Assert.AreEqual(0,a.barrierLayers.Count);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(8,a.hp);}
  [Test] public void FatalResolver_RunsAfterBarrierBeforeHpCommit(){var a=A();EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="B",sourceId="S",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=9},3);var beforeSeen=-1;var projectedSeen=99;var r=BattleDamageRuntime.Commit(a,20,(before,projected)=>{beforeSeen=before;projectedSeen=projected;return 1;});Assert.AreEqual(10,beforeSeen);Assert.AreEqual(-7,projectedSeen);Assert.IsTrue(r.fatalPrevented);Assert.AreEqual(1,a.hp);Assert.IsTrue(a.alive);}
  [Test] public void BarrierCanBeBypassedForDamageKindsThatDoNotUseShield(){var a=A();EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="B",sourceId="S",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=9},5);var r=BattleDamageRuntime.Commit(a,3,null,false);Assert.AreEqual(0,r.barrierAbsorbed);Assert.AreEqual(7,a.hp);Assert.AreEqual(5,a.barrierLayers[0].remaining);}
  static BattleActorSaveRecord A()=>new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};
 }
}
#endif
