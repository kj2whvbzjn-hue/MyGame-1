using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Skills
{
    public enum FormalEffectKind { DAMAGE, HEAL, APPLY, REMOVE, TARGET_CONTROL, REVIVE }

    public sealed class CompiledSkillEffect
    {
        public FormalEffectKind kind;
        public string statusId;
        public double power;
        public int duration;
    }

    public sealed class CompiledSkill
    {
        public string id;
        public SkillTargetKind target;
        public List<CompiledSkillEffect> effects=new List<CompiledSkillEffect>();
    }

    public static class FormalSkillCompiler
    {
        public static CompiledSkill Compile(SkillExportRow row)
        {
            if(row==null||string.IsNullOrWhiteSpace(row.id))
                throw new ArgumentException("SKILL_INVALID");

            var runtime=SkillExportLoader.ToRuntime(row);
            var result=new CompiledSkill{id=row.id,target=runtime.target};

            foreach(var e in row.effects??Array.Empty<SkillEffect>())
            {
                if(!Enum.TryParse((e.type??"").Trim(),true,out FormalEffectKind kind))
                    throw new ArgumentException("SKILL_EFFECT_UNKNOWN:"+e.type);

                if((kind==FormalEffectKind.APPLY||kind==FormalEffectKind.REMOVE) &&
                   string.IsNullOrWhiteSpace(e.statusId))
                    throw new ArgumentException("SKILL_STATUS_ID_REQUIRED");

                if(kind==FormalEffectKind.REVIVE && runtime.target==SkillTargetKind.ENEMY)
                    throw new ArgumentException("REVIVE_TARGET_INVALID");

                result.effects.Add(new CompiledSkillEffect{
                    kind=kind,statusId=e.statusId,power=e.power,duration=e.duration});
            }
            return result;
        }
    }
}
