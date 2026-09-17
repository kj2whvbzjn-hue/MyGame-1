using System;
using System.Linq;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class ForcedMovementRequest{public string targetId;public int destinationFormationIndex;public Func<BattleSnapshotSaveRecord,BattleActorSaveRecord,int,bool> canEstablish;public Action<BattleSnapshotSaveRecord,BattleActorSaveRecord> updateRangeAndTargets;public Action<BattleSnapshotSaveRecord,BattleActorSaveRecord> requestAiReevaluation;}
 public sealed class ForcedMovementResult{public bool ok;public string reason;public bool established,castInterrupted,moved,rangeAndTargetsUpdated,aiReevaluationRequested;public int fromFormationIndex,toFormationIndex;}
 public static class ForcedMovementRuntime
 {
  public static ForcedMovementResult Execute(BattleSnapshotSaveRecord snapshot,ForcedMovementRequest request)
  {
   if(snapshot==null||request==null||string.IsNullOrWhiteSpace(request.targetId))return Fail("FORCED_MOVEMENT_INPUT_INVALID");
   var actor=snapshot.actors.FirstOrDefault(x=>x!=null&&x.actorId==request.targetId);if(actor==null)return Fail("FORCED_MOVEMENT_TARGET_MISSING");if(!actor.alive||actor.hp<=0)return Fail("FORCED_MOVEMENT_TARGET_DEAD");
   if(request.canEstablish!=null&&!request.canEstablish(snapshot,actor,request.destinationFormationIndex))return new ForcedMovementResult{ok=true,reason="FORCED_MOVEMENT_NOT_ESTABLISHED",fromFormationIndex=actor.formationRow,toFormationIndex=actor.formationRow};
   var result=new ForcedMovementResult{ok=true,established=true,fromFormationIndex=actor.formationRow,toFormationIndex=request.destinationFormationIndex};
   if(actor.cast!=null&&actor.cast.active){var reservationId=actor.cast.reservationId;actor.cast.active=false;actor.cast.remainingTicks=0;if(snapshot.actionReservations!=null&&!string.IsNullOrWhiteSpace(reservationId))snapshot.actionReservations.RemoveAll(x=>x!=null&&x.reservationId==reservationId);result.castInterrupted=true;}
   actor.formationRow=request.destinationFormationIndex;result.moved=true;
   request.updateRangeAndTargets?.Invoke(snapshot,actor);result.rangeAndTargetsUpdated=request.updateRangeAndTargets!=null;
   request.requestAiReevaluation?.Invoke(snapshot,actor);result.aiReevaluationRequested=request.requestAiReevaluation!=null;
   return result;
  }
  static ForcedMovementResult Fail(string reason)=>new ForcedMovementResult{ok=false,reason=reason};
 }
}
