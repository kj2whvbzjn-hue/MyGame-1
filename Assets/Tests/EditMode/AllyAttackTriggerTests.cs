#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class AllyAttackTriggerTests
 {
  [Test] public void AllyAttack_OnlyLivingSameTeamOtherOwnersReceive(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="T1",hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="B",teamId="T1",hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="T2",hp=10,maxHp=10});s.fixedActorOrder.AddRange(new[]{"A","B","C"});var regs=new List<TriggerRegistration>{new TriggerRegistration{id="RA",ownerId="A",trigger=TriggerEvent.ON_ALLY_ATTACK},new TriggerRegistration{id="RB",ownerId="B",trigger=TriggerEvent.ON_ALLY_ATTACK},new TriggerRegistration{id="RC",ownerId="C",trigger=TriggerEvent.ON_ALLY_ATTACK}};var hit=new ResolvedHitSaveRecord{actionId="X",sourceId="A",targetId="C",judgement="HIT",actualHpLoss=1};var d=BattleEffectLifecycle.DispatchResolvedHitEvents(hit,regs,BattleEffectLifecycle.BuildFixedOrder(s),snapshot:s);var ally=d.Find(x=>x.trigger==TriggerEvent.ON_ALLY_ATTACK);Assert.NotNull(ally);Assert.AreEqual(1,ally.registrations.Count);Assert.AreEqual("B",ally.registrations[0].ownerId);}
 }
}
#endif
