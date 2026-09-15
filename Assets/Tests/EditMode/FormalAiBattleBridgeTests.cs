#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class FormalAiBattleBridgeTests
 {
  [Test] public void ActionNode_BuildsC02ReadySkillRequest()
  {
   var s=new BattleSnapshotSaveRecord{tick=7};s.fixedActorOrder.AddRange(new[]{"A","B"});var a=new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,actionGauge=100};s.actors.Add(a);s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=10,maxHp=10});var node=new FormalAiNode{instance_id="N",master_node_id="AIA-X",node_type="action"};var p=new FormalAiProgram{schema_version="2.0.0",id="AIP-X",entry_node_id="N",nodes=new[]{node},edges=new FormalAiEdge[0]};
   var c=new FormalAiBattleContext{program=p,execution=new FormalAiExecutionContext(),resolveSkill=n=>new SkillDefinition{id="S"},resolveTargetCandidates=n=>new[]{"B"},buildAttack=(n,sk,t)=>new BattleAttackProposal{hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=1}};var r=FormalAiBattleBridge.Decide(s,a,c);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual("A",r.request.attack.sourceId);Assert.AreEqual("B",r.request.attack.targetId);Assert.AreEqual("S",r.request.attack.skillId);Assert.AreEqual("AI:7:A:S",r.request.attack.reservationId);
  }
  [Test] public void NotReadyGauge_DoesNotEvaluateGraph(){var s=new BattleSnapshotSaveRecord();var a=new BattleActorSaveRecord{actorId="A",alive=true,hp=10,actionGauge=90};s.actors.Add(a);var r=FormalAiBattleBridge.Decide(s,a,new FormalAiBattleContext());Assert.IsFalse(r.ok);Assert.AreEqual("AI_BATTLE_GAUGE_NOT_READY",r.reason);}
 }
}
#endif
