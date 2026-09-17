using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.Battle
{
 public sealed class SkillTargetReservationResult{public bool ok;public string reason;public SkillActionRequest action;public SkillTargetResult targets;}
 public static class SkillTargetReservationRuntime
 {
  public static SkillTargetReservationResult Build(BattleSnapshotSaveRecord snapshot,SkillDefinition skill,BattleAttackProposal attack,SkillTargetRequest targetRequest,IRandomSource targetRng,System.Func<string,BattleAttackProposal> buildAttack=null){if(skill==null||attack==null||targetRequest==null)return Fail("SKILL_TARGET_RESERVATION_INPUT_INVALID");var targets=SkillTargetRuntime.Resolve(snapshot,targetRequest,targetRng);if(!targets.ok)return new SkillTargetReservationResult{ok=false,reason=targets.reason,targets=targets};if(targets.targetIds.Count==0)return Fail("SKILL_TARGET_RESERVATION_EMPTY");attack.sourceId=targetRequest.sourceId;attack.targetId=targets.targetIds[0];return new SkillTargetReservationResult{ok=true,targets=targets,action=new SkillActionRequest{skill=skill,attack=attack,fixedTargetIds=targets.targetIds,buildAttack=buildAttack,hasTargetContract=true,targetCategory=targetRequest.category,targetRange=targetRequest.range,targetExcludeSelf=targetRequest.excludeSelf}};}
  static SkillTargetReservationResult Fail(string r)=>new SkillTargetReservationResult{ok=false,reason=r};
 }
}
