using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    // GS-20 formal trigger vocabulary. Legacy aliases are retained for existing callers.
    public enum TriggerEvent
    {
        ON_USE,
        ON_HIT_RECEIVED,
        ON_DAMAGE_DEALT,
        ON_TURN_START,
        ON_TURN_END,
        ON_DEATH,
        ON_STATUS_APPLIED,
        ON_ALLY_ATTACK,
        WHILE_SOURCE_ALIVE,
        ON_CRITICAL,
        ON_HIT_DEALT,
        ON_EVADE,
        ON_BLOCK,
        ON_BATTLE_START,
        ON_FATAL_DAMAGE,
        BATTLE_START=ON_BATTLE_START,
        TURN_START=ON_TURN_START,
        BEFORE_ACTION=ON_USE,
        AFTER_ACTION=ON_TURN_END,
        ON_HIT=ON_HIT_DEALT,
        ON_DAMAGE=ON_DAMAGE_DEALT
    }

    [Serializable]
    public sealed class TriggerRegistration
    {
        public string id,ownerId;
        public TriggerEvent trigger;
        public int priority;
        public int sequence;
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
                .ThenBy(x=>x.sequence)
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
