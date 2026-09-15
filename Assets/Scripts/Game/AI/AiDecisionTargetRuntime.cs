using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.AI
{
 public sealed class AiTargetDecision{public bool ok;public string reason,targetId;public List<string> candidateIds=new List<string>();}
 public static class AiDecisionTargetRuntime
 {
  public const string RngPurpose="AI_TIE_SELECTION";
  public static AiTargetDecision Select(BattleSnapshotSaveRecord snapshot,IEnumerable<string> candidateIds,IRandomSource rng)
  {
   if(snapshot==null)return Fail("AI_TARGET_SNAPSHOT_MISSING");var fixedOrder=new Dictionary<string,int>(StringComparer.Ordinal);for(var i=0;i<(snapshot.fixedActorOrder?.Count??0);i++)fixedOrder[snapshot.fixedActorOrder[i]]=i;
   var candidates=(candidateIds??Array.Empty<string>()).Where(id=>!string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).Select(id=>snapshot.actors?.Find(a=>a!=null&&a.actorId==id)).Where(a=>a!=null&&a.alive&&a.hp>0).OrderBy(a=>fixedOrder.TryGetValue(a.actorId,out var n)?n:int.MaxValue).ThenBy(a=>a.actorId,StringComparer.Ordinal).Select(a=>a.actorId).ToList();
   if(candidates.Count==0)return Fail("AI_TARGET_NOT_FOUND",candidates);if(candidates.Count==1)return new AiTargetDecision{ok=true,targetId=candidates[0],candidateIds=candidates};if(rng==null)return Fail("AI_TIE_SELECTION_RNG_MISSING",candidates);var roll=rng.Next01(RngPurpose);if(roll<0||roll>=1)return Fail("AI_TIE_SELECTION_RNG_RANGE",candidates);var index=Math.Min(candidates.Count-1,(int)Math.Floor(roll*candidates.Count));return new AiTargetDecision{ok=true,targetId=candidates[index],candidateIds=candidates};
  }
  static AiTargetDecision Fail(string reason,List<string> candidates=null)=>new AiTargetDecision{ok=false,reason=reason,candidateIds=candidates??new List<string>()};
 }
}
