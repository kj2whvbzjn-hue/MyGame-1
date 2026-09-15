using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum ConditionProperty { TARGET_POISONED }

    [Serializable]
    public sealed class CompiledCondition
    {
        public string scope;
        public ConditionProperty property;
        public string enginePredicate;
        public bool expected;
    }

    public static class ConditionRuntime
    {
        public static bool Validate(CompiledCondition c)
            => c!=null && c.scope=="TARGET" && c.property==ConditionProperty.TARGET_POISONED
               && c.enginePredicate=="target_poisoned" && c.expected;

        public static bool Evaluate(CompiledCondition c,IEnumerable<AppliedEffect> targetEffects)
        {
            if(!Validate(c))throw new ArgumentException("CONDITION_CONTRACT_INVALID");
            return (targetEffects??Array.Empty<AppliedEffect>())
                .Any(x=>!x.consumed && x.kind==ApplyKind.STATUS &&
                        string.Equals(x.effectId,"POISON",StringComparison.OrdinalIgnoreCase));
        }
    }
}
