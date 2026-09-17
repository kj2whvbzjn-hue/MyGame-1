using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class ReviveRequest
 {
  public string targetId;
  public int hpAmount,mpAmount;
  public Action<BattleActorSaveRecord> reviveEffect;
 }
 public sealed class ReviveResult{public bool ok;public string reason;public bool revived;public int hpAfter,mpAfter;}
 public static class ReviveRuntime
 {
  public static ReviveResult Execute(BattleSnapshotSaveRecord snapshot,ReviveRequest request)
  {
   if(snapshot==null||request==null||string.IsNullOrWhiteSpace(request.targetId))return Fail("REVIVE_INPUT_INVALID");
   var actor=(snapshot.actors??new List<BattleActorSaveRecord>()).Find(x=>x!=null&&x.actorId==request.targetId);
   if(actor==null)return Fail("REVIVE_TARGET_MISSING");
   if(actor.alive&&actor.hp>0)return new ReviveResult{ok=true,revived=false,hpAfter=actor.hp,mpAfter=actor.mp};
   if(actor.maxHp<=0)return Fail("REVIVE_TARGET_MAX_HP_INVALID");
   actor.alive=true;
   actor.hp=Math.Min(actor.maxHp,Math.Max(1,request.hpAmount));
   actor.mp=Math.Min(actor.maxMp,Math.Max(0,request.mpAmount));
   actor.appliedEffects?.Clear();
   actor.barrierLayers?.Clear();
   actor.cast=null;
   request.reviveEffect?.Invoke(actor);
   return new ReviveResult{ok=true,revived=true,hpAfter=actor.hp,mpAfter=actor.mp};
  }
  static ReviveResult Fail(string reason)=>new ReviveResult{ok=false,reason=reason};
 }
}
