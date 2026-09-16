using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Skills
{
 public sealed class SkillFixedTargetValidationRequest
 {
  public string sourceId;
  public SkillTargetCategory category;
  public SkillTargetRange range;
  public bool excludeSelf;
  public IEnumerable<string> fixedTargetIds;
 }
 public sealed class SkillFixedTargetValidationResult
 {
  public bool ok;
  public string reason;
  public List<string> validTargetIds=new List<string>();
 }
 public static class SkillFixedTargetValidation
 {
  // Revalidates only the already-fixed C02 targets. It never selects, replenishes or rerolls.
  public static SkillFixedTargetValidationResult Validate(BattleSnapshotSaveRecord snapshot,SkillFixedTargetValidationRequest request)
  {
   if(snapshot==null||request==null||string.IsNullOrWhiteSpace(request.sourceId))return Fail("SKILL_FIXED_TARGET_INPUT_INVALID");
   var source=snapshot.actors.Find(x=>x.actorId==request.sourceId);
   if(source==null)return Fail("SKILL_FIXED_TARGET_SOURCE_MISSING");
   if(request.category==SkillTargetCategory.POINT)return Fail("SKILL_FIXED_TARGET_POINT_UNSUPPORTED");
   var ids=(request.fixedTargetIds??Array.Empty<string>()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToList();
   var result=new SkillFixedTargetValidationResult{ok=true};
   foreach(var id in ids){var target=snapshot.actors.Find(x=>x.actorId==id);if(IsValid(snapshot,source,target,request.category,request.range,request.excludeSelf))result.validTargetIds.Add(id);}
   return result;
  }
  static bool IsValid(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord source,BattleActorSaveRecord target,SkillTargetCategory category,SkillTargetRange range,bool excludeSelf)
  {
   if(target==null||(excludeSelf&&target.actorId==source.actorId))return false;
   if(category==SkillTargetCategory.CORPSE)return !target.alive||target.hp<=0;
   if(!target.alive||target.hp<=0)return false;
   if(category==SkillTargetCategory.SELF)return target.actorId==source.actorId;
   if(category==SkillTargetCategory.ALLY){if(target.actorId==source.actorId||string.IsNullOrWhiteSpace(source.teamId)||target.teamId!=source.teamId)return false;return true;}
   if(category!=SkillTargetCategory.ENEMY||string.IsNullOrWhiteSpace(source.teamId)||target.teamId==source.teamId)return false;
   if(range!=SkillTargetRange.SINGLE&&range!=SkillTargetRange.FRONT)return true;
   var enemyRows=snapshot.actors.Where(x=>x!=null&&x.alive&&x.hp>0&&x.teamId!=source.teamId&&!string.IsNullOrWhiteSpace(x.teamId)).Select(x=>x.formationRow).ToList();
   return enemyRows.Count>0&&target.formationRow==enemyRows.Min();
  }
  static SkillFixedTargetValidationResult Fail(string reason)=>new SkillFixedTargetValidationResult{ok=false,reason=reason};
 }
}
