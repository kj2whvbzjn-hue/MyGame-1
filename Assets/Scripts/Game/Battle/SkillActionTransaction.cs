using System;
using System.Linq;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Game.Battle
{
    public sealed class SkillActionRequest
    {
        public SkillDefinition skill;
        public BattleAttackProposal attack;
    }

    public sealed class SkillActionResult
    {
        public bool ok;
        public string reason;
        public bool castingStarted;
        public BattleStepResult battleStep;
    }

    public static class SkillActionTransaction
    {
        public static SkillActionResult Execute(
            BattleSnapshotSaveRecord snapshot,SkillActionRequest request,
            IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            if(snapshot==null||request==null||request.skill==null||request.attack==null)return Fail("SKILL_ACTION_INPUT_INVALID");
            var actor=snapshot.actors.FirstOrDefault(x=>x.actorId==request.attack.sourceId);
            if(actor==null)return Fail("SKILL_ACTION_ACTOR_MISSING");
            if(actor.actionGauge<ActionGauge.ReadyThreshold)return Fail("ACTION_GAUGE_NOT_READY");
            if(actor.cast!=null&&actor.cast.active)return Fail("CAST_ALREADY_ACTIVE");
            if(actor.cooldowns.Any(x=>x.skillId==request.skill.id&&x.remainingTicks>0))return Fail("SKILL_COOLDOWN_ACTIVE");

            var runtimeActor=new SkillActorState{
                actorId=actor.actorId,hp=actor.hp,maxHp=actor.maxHp,mp=actor.mp,maxMp=actor.maxMp,alive=actor.alive};
            var check=SkillUseCondition.CanUse(request.skill,runtimeActor);
            if(!check.ok)return Fail(check.reason);

            // Validate before mutation. Cost/gauge/cooldown are committed only after all preconditions above pass.
            SkillUseCondition.PayCost(request.skill,runtimeActor);
            actor.hp=runtimeActor.hp; actor.mp=runtimeActor.mp;
            actor.actionGauge=Math.Max(0,actor.actionGauge-ActionGauge.ReadyThreshold);

            if(request.skill.castTicks>0)
            {
                actor.cast=new CastSaveRecord{
                    skillId=request.skill.id,targetId=request.attack.targetId,
                    remainingTicks=request.skill.castTicks,active=true};
                if(request.skill.cooldownTicks>0)
                    actor.cooldowns.Add(new CooldownSaveRecord{skillId=request.skill.id,remainingTicks=request.skill.cooldownTicks});
                return new SkillActionResult{ok=true,castingStarted=true};
            }

            var step=BattleStepExecutor.ExecuteAttack(snapshot,request.attack,criticalRng,hitRng,blockRng);
            if(!step.ok)
            {
                // Roll back local cost/gauge if attack proposal itself fails before a terminal reservation is committed.
                actor.hp += request.skill.resource==SkillResourceKind.HP?request.skill.resourceCost:0;
                actor.mp += request.skill.resource==SkillResourceKind.MP?request.skill.resourceCost:0;
                actor.actionGauge += ActionGauge.ReadyThreshold;
                return Fail(step.reason);
            }
            if(request.skill.cooldownTicks>0)
                actor.cooldowns.Add(new CooldownSaveRecord{skillId=request.skill.id,remainingTicks=request.skill.cooldownTicks});
            return new SkillActionResult{ok=true,battleStep=step};
        }

        public static bool AdvanceCastAndCooldowns(BattleActorSaveRecord actor)
        {
            if(actor==null)throw new ArgumentNullException(nameof(actor));
            foreach(var c in actor.cooldowns)c.remainingTicks=Math.Max(0,c.remainingTicks-1);
            actor.cooldowns.RemoveAll(x=>x.remainingTicks<=0);
            if(actor.cast==null||!actor.cast.active)return false;
            actor.cast.remainingTicks=Math.Max(0,actor.cast.remainingTicks-1);
            if(actor.cast.remainingTicks==0){actor.cast.active=false;return true;}
            return false;
        }

        static SkillActionResult Fail(string reason)=>new SkillActionResult{ok=false,reason=reason};
    }
}
