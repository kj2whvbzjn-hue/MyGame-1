using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.AI
{
    public sealed class FormalAiExecutionContext
    {
        public Func<FormalAiNode,bool> search;
        public Func<FormalAiNode,bool> condition;
        public int maxSteps=-1;
    }

    public sealed class FormalAiExecutionResult
    {
        public bool ok;
        public string reason;
        public FormalAiNode action;
        public List<string> visitedNodeIds=new List<string>();
    }

    // GS-21 execution boundary: graph structure is Formal AI V2; Search/Condition semantics
    // are supplied by compiled masters. The executor only performs deterministic graph traversal.
    public static class FormalAiV2Executor
    {
        public static FormalAiExecutionResult Execute(FormalAiProgram program,FormalAiExecutionContext context)
        {
            var validation=FormalAiV2.Validate(program);
            if(!validation.ok)return Fail(validation.reason);
            if(context==null)return Fail("AI_V2_CONTEXT_MISSING");
            if(context.maxSteps<=0)return Fail("AI_V2_STEP_LIMIT_INVALID");
            var maxSteps=context.maxSteps;

            var byId=(program.nodes??Array.Empty<FormalAiNode>()).ToDictionary(x=>x.instance_id,StringComparer.Ordinal);
            var outgoing=(program.edges??Array.Empty<FormalAiEdge>())
                .GroupBy(x=>x.from.node_id,StringComparer.Ordinal)
                .ToDictionary(x=>x.Key,x=>x.ToList(),StringComparer.Ordinal);
            var result=new FormalAiExecutionResult{ok=true};
            var current=program.entry_node_id;

            for(var step=0;step<maxSteps;step++)
            {
                var node=byId[current];
                result.visitedNodeIds.Add(current);
                if(node.node_type=="action")
                {
                    result.action=node;
                    return result;
                }

                bool outcome;
                string port;
                if(node.node_type=="search")
                {
                    if(context.search==null)return Fail("AI_V2_SEARCH_HANDLER_MISSING",result.visitedNodeIds);
                    outcome=context.search(node);
                    port=outcome?"found":"not_found";
                }
                else
                {
                    if(context.condition==null)return Fail("AI_V2_CONDITION_HANDLER_MISSING",result.visitedNodeIds);
                    outcome=context.condition(node);
                    port=outcome?"true":"false";
                }

                if(!outgoing.TryGetValue(current,out var edges))return Fail("AI_V2_EDGE_MISSING",result.visitedNodeIds);
                var edge=edges.SingleOrDefault(x=>x.from.port_id==port);
                if(edge==null)return Fail("AI_V2_BRANCH_MISSING",result.visitedNodeIds);
                current=edge.to.node_id;
            }
            return Fail("AI_V2_STEP_LIMIT",result.visitedNodeIds);
        }

        static FormalAiExecutionResult Fail(string reason,List<string> visited=null)=>new FormalAiExecutionResult{
            ok=false,reason=reason,visitedNodeIds=visited??new List<string>()};
    }
}
