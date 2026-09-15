#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickAiReevaluationTests
 {
  [Test] public void GaugeCrossingMultipleThresholds_ReevaluatesEachTenPercentWithoutReservation(){var s=S(5,100);var calls=0;var r=BattleTickRuntime.Advance(s,new BattleTickOptions{resolveAi=a=>Context(()=>{calls++;return true;})});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(2,calls);CollectionAssert.AreEqual(new[]{"A:10","A:20"},r.aiEvaluated);Assert.AreEqual(0,s.actionReservations.Count);}
  [Test] public void GaugeBelowNextThreshold_DoesNotReevaluate(){var s=S(1,0);var calls=0;var r=BattleTickRuntime.Advance(s,new BattleTickOptions{resolveAi=a=>Context(()=>{calls++;return true;})});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(0,calls);Assert.AreEqual(0,r.aiEvaluated.Count);}
  static FormalAiBattleContext Context(System.Func<bool> condition){var p=new FormalAiProgram{entry_node_id="C",nodes=new[]{new FormalAiNode{instance_id="C",node_type="condition"},new FormalAiNode{instance_id="A",node_type="action"}},edges=new[]{new FormalAiEdge{from=new FormalAiEndpoint{node_id="C",port_id="true"},to=new FormalAiEndpoint{node_id="A",port_id="in"}},new FormalAiEdge{from=new FormalAiEndpoint{node_id="C",port_id="false"},to=new FormalAiEndpoint{node_id="A",port_id="in"}}}};return new FormalAiBattleContext{program=p,execution=new FormalAiExecutionContext{condition=n=>condition()}};}
  static BattleSnapshotSaveRecord S(double gauge,double speed){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true,actionGauge=gauge,speed=speed});return s;}
 }
}
#endif
