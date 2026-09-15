using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAdventure.Game.AI
{
    [Serializable] public sealed class AiRuntimeFile { public string schema_version; public AiProgram[] data; }
    [Serializable] public sealed class AiProgram { public string program_id,entry_instruction; public AiInstruction[] instructions; public AiLimits limits; }
    [Serializable] public sealed class AiLimits { public int max_steps,max_subroutine_depth; }
    [Serializable] public sealed class AiInstruction { public string instruction_id,op,evaluator; public AiTargetSelector target_selector; }
    [Serializable] public sealed class AiTargetSelector { public string selector_id; }

    public sealed class AiDecision { public bool ok; public string reason,evaluator,selectorId; }

    public static class AiRuntime
    {
        public static Dictionary<string,AiProgram> Load(string json)
        {
            var f=JsonUtility.FromJson<AiRuntimeFile>(json);
            if(f==null||f.data==null)throw new ArgumentException("AI_RUNTIME_INVALID");
            var r=new Dictionary<string,AiProgram>();
            foreach(var p in f.data){if(p==null||string.IsNullOrWhiteSpace(p.program_id))continue;if(r.ContainsKey(p.program_id))throw new ArgumentException("AI_PROGRAM_DUPLICATE");r[p.program_id]=p;}
            return r;
        }

        public static AiDecision Decide(AiProgram p)
        {
            if(p==null)return Fail("AI_PROGRAM_MISSING");
            var byId=new Dictionary<string,AiInstruction>();
            foreach(var i in p.instructions??Array.Empty<AiInstruction>())if(i!=null&&!string.IsNullOrWhiteSpace(i.instruction_id))byId[i.instruction_id]=i;
            if(!byId.TryGetValue(p.entry_instruction??"",out var entry))return Fail("AI_ENTRY_MISSING");
            if(entry.op!="ACTION")return Fail("AI_OP_UNSUPPORTED");
            if(string.IsNullOrWhiteSpace(entry.evaluator))return Fail("AI_EVALUATOR_MISSING");
            return new AiDecision{ok=true,evaluator=entry.evaluator,selectorId=entry.target_selector?.selector_id};
        }
        private static AiDecision Fail(string r)=>new AiDecision{ok=false,reason=r};
    }
}
