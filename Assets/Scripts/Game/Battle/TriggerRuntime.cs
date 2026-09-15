using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum TriggerEvent { BATTLE_START, TURN_START, BEFORE_ACTION, AFTER_ACTION, ON_HIT, ON_DAMAGE, ON_DEATH }

    [Serializable]
    public sealed class TriggerRegistration
    {
        public string id,ownerId;
        public TriggerEvent trigger;
        public int priority;
        public bool once;
        public bool consumed;
    }

    public static class TriggerRuntime
    {
        public static List<TriggerRegistration> Resolve(
            IEnumerable<TriggerRegistration> registrations,
            TriggerEvent trigger,
            IReadOnlyDictionary<string,int> fixedBattleOrder)
        {
            return (registrations??Array.Empty<TriggerRegistration>())
                .Where(x=>x!=null&&!x.consumed&&x.trigger==trigger)
                .OrderByDescending(x=>x.priority)
                .ThenBy(x=>fixedBattleOrder!=null&&fixedBattleOrder.TryGetValue(x.ownerId,out var n)?n:int.MaxValue)
                .ThenBy(x=>x.id,StringComparer.Ordinal)
                .ToList();
        }

        public static void MarkConsumed(TriggerRegistration r)
        {
            if(r!=null&&r.once)r.consumed=true;
        }
    }
}
