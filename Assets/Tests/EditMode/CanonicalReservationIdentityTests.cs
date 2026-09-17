#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CanonicalReservationIdentityTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  [Test] public void ExistingCanonicalReservationRejectsDifferentActorOrSkill(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B"});s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=new List<string>{"B"}});var supplied=new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="OTHER",fixedTargetIds=new List<string>{"B"}};var p=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="OTHER",baseDamage=10,accuracy=999};var rng=new Rng();var r=BattleStepExecutor.ExecuteAttack(s,p,supplied,rng,rng,rng);Assert.IsFalse(r.ok);Assert.AreEqual("BATTLE_STEP_RESERVATION_MISMATCH",r.reason);Assert.AreEqual(100,s.actors[1].hp);}
 }
}
#endif
