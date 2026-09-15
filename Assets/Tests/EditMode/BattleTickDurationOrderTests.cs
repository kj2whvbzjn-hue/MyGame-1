#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickDurationOrderTests
 {
  [Test] public void OneTickDot_UpdatesDurationThenAppliesPeriodicThenExpires(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",sourceId="S",effectId="BURN",kind=EffectLifecycleKind.DOT.ToString(),remainingTicks=1,value=3,appliedTick=0,sequence=0});s.actors.Add(a);s.fixedActorOrder.Add("A");var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsTrue(r.ok);Assert.AreEqual(7,a.hp);CollectionAssert.Contains(r.dotSources,"D");CollectionAssert.Contains(r.expiredEffects,"D");Assert.AreEqual(0,a.appliedEffects.Count);}
  [Test] public void OneTickStun_StillInterruptsCastBeforeExpiry(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true,cast=new CastSaveRecord{reservationId="R",skillId="SK",active=true,remainingTicks=3}};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="S1",effectId="STUN",kind=EffectLifecycleKind.STATUS.ToString(),remainingTicks=1});s.actors.Add(a);s.fixedActorOrder.Add("A");s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK"});var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsFalse(a.cast.active);Assert.AreEqual(0,s.actionReservations.Count);CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_INCAPACITATED");CollectionAssert.Contains(r.expiredEffects,"S1");}
  [Test] public void OneTickBarrier_ExpiresAndRemovesLayerAfterDurationUpdate(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="BR",effectId="BARRIER",kind=EffectLifecycleKind.BARRIER.ToString(),remainingTicks=1});a.barrierLayers.Add(new BarrierLayerSaveRecord{id="BR",effectId="BARRIER",remaining=5});s.actors.Add(a);s.fixedActorOrder.Add("A");var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsTrue(r.ok);CollectionAssert.Contains(r.expiredEffects,"BR");Assert.AreEqual(0,a.barrierLayers.Count);}
 }
}
#endif
