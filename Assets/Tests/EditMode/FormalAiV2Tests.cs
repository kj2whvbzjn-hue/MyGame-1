#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FormalAiV2Tests
    {
        static FormalAiNode Node(string id,string master,string type)=>new FormalAiNode{instance_id=id,master_node_id=master,node_type=type,position=new FormalAiPosition()};
        static FormalAiEdge Edge(string from,string port,string to)=>new FormalAiEdge{from=new FormalAiEndpoint{node_id=from,port_id=port},to=new FormalAiEndpoint{node_id=to,port_id="in"}};

        [Test] public void FormalV2_SearchHasTwoBranches_AndActionIsTerminal()
        {
            var p=new FormalAiProgram{schema_version="2.0.0",id="AIP-0001",entry_node_id="S",
                nodes=new[]{Node("S","AIS-0001","search"),Node("A","AIA-0001","action"),Node("B","AIA-0001","action")},
                edges=new[]{Edge("S","found","A"),Edge("S","not_found","B")},subroutines=new FormalAiSubroutine[0]};
            Assert.IsTrue(FormalAiV2.Validate(p).ok);
        }

        [Test] public void FormalV2_RejectsActionOutputEdge()
        {
            var p=new FormalAiProgram{schema_version="2.0.0",id="AIP-0001",entry_node_id="A",
                nodes=new[]{Node("A","AIA-0001","action"),Node("B","AIA-0001","action")},
                edges=new[]{Edge("A","next","B")},subroutines=new FormalAiSubroutine[0]};
            Assert.AreEqual("AI_V2_ACTION_NOT_TERMINAL",FormalAiV2.Validate(p).reason);
        }

        [Test] public void FormalV2_RejectsCycleAndIsolatedNodes()
        {
            var cycle=new FormalAiProgram{schema_version="2.0.0",id="AIP-0001",entry_node_id="C1",
                nodes=new[]{Node("C1","AIC-0004","condition"),Node("C2","AIC-0004","condition")},
                edges=new[]{Edge("C1","true","C2"),Edge("C1","false","C2"),Edge("C2","true","C1"),Edge("C2","false","C1")}};
            Assert.AreEqual("AI_V2_CYCLE",FormalAiV2.Validate(cycle).reason);

            var isolated=new FormalAiProgram{schema_version="2.0.0",id="AIP-0002",entry_node_id="A",
                nodes=new[]{Node("A","AIA-0001","action"),Node("B","AIA-0001","action")},edges=new FormalAiEdge[0]};
            Assert.AreEqual("AI_V2_ISOLATED_NODE",FormalAiV2.Validate(isolated).reason);
        }
    }
}
#endif
