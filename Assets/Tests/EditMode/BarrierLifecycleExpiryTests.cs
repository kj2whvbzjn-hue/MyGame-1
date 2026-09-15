#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BarrierLifecycleExpiryTests
 {
  [Test] public void BarrierApply_CreatesEffectAndLayer_ThenTickExpiryRemovesBoth(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");var a=new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true};s.actors.Add(a);var applied=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.APPLY,lifecycleKind=EffectLifecycleKind.BARRIER,instanceId="SH",effectId="BARRIER",sourceId="A",power=20,durationTicks=1});Assert.IsTrue(applied.ok,applied.reason);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(1,a.barrierLayers.Count);Assert.AreEqual(20,a.barrierLayers[0].remaining);var tick=BattleTickRuntime.Advance(s);Assert.IsTrue(tick.ok,tick.reason);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(0,a.barrierLayers.Count);CollectionAssert.Contains(tick.expiredEffects,"SH");}
 }
}
#endif
