using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Game.Battle
{
    public sealed class SkillActionRequest { public SkillDefinition skill; public BattleAttackProposal attack; }
    public sealed class SkillActionResult { public bool ok; public string reason; public bool castingStarted,executionSkipped; public BattleStepResult battleStep; public ActionReservationSaveRecord reservation; }
    public static class SkillActionTransaction
    {
        public static SkillActionResult Execute(BattleSnapshotSaveRecord snapshot,SkillActionRequest request,IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            var precheck=Precheck(snapshot,request);if(!precheck.ok)return precheck;var actor=snapshot.actors.First(x=>x.actorId==request.attack.sourceId);var reservation=precheck.reservation;
            // GS-15 reservation/cast start has no resource, cooldown or gauge execution cost.
            snapshot.actionReservations.Add(reservation);
            if(request.skill.castTicks>0){actor.cast=new CastSaveRecord{reservationId=reservation.reservationId,skillId=reservation.skillId,targetId=reservation.fixedTargetIds.FirstOrDefault(),startTick=reservation.startTick,completeTick=reservation.completeTick,remainingTicks=Math.Max(0,reservation.completeTick-snapshot.tick),active=true,fixedTargetIds=new List<string>(reservation.fixedTargetIds),usageConditions=reservation.usageConditions};return new SkillActionResult{ok=true,castingStarted=true,reservation=reservation};}
            return ExecuteReserved(snapshot,request.skill,request.attack,reservation,criticalRng,hitRng,blockRng);
        }
        public static SkillActionResult CompleteCast(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord actor,SkillDefinition skill,BattleAttackProposal attack,IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            if(snapshot==null||actor==null||skill==null||attack==null)return Fail("CAST_COMPLETION_INPUT_INVALID");var cast=actor.cast;if(cast==null||cast.active||cast.remainingTicks>0)return Fail("CAST_NOT_COMPLETE");if(string.IsNullOrWhiteSpace(cast.reservationId))return Fail("CAST_RESERVATION_MISSING");var reservation=snapshot.actionReservations.FirstOrDefault(x=>x.reservationId==cast.reservationId);if(reservation==null)return Fail("CAST_RESERVATION_MISSING");if(reservation.actorId!=actor.actorId||reservation.skillId!=skill.id)return Fail("CAST_RESERVATION_MISMATCH");var fixedTargetId=reservation.fixedTargetIds.FirstOrDefault();attack.reservationId=reservation.reservationId;attack.sourceId=reservation.actorId;attack.skillId=reservation.skillId;attack.targetId=fixedTargetId;return ExecuteReserved(snapshot,skill,attack,reservation,criticalRng,hitRng,blockRng);
        }
        public static SkillActionResult Precheck(BattleSnapshotSaveRecord snapshot,SkillActionRequest request)
        {
            if(snapshot==null||request==null||request.skill==null||request.attack==null)return Fail("SKILL_ACTION_INPUT_INVALID");var actor=snapshot.actors.FirstOrDefault(x=>x.actorId==request.attack.sourceId);if(actor==null)return Fail("SKILL_ACTION_ACTOR_MISSING");var gaugeSettings=new ActionGaugeSettings();if(actor.actionGauge<gaugeSettings.maxGauge)return Fail("ACTION_GAUGE_NOT_READY");if(actor.cast!=null&&actor.cast.active)return Fail("CAST_ALREADY_ACTIVE");if(actor.cooldowns.Any(x=>x.skillId==request.skill.id&&x.remainingTicks>0))return Fail("SKILL_COOLDOWN_ACTIVE");if(string.IsNullOrWhiteSpace(request.attack.reservationId))return Fail("ACTION_RESERVATION_ID_MISSING");if(snapshot.actionReservations.Any(x=>x.reservationId==request.attack.reservationId))return Fail("ACTION_RESERVATION_ID_DUPLICATE");var target=snapshot.actors.FirstOrDefault(x=>x.actorId==request.attack.targetId);if(target==null||!target.alive||target.hp<=0)return Fail("TARGET_INVALID_AT_PRECHECK");var check=SkillUseCondition.CanUse(request.skill,ToRuntimeActor(actor));if(!check.ok)return Fail(check.reason);var completeTick=snapshot.tick+Math.Max(0,request.skill.castTicks);var reservation=new ActionReservationSaveRecord{reservationId=request.attack.reservationId,actorId=actor.actorId,skillId=request.skill.id,startTick=snapshot.tick,completeTick=completeTick,fixedTargetIds=new List<string>{target.actorId},usageConditions=new UsageConditionsSaveRecord()};return new SkillActionResult{ok=true,reservation=reservation};
        }
        static SkillActionResult ExecuteReserved(BattleSnapshotSaveRecord snapshot,SkillDefinition skill,BattleAttackProposal attack,ActionReservationSaveRecord reservation,IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            var actor=snapshot.actors.FirstOrDefault(x=>x.actorId==reservation.actorId);if(actor==null)return Fail("SKILL_ACTION_ACTOR_MISSING");var runtimeActor=ToRuntimeActor(actor);var executionCheck=SkillUseCondition.CanUse(skill,runtimeActor);if(!executionCheck.ok){ConsumeGauge(actor,false);return new SkillActionResult{ok=false,reason=executionCheck.reason,reservation=reservation};}
            var fixedTargetId=reservation.fixedTargetIds.FirstOrDefault();var target=snapshot.actors.FirstOrDefault(x=>x.actorId==fixedTargetId);if(target==null||!target.alive||target.hp<=0){ConsumeGauge(actor,false);return new SkillActionResult{ok=true,executionSkipped=true,reason="TARGET_INVALID_AT_EFFECT_START",reservation=reservation};}
            if(skill.resource==SkillResourceKind.HP&&skill.resourceCost>0)return Fail("HP_COST_NOT_SUPPORTED_BY_FORMAL_RUNTIME");SkillUseCondition.PayCost(skill,runtimeActor);actor.hp=runtimeActor.hp;actor.mp=runtimeActor.mp;StartCooldown(actor,skill);attack.reservationId=reservation.reservationId;attack.sourceId=reservation.actorId;attack.skillId=reservation.skillId;attack.targetId=fixedTargetId;var step=BattleStepExecutor.ExecuteAttack(snapshot,attack,reservation,criticalRng,hitRng,blockRng);if(!step.ok)return Fail(step.reason);ConsumeGauge(actor,true);return new SkillActionResult{ok=true,battleStep=step,reservation=reservation};
        }
        static void ConsumeGauge(BattleActorSaveRecord actor,bool success){var s=new ActionGaugeState{actorId=actor.actorId,gauge=actor.actionGauge,alive=actor.alive};var cfg=new ActionGaugeSettings();if(success)ActionGauge.ConsumeSuccessful(s,cfg);else ActionGauge.ConsumeFailedExecution(s,cfg);actor.actionGauge=s.gauge;}
        public static bool AdvanceCastAndCooldowns(BattleActorSaveRecord actor){if(actor==null)throw new ArgumentNullException(nameof(actor));foreach(var c in actor.cooldowns)c.remainingTicks=Math.Max(0,c.remainingTicks-1);actor.cooldowns.RemoveAll(x=>x.remainingTicks<=0);if(actor.cast==null||!actor.cast.active)return false;actor.cast.remainingTicks=Math.Max(0,actor.cast.remainingTicks-1);if(actor.cast.remainingTicks==0){actor.cast.active=false;return true;}return false;}
        static SkillActorState ToRuntimeActor(BattleActorSaveRecord actor)=>new SkillActorState{actorId=actor.actorId,hp=actor.hp,maxHp=actor.maxHp,mp=actor.mp,maxMp=actor.maxMp,alive=actor.alive};
        static void StartCooldown(BattleActorSaveRecord actor,SkillDefinition skill){if(skill.cooldownTicks<=0)return;actor.cooldowns.RemoveAll(x=>x.skillId==skill.id);actor.cooldowns.Add(new CooldownSaveRecord{skillId=skill.id,remainingTicks=skill.cooldownTicks});}
        static SkillActionResult Fail(string reason)=>new SkillActionResult{ok=false,reason=reason};
    }
}
