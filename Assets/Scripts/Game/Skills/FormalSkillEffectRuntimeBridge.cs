using System;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Game.Skills
{
    public static class FormalSkillEffectRuntimeBridge
    {
        public static SkillEffectRequest BuildApplyRequest(CompiledSkillEffect effect,string sourceId,string instanceId,double targetResistancePercent,int sequence=0)
        {
            if(effect==null)throw new ArgumentNullException(nameof(effect));
            if(effect.kind!=FormalEffectKind.APPLY)throw new ArgumentException("SKILL_EFFECT_NOT_APPLY",nameof(effect));
            if(string.IsNullOrWhiteSpace(effect.statusId))throw new ArgumentException("SKILL_STATUS_ID_REQUIRED",nameof(effect));
            return new SkillEffectRequest
            {
                kind=SkillEffectKind.APPLY,
                sourceId=sourceId,
                effectId=effect.statusId,
                instanceId=instanceId,
                lifecycleKind=effect.lifecycleKind,
                stackRule=effect.stackRule,
                refreshRule=effect.refreshRule,
                snapshotPolicy=effect.snapshotPolicy,
                dispelCategory=effect.dispelCategory,
                removeOnDeath=effect.removeOnDeath,
                removeOnBattleEnd=effect.removeOnBattleEnd,
                durationTicks=effect.duration,
                sequence=sequence,
                maxStacks=effect.maxStacks,
                power=effect.power,
                statusResistancePercent=targetResistancePercent,
                statusResistanceCapPercent=effect.resistanceCapPercent,
                removable=effect.removable,
                protectedEffect=effect.protectedEffect,
                normalCleanseEligible=effect.normalCleanseEligible,
                actionDisabled=effect.actionDisabled
            };
        }
    }
}
