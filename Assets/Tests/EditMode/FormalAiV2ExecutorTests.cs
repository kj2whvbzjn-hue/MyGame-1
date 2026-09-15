#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FormalAiV2ExecutorTests
    {
        [Test] public void SearchFound_ConditionTrue_ReachesActionDeterministically()
        {
            var p=Program();
            var r=FormalAiV2Executor.Execute(p,new FormalAiExecutionContext{
                search=n=>true,
                condition=n=>true
            });
            Assert.IsTrue(r.ok,r.reason);
            Assert.AreEqual("N-A1",r.action.instance_id);
            CollectionAssert.AreEqual(new[]{"N-S","N-C","N-A1"},r.visitedNodeIds);
        }

        [Test] public void SearchNotFound_UsesNotFoundAction()
        {
            var r=FormalAiV2Executor.Execute(Program(),new FormalAiExecutionContext{
                search=n=>false,
                condition=n=>true
            });
            Assert.IsTrue(r.ok,r.reason);
            Assert.AreEqual("N-A2",r.action.instance_id);
            CollectionAssert.AreEqual(new[]{"N-S","N-A2"},r.visitedNodeIds);
        }

        [Test] public void MissingCompiledHandler_FailsClosed()
        {
            var r=FormalAiV2Executor.Execute(Program(),new FormalAiExecutionContext());
            Assert.IsFalse(r.ok);
            Assert.AreEqual("AI_V2_SEARCH_HANDLER_MISSING",r.reason);
        }

        static FormalAiProgram Program()=>new FormalAiProgram{
            schema_version=FormalAiV2.SchemaVersion,id="AIP-TEST",entry_node_id="N-S",
            nodes=new[]{
                new FormalAiNode{instance_id="N-S",node_type="search",master_node_id="AIS-1"},
                new FormalAiNode{instance_id="N-C",node_type="condition",master_node_id="AIC-1"},
                new FormalAiNode{instance_id="N-A1",node_type="action",master_node_id="AIA-1"},
                new FormalAiNode{instance_id="N-A2",node_type="action",master_node_id="AIA-2"}
            },
            edges=new[]{
                E("E1","N-S","found","N-C"),E("E2","N-S","not_found","N-A2"),
                E("E3","N-C","true","N-A1"),E("E4","N-C","false","N-A2")
            }
        };

        static FormalAiEdge E(string id,string from,string port,string to)=>new FormalAiEdge{
            id=id,from=new FormalAiEndpoint{node_id=from,port_id=port},to=new FormalAiEndpoint{node_id=to,port_id="in"}};
    }
}
#endif
