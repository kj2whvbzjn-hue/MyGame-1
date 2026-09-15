#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleCastInterruptTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true,cast=new CastSaveRecord{reservationId="R",skillId="SK",active=true,remainingTicks=5}});s.fixedActorOrder.Add("A");return s;}
  [Test] public void Stun_InterruptsActiveCast(){var s=Snapshot();s.actors[0].appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="STUN",kind=EffectLifecycleKind.STATUS.ToString(),remainingTicks=2});var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsFalse(s.actors[0].cast.active);Assert.AreEqual(0,s.actors[0].cast.remainingTicks);CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_INCAPACITATED");}
  [Test] public void SkillBlock_InterruptsActiveCast(){var s=Snapshot();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{isSkillBlocked=_=>true});Assert.IsFalse(s.actors[0].cast.active);CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_SKILL_BLOCK");}
  [Test] public void ForcedMovement_InterruptsActiveCast(){var s=Snapshot();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{wasForcedMoved=_=>true});Assert.IsFalse(s.actors[0].cast.active);CollectionAssert.Contains(r.castsInterrupted,"A:CAST_INTERRUPTED_FORCED_MOVEMENT");}
 }
}
#endif
