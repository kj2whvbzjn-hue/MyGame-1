using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuildAdventure.Game.AI
{
    [Serializable] public sealed class FormalAiProgramFile { public string schema_version,data_version; public FormalAiProgram[] data; }
    [Serializable] public sealed class FormalAiProgram
    {
        public string schema_version,data_version,id,name,status,entry_node_id;
        public int version;
        public FormalAiNode[] nodes;
        public FormalAiEdge[] edges;
        public FormalAiSubroutine[] subroutines;
    }
    [Serializable] public sealed class FormalAiPosition { public double x,y; }
    [Serializable] public sealed class FormalAiNode
    {
        public string instance_id,master_node_id,master_data_version,node_type;
        public FormalAiPosition position;
        public FormalAiTargetSelector target_selector;
    }
    [Serializable] public sealed class FormalAiTargetSelector { public string selector_id; }
    [Serializable] public sealed class FormalAiEndpoint { public string node_id,port_id; }
    [Serializable] public sealed class FormalAiEdge { public string id; public FormalAiEndpoint from,to; }
    [Serializable] public sealed class FormalAiSubroutine { public string id,entry_node_id; }

    public sealed class FormalAiValidationResult
    {
        public bool ok;
        public string reason;
        public static FormalAiValidationResult Ok()=>new FormalAiValidationResult{ok=true};
        public static FormalAiValidationResult Fail(string reason)=>new FormalAiValidationResult{ok=false,reason=reason};
    }

    // GS-21: validates the player-visible Formal AI V2 graph. Runtime meaning comes from masters/tags,
    // never from node position or display names.
    public static class FormalAiV2
    {
        public const string SchemaVersion="2.0.0";

        public static Dictionary<string,FormalAiProgram> Load(string json)
        {
            var file=JsonUtility.FromJson<FormalAiProgramFile>(json);
            if(file==null||file.schema_version!=SchemaVersion||file.data==null)
                throw new ArgumentException("AI_V2_FILE_INVALID");
            var result=new Dictionary<string,FormalAiProgram>(StringComparer.Ordinal);
            foreach(var program in file.data)
            {
                var checkedProgram=Validate(program);
                if(!checkedProgram.ok)throw new ArgumentException(checkedProgram.reason);
                if(result.ContainsKey(program.id))throw new ArgumentException("AI_V2_PROGRAM_DUPLICATE");
                result.Add(program.id,program);
            }
            return result;
        }

        public static FormalAiValidationResult Validate(FormalAiProgram program)
        {
            if(program==null)return FormalAiValidationResult.Fail("AI_V2_PROGRAM_MISSING");
            if(program.schema_version!=SchemaVersion)return FormalAiValidationResult.Fail("AI_V2_SCHEMA_VERSION");
            if(string.IsNullOrWhiteSpace(program.id)||!program.id.StartsWith("AIP-",StringComparison.Ordinal))return FormalAiValidationResult.Fail("AI_V2_PROGRAM_ID");
            if(string.IsNullOrWhiteSpace(program.entry_node_id))return FormalAiValidationResult.Fail("AI_V2_ENTRY_MISSING");
            var nodes=program.nodes??Array.Empty<FormalAiNode>();
            if(nodes.Length==0)return FormalAiValidationResult.Fail("AI_V2_NODES_EMPTY");
            var byId=new Dictionary<string,FormalAiNode>(StringComparer.Ordinal);
            foreach(var node in nodes)
            {
                if(node==null||string.IsNullOrWhiteSpace(node.instance_id))return FormalAiValidationResult.Fail("AI_V2_NODE_ID");
                if(byId.ContainsKey(node.instance_id))return FormalAiValidationResult.Fail("AI_V2_NODE_DUPLICATE");
                if(node.node_type!="search"&&node.node_type!="condition"&&node.node_type!="action")return FormalAiValidationResult.Fail("AI_V2_NODE_TYPE");
                var prefix=node.node_type=="search"?"AIS-":node.node_type=="condition"?"AIC-":"AIA-";
                if(string.IsNullOrWhiteSpace(node.master_node_id)||!node.master_node_id.StartsWith(prefix,StringComparison.Ordinal))return FormalAiValidationResult.Fail("AI_V2_MASTER_TYPE_MISMATCH");
                byId.Add(node.instance_id,node);
            }
            if(!byId.ContainsKey(program.entry_node_id))return FormalAiValidationResult.Fail("AI_V2_ENTRY_UNKNOWN");

            var outgoing=new Dictionary<string,List<FormalAiEdge>>(StringComparer.Ordinal);
            foreach(var id in byId.Keys)outgoing[id]=new List<FormalAiEdge>();
            foreach(var edge in program.edges??Array.Empty<FormalAiEdge>())
            {
                if(edge?.from==null||edge.to==null||!byId.ContainsKey(edge.from.node_id??"")||!byId.ContainsKey(edge.to.node_id??""))return FormalAiValidationResult.Fail("AI_V2_EDGE_NODE_UNKNOWN");
                if(string.IsNullOrWhiteSpace(edge.from.port_id)||string.IsNullOrWhiteSpace(edge.to.port_id))return FormalAiValidationResult.Fail("AI_V2_EDGE_PORT_MISSING");
                outgoing[edge.from.node_id].Add(edge);
            }
            foreach(var pair in byId)
            {
                var node=pair.Value; var edges=outgoing[pair.Key];
                if(node.node_type=="action"&&edges.Count!=0)return FormalAiValidationResult.Fail("AI_V2_ACTION_NOT_TERMINAL");
                if(node.node_type=="search"&&!HasExactly(edges,"found","not_found"))return FormalAiValidationResult.Fail("AI_V2_SEARCH_BRANCHES");
                if(node.node_type=="condition"&&!HasExactly(edges,"true","false"))return FormalAiValidationResult.Fail("AI_V2_CONDITION_BRANCHES");
            }

            var reachable=new HashSet<string>(StringComparer.Ordinal);
            var visiting=new HashSet<string>(StringComparer.Ordinal);
            if(HasCycle(program.entry_node_id,outgoing,reachable,visiting))return FormalAiValidationResult.Fail("AI_V2_CYCLE");
            if(reachable.Count!=byId.Count)return FormalAiValidationResult.Fail("AI_V2_ISOLATED_NODE");
            return FormalAiValidationResult.Ok();
        }

        static bool HasExactly(List<FormalAiEdge> edges,string a,string b)
        {
            if(edges.Count!=2)return false;
            var ports=edges.Select(x=>x.from.port_id).ToArray();
            return ports.Count(x=>x==a)==1&&ports.Count(x=>x==b)==1;
        }

        static bool HasCycle(string id,Dictionary<string,List<FormalAiEdge>> outgoing,HashSet<string> reachable,HashSet<string> visiting)
        {
            if(visiting.Contains(id))return true;
            if(reachable.Contains(id))return false;
            visiting.Add(id); reachable.Add(id);
            foreach(var edge in outgoing[id])if(HasCycle(edge.to.node_id,outgoing,reachable,visiting))return true;
            visiting.Remove(id); return false;
        }
    }
}
