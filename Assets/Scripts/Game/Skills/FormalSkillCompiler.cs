using System;
using System.Collections.Generic;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Game.Skills
{
    public enum FormalEffectKind { DAMAGE, HEAL, APPLY, REMOVE, TARGET_CONTROL, REVIVE }

    public sealed class CompiledSkillEffect
    {
        public FormalEffectKind kind;
        public string statusId;
        public double power;
        public int duration;
        public EffectLifecycleKind lifecycleKind;
        public int maxStacks;
        public double resistanceCapPercent=-1d;
        public bool removable=true,protectedEffect,normalCleanseEligible,actionDisabled;
        public string dispelCategory;
    }

    public sealed class CompiledSkill
    {
        public string id;
        public SkillTargetKind target;
        public List<CompiledSkillEffect> effects=new List<CompiledSkillEffect>();
    }

    public static class FormalSkillCompiler
    {
        public static CompiledSkill Compile(SkillExportRow row)=>Compile(row,null);

        public static CompiledSkill Compile(SkillExportRow row,IReadOnlyDictionary<string,StatusMasterRow> statuses)
        {
            if(row==null||string.IsNullOrWhiteSpace(row.id))throw new ArgumentException("SKILL_INVALID");
            var runtime=SkillExportLoader.ToRuntime(row);
            var result=new CompiledSkill{id=row.id,target=runtime.target};

            foreach(var e in row.effects??Array.Empty<SkillEffect>())
            {
                if(!Enum.TryParse((e.type??"").Trim(),true,out FormalEffectKind kind))throw new ArgumentException("SKILL_EFFECT_UNKNOWN:"+e.type);
                var requiresStatus=kind==FormalEffectKind.APPLY||kind==FormalEffectKind.REMOVE;
                if(requiresStatus&&string.IsNullOrWhiteSpace(e.statusId))throw new ArgumentException("SKILL_STATUS_ID_REQUIRED");
                if(kind==FormalEffectKind.REVIVE&&runtime.target==SkillTargetKind.ENEMY)throw new ArgumentException("REVIVE_TARGET_INVALID");

                var compiled=new CompiledSkillEffect{kind=kind,statusId=e.statusId,power=e.power,duration=e.duration};
                if(requiresStatus)
                {
                    if(statuses==null)throw new ArgumentException("STATUS_MASTER_REQUIRED:"+e.statusId);
                    if(!statuses.TryGetValue(e.statusId,out var status)||status==null)throw new ArgumentException("STATUS_ID_UNKNOWN:"+e.statusId);
                    if(!Enum.TryParse((status.lifecycle_kind??"").Trim(),true,out EffectLifecycleKind lifecycleKind))throw new ArgumentException("STATUS_LIFECYCLE_KIND_INVALID:"+e.statusId);
                    compiled.lifecycleKind=lifecycleKind;
                    compiled.maxStacks=status.max_stacks;
                    compiled.resistanceCapPercent=status.resistance_cap_percent;
                    compiled.removable=status.removable;
                    compiled.protectedEffect=status.protected_effect;
                    compiled.normalCleanseEligible=status.normal_cleanse_eligible;
                    compiled.actionDisabled=status.action_disabled;
                    compiled.dispelCategory=status.dispel_category;
                }
                result.effects.Add(compiled);
            }
            return result;
        }
    }
}
