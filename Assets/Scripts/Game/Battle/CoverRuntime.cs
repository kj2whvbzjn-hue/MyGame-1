using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.Battle
{
 public enum CoverLifetimeKind{PERSISTENT,USES,DURATION}
 [Serializable] public sealed class CoverContract
 {
  public string id,protectorId,protectedActorId;
  public CoverLifetimeKind lifetime;
  public int remainingUses,remainingTicks;
  public bool active=true;
 }
 public sealed class CoverResolveResult{public bool ok;public string reason;public string targetId;public CoverContract consumedCover;}
 public static class CoverRuntime
 {
  public static CoverResolveResult Resolve(BattleSnapshotSaveRecord snapshot,string originalTargetId,SkillTargetRange range,IEnumerable<CoverContract> contracts)
  {
   if(snapshot==null||string.IsNullOrWhiteSpace(originalTargetId))return Fail("COVER_INPUT_INVALID");
   if(range==SkillTargetRange.FRONT||range==SkillTargetRange.ALL)return Ok(originalTargetId,null);
   if(range!=SkillTargetRange.SINGLE&&range!=SkillTargetRange.BACK&&range!=SkillTargetRange.RANDOM)return Ok(originalTargetId,null);
   var valid=(contracts??Array.Empty<CoverContract>()).Where(x=>Eligible(snapshot,x,originalTargetId)).ToList();
   if(valid.Count==0)return Ok(originalTargetId,null);
   // GS-13 deliberately leaves simultaneous-candidate priority to an individual contract.
   // Fail closed instead of inventing an order from id/list/formation.
   if(valid.Count>1)return Fail("COVER_PRIORITY_CONTRACT_REQUIRED");
   var cover=valid[0];
   if(cover.lifetime==CoverLifetimeKind.USES){cover.remainingUses=Math.Max(0,cover.remainingUses-1);if(cover.remainingUses==0)cover.active=false;}
   return Ok(cover.protectorId,cover);
  }
  static bool Eligible(BattleSnapshotSaveRecord snapshot,CoverContract c,string targetId)
  {
   if(c==null||!c.active||c.protectedActorId!=targetId||string.IsNullOrWhiteSpace(c.protectorId))return false;
   if(c.lifetime==CoverLifetimeKind.USES&&c.remainingUses<=0)return false;
   if(c.lifetime==CoverLifetimeKind.DURATION&&c.remainingTicks<=0)return false;
   var protector=snapshot.actors.Find(x=>x.actorId==c.protectorId);
   var protectedActor=snapshot.actors.Find(x=>x.actorId==targetId);
   return protector!=null&&protectedActor!=null&&protector.alive&&protector.hp>0&&protector.teamId==protectedActor.teamId;
  }
  static CoverResolveResult Ok(string id,CoverContract c)=>new CoverResolveResult{ok=true,targetId=id,consumedCover=c};
  static CoverResolveResult Fail(string reason)=>new CoverResolveResult{ok=false,reason=reason};
 }
}
