#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleCastInterruptTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true,cast=new CastSaveRecord{reservationId="R",skillId="SK",active=true,remainingTicks=5}});s.fixedActorOrder.Add("A");s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",startTick=0,completeTick=5});return s;}
  [Test] public void ActionDisabledCapability_InterruptsActiveCast_RegardlessOfEffectId(){var s=Snapshot();s.actors[0].appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="FUTURE_DISABLE",kind=EffectLifecycleKind.STATUS.ToString(),remainingTicks=2,actionDisabled=true});var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsFalse(s.actors[0].cast.active);Assert.AreEqual(0,s.actors[0].cast.remainingTicks);Assert.IsFalse(s.actionReservations.Exists(x=>x.reservationId=="R"));CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_INCAPACITATED");}
  [Test] public void EffectIdAlone_DoesNotDefineActionDisabledCapability(){var s=Snapshot();s.actors[0].appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="STUN",kind=EffectLifecycleKind.STATUS.ToString(),remainingTicks=2,actionDisabled=false});BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsTrue(s.actors[0].cast.active);}
  [Test] public void SkillBlock_InterruptsActiveCast(){var s=Snapshot();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{isSkillBlocked=_=>true});Assert.IsFalse(s.actors[0].cast.active);Assert.IsFalse(s.actionReservations.Exists(x=>x.reservationId=="R"));CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_SKILL_BLOCK");}
  [Test] public void ForcedMovement_InterruptsActiveCast(){var s=Snapshot();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{wasForcedMoved=_=>true});Assert.IsFalse(s.actors[0].cast.active);Assert.IsFalse(s.actionReservations.Exists(x=>x.reservationId=="R"));CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_FORCED_MOVEMENT");}
 }
}
#endif
