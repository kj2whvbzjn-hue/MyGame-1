using System;

namespace GuildAdventure.Game.Skills
{
    public enum SkillTargetKind { SELF, ALLY, ENEMY, ALL_ALLIES, ALL_ENEMIES }
    public enum SkillResourceKind { NONE, MP, HP }

    [Serializable]
    public sealed class SkillDefinition
    {
        public string id,name;
        public SkillTargetKind target;
        public SkillResourceKind resource;
        public int resourceCost;
        public int castTicks;
        public int cooldownTicks;
        public bool enabled=true;
    }

    [Serializable]
    public sealed class SkillActorState
    {
        public string actorId;
        public int hp,maxHp,mp,maxMp;
        public bool alive=true;
        public bool silenced;
    }

    public sealed class SkillUseCheck
    {
        public bool ok;
        public string reason;
    }

    public static class SkillUseCondition
    {
        public static SkillUseCheck CanUse(SkillDefinition skill,SkillActorState actor)
        {
            if(skill==null||actor==null)return Fail("skill_or_actor_missing");
            if(!skill.enabled)return Fail("skill_disabled");
            if(!actor.alive||actor.hp<=0)return Fail("actor_dead");
            if(actor.silenced&&skill.castTicks>0)return Fail("actor_silenced");
            if(skill.resourceCost<0)return Fail("resource_cost_invalid");
            if(skill.resource==SkillResourceKind.MP&&actor.mp<skill.resourceCost)return Fail("mp_insufficient");
            if(skill.resource==SkillResourceKind.HP&&actor.hp<=skill.resourceCost)return Fail("hp_insufficient");
            return new SkillUseCheck{ok=true};
        }

        public static void PayCost(SkillDefinition skill,SkillActorState actor)
        {
            var check=CanUse(skill,actor);
            if(!check.ok)throw new InvalidOperationException(check.reason);
            if(skill.resource==SkillResourceKind.MP)actor.mp-=skill.resourceCost;
            else if(skill.resource==SkillResourceKind.HP)actor.hp-=skill.resourceCost;
        }

        private static SkillUseCheck Fail(string r)=>new SkillUseCheck{ok=false,reason=r};
    }
}
