using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum TriggerEvent
    {
        ON_USE,ON_HIT_RECEIVED,ON_DAMAGE_DEALT,ON_TURN_START,ON_TURN_END,ON_DEATH,ON_STATUS_APPLIED,
        ON_ALLY_ATTACK,WHILE_SOURCE_ALIVE,ON_CRITICAL,ON_HIT_DEALT,ON_EVADE,ON_BLOCK,ON_BATTLE_START,ON_BATTLE_END,ON_FATAL_DAMAGE,
        BATTLE_START=ON_BATTLE_START,BATTLE_END=ON_BATTLE_END,TURN_START=ON_TURN_START,BEFORE_ACTION=ON_USE,AFTER_ACTION=ON_TURN_END,
        ON_HIT=ON_HIT_DEALT,ON_DAMAGE=ON_DAMAGE_DEALT
    }
    public enum ReactiveFamily{NORMAL,COUNTER,FOLLOW_UP}

    [Serializable] public sealed class TriggerRegistration
    {
        public string id,ownerId;
        public TriggerEvent trigger;
        public ReactiveFamily reactiveFamily=ReactiveFamily.NORMAL;
        public int priority,sequence;
        public bool once,consumed;
        public double activationChance=1d;
    }

    public static class TriggerRuntime
    {
        public static List<TriggerRegistration> Resolve(IEnumerable<TriggerRegistration> registrations,TriggerEvent trigger,
            IReadOnlyDictionary<string,int> fixedBattleOrder)
        {
            return (registrations??Array.Empty<TriggerRegistration>()).Where(x=>x!=null&&!x.consumed&&x.trigger==trigger)
                .OrderByDescending(x=>x.priority).ThenBy(x=>x.sequence)
                .ThenBy(x=>fixedBattleOrder!=null&&fixedBattleOrder.TryGetValue(x.ownerId,out var n)?n:int.MaxValue)
                .ThenBy(x=>x.id,StringComparer.Ordinal).ToList();
        }
        public static void MarkConsumed(TriggerRegistration r){if(r!=null&&r.once)r.consumed=true;}
    }
}
