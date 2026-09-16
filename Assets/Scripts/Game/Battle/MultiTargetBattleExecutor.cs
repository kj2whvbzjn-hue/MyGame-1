using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.Battle
{
 public sealed class MultiTargetAttackRequest{public ActionReservationSaveRecord reservation;public Func<string,BattleAttackProposal> buildAttack;public IEnumerable<CoverContract> coverContracts;}
 public sealed class MultiTargetBattleResult{public bool ok;public string reason;public readonly List<string> executedTargetIds=new List<string>();public readonly List<string> skippedTargetIds=new List<string>();public readonly List<BattleStepResult> targetResults=new List<BattleStepResult>();}
 public static class MultiTargetBattleExecutor
 {
  public static MultiTargetBattleResult Execute(BattleSnapshotSaveRecord snapshot,MultiTargetAttackRequest request,IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng){if(snapshot==null||request==null||request.reservation==null||request.buildAttack==null)return Fail("MULTI_TARGET_INPUT_INVALID");var reservation=request.reservation;if(reservation.fixedTargetIds==null||reservation.fixedTargetIds.Count==0)return Fail("MULTI_TARGET_FIXED_TARGETS_EMPTY");var result=new MultiTargetBattleResult{ok=true};var range=SkillTargetRange.SINGLE;var hasRange=reservation.hasTargetContract&&Enum.TryParse(reservation.targetRange,true,out range);foreach(var fixedTargetId in reservation.fixedTargetIds){var actor=snapshot.actors.Find(x=>x.actorId==fixedTargetId);if(actor==null||!actor.alive||actor.hp<=0){result.skippedTargetIds.Add(fixedTargetId);continue;}var targetId=fixedTargetId;if(hasRange){var cover=CoverRuntime.Resolve(snapshot,fixedTargetId,range,request.coverContracts);if(!cover.ok){result.ok=false;result.reason=cover.reason;return result;}targetId=cover.targetId;}var target=snapshot.actors.Find(x=>x.actorId==targetId);if(target==null||!target.alive||target.hp<=0){result.skippedTargetIds.Add(fixedTargetId);continue;}var p=request.buildAttack(targetId);if(p==null){result.ok=false;result.reason="MULTI_TARGET_PROPOSAL_MISSING";return result;}p.reservationId=reservation.reservationId;p.sourceId=reservation.actorId;p.skillId=reservation.skillId;p.reservedTargetId=fixedTargetId;p.targetId=targetId;var step=BattleStepExecutor.ExecuteAttack(snapshot,p,reservation,criticalRng,hitRng,blockRng);if(!step.ok){result.ok=false;result.reason=step.reason;return result;}result.executedTargetIds.Add(targetId);result.targetResults.Add(step);var source=snapshot.actors.Find(x=>x.actorId==reservation.actorId);if(source==null||!source.alive||source.hp<=0)break;}return result;}
  static MultiTargetBattleResult Fail(string r)=>new MultiTargetBattleResult{ok=false,reason=r};
 }
}
