using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Save
{
    [Serializable] public sealed class BattleSnapshotSaveRecord
    {
        public string contract="C01"; public int schemaVersion=1; public string battleId,settingsVersion,seed; public int tick;
        public List<BattleActorSaveRecord> actors=new List<BattleActorSaveRecord>(); public List<string> formation=new List<string>(); public List<string> fixedActorOrder=new List<string>();
        public List<RngStreamSaveRecord> rngStreams=new List<RngStreamSaveRecord>(); public List<ActionReservationSaveRecord> actionReservations=new List<ActionReservationSaveRecord>(); public List<ResolvedHitSaveRecord> resolvedHits=new List<ResolvedHitSaveRecord>();
    }
    [Serializable] public sealed class BattleActorSaveRecord
    {
        public string actorId,aiProgramId; public int hp,maxHp,mp,maxMp; public double actionGauge,speed; public bool alive=true; public CastSaveRecord cast;
        public List<CooldownSaveRecord> cooldowns=new List<CooldownSaveRecord>(); public List<AppliedEffectSaveRecord> appliedEffects=new List<AppliedEffectSaveRecord>();
    }
    [Serializable] public sealed class CastSaveRecord { public string reservationId,skillId,targetId; public int startTick,completeTick,remainingTicks; public bool active; public List<string> fixedTargetIds=new List<string>(); public UsageConditionsSaveRecord usageConditions=new UsageConditionsSaveRecord(); }
    [Serializable] public sealed class CooldownSaveRecord { public string skillId; public int remainingTicks; }
    [Serializable] public sealed class AppliedEffectSaveRecord { public string instanceId,sourceId,effectId,kind; public int remainingTicks,appliedTick,sequence; public double value; public bool consumed; }
    [Serializable] public sealed class RngStreamSaveRecord { public string purpose; public int cursor; public List<double> recordedRolls=new List<double>(); }

    public static class BattleSnapshotValidation
    {
        public static string Validate(BattleSnapshotSaveRecord b)
        {
            if(b==null)return null;if(b.contract!="C01"||b.schemaVersion!=1)return "BATTLE_CONTRACT_INVALID";if(string.IsNullOrWhiteSpace(b.battleId))return "BATTLE_ID_MISSING";if(string.IsNullOrWhiteSpace(b.settingsVersion))return "BATTLE_SETTINGS_VERSION_MISSING";if(string.IsNullOrWhiteSpace(b.seed))return "BATTLE_SEED_MISSING";if(b.tick<0)return "BATTLE_TICK_INVALID";
            var actorIds=new HashSet<string>();foreach(var a in b.actors??new List<BattleActorSaveRecord>()){if(a==null||string.IsNullOrWhiteSpace(a.actorId)||!actorIds.Add(a.actorId))return "BATTLE_ACTOR_INVALID_OR_DUPLICATE";if(a.hp<0||a.maxHp<0||a.hp>a.maxHp||a.mp<0||a.maxMp<0||a.mp>a.maxMp)return "BATTLE_RESOURCE_INVALID";if(a.actionGauge<0||a.speed<0)return "BATTLE_GAUGE_INVALID";var skills=new HashSet<string>();foreach(var c in a.cooldowns??new List<CooldownSaveRecord>())if(c==null||string.IsNullOrWhiteSpace(c.skillId)||c.remainingTicks<0||!skills.Add(c.skillId))return "BATTLE_COOLDOWN_INVALID";var effects=new HashSet<string>();foreach(var e in a.appliedEffects??new List<AppliedEffectSaveRecord>())if(e==null||string.IsNullOrWhiteSpace(e.instanceId)||e.remainingTicks<0||e.appliedTick<0||e.sequence<0||!effects.Add(e.instanceId))return "BATTLE_EFFECT_INVALID";if(a.cast!=null&&a.cast.remainingTicks<0)return "BATTLE_CAST_INVALID";}
            var order=new HashSet<string>();foreach(var id in b.fixedActorOrder??new List<string>())if(string.IsNullOrWhiteSpace(id)||!actorIds.Contains(id)||!order.Add(id))return "BATTLE_ORDER_INVALID";if(order.Count!=actorIds.Count)return "BATTLE_ORDER_INCOMPLETE";
            var reservations=new HashSet<string>();foreach(var x in b.actionReservations??new List<ActionReservationSaveRecord>()){if(x==null||x.contract!="C02"||x.schemaVersion!=1||string.IsNullOrWhiteSpace(x.reservationId)||!reservations.Add(x.reservationId))return "ACTION_RESERVATION_INVALID_OR_DUPLICATE";if(!actorIds.Contains(x.actorId)||x.startTick<0||x.completeTick<x.startTick)return "ACTION_RESERVATION_STATE_INVALID";foreach(var id in x.fixedTargetIds??new List<string>())if(!actorIds.Contains(id))return "ACTION_RESERVATION_TARGET_INVALID";}
            foreach(var x in b.resolvedHits??new List<ResolvedHitSaveRecord>()){if(x==null||x.contract!="C03"||x.schemaVersion!=1||string.IsNullOrWhiteSpace(x.actionId)||x.hitIndex<0)return "RESOLVED_HIT_INVALID";if(!actorIds.Contains(x.sourceId)||!actorIds.Contains(x.targetId)||x.committedHp<0||x.actualHpLoss<0)return "RESOLVED_HIT_STATE_INVALID";}
            var purposes=new HashSet<string>();foreach(var r in b.rngStreams??new List<RngStreamSaveRecord>()){if(r==null||string.IsNullOrWhiteSpace(r.purpose)||!purposes.Add(r.purpose))return "BATTLE_RNG_PURPOSE_INVALID";if(r.cursor<0||r.recordedRolls==null||r.cursor>r.recordedRolls.Count)return "BATTLE_RNG_CURSOR_INVALID";foreach(var roll in r.recordedRolls)if(roll<0||roll>=1)return "BATTLE_RNG_ROLL_INVALID";}return null;
        }
    }
}
