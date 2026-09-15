#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class MultiTargetFixedSequenceTests
 {
  [Test] public void FixedTargetArray_PreservesDuplicateDrawsAndSequence(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B","C"});var r=new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=new System.Collections.Generic.List<string>{"C","B","C"}};var result=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=r,buildAttack=id=>new BattleAttackProposal{hitCount=1,baseDamage=1,accuracy=999,evasion=0,criticalRatePercent=0}},new ConstantRandomSource(0.99),new ConstantRandomSource(0),new ConstantRandomSource(0.99));Assert.IsTrue(result.ok,result.reason);CollectionAssert.AreEqual(new[]{"C","B","C"},result.executedTargetIds);Assert.AreEqual(98,s.actors[2].hp);Assert.AreEqual(99,s.actors[1].hp);}
 }
}
#endif
