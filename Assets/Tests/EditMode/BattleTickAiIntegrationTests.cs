#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickAiIntegrationTests
 {
  sealed class Rng:IRandomSource{public double Next01(string p)=>0;}
  [Test] public void ReadyAiActor_ReservesAndExecutesInstantAction()
  {
   var s=Snapshot(100);var o=Options(0);var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(1,r.aiEvaluated.Count);Assert.AreEqual(1,r.actionsStarted.Count);Assert.AreEqual(1,s.actionReservations.Count);Assert.AreEqual(0,s.actors[0].actionGauge);Assert.AreEqual(9,s.actors[1].hp);
  }
  [Test] public void ReadyAiActor_CanStartCastWithoutGaugeConsumption()
  {
   var s=Snapshot(100);var r=BattleTickRuntime.Advance(s,Options(2));Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(s.actors[0].cast.active);Assert.AreEqual(100,s.actors[0].actionGauge);Assert.AreEqual(1,s.actionReservations.Count);StringAssert.EndsWith(":CAST",r.actionsStarted[0]);
  }
  static BattleSnapshotSaveRecord Snapshot(double gauge){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B"});s.actors.Add(new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,mp=10,maxMp=10,actionGauge=gauge});s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=10,maxHp=10});return s;}
  static BattleTickOptions Options(int cast){return new BattleTickOptions{criticalRng=new Rng(),hitRng=new Rng(),blockRng=new Rng(),resolveAi=a=>{var n=new FormalAiNode{instance_id="N",master_node_id="AIA-X",node_type="action"};return new FormalAiBattleContext{program=new FormalAiProgram{schema_version="2.0.0",id="AIP-X",entry_node_id="N",nodes=new[]{n},edges=new FormalAiEdge[0]},execution=new FormalAiExecutionContext(),resolveSkill=x=>new SkillDefinition{id="S",castTicks=cast},resolveTargetCandidates=x=>new[]{"B"},buildAttack=(x,sk,t)=>new BattleAttackProposal{hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=1}};}};}
 }
}
#endif
