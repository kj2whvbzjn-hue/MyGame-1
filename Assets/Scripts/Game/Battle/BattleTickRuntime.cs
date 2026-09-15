using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleTickResult
    {
        public bool ok=true; public string reason; public int tick;
        public List<string> expiredEffects=new List<string>(); public List<string> dotSources=new List<string>();
    }

    public static class BattleTickRuntime
    {
        // GS-14 order implemented here: fixed actor order -> periodic effects -> cooldown -> expire -> gauge.
        // Action reservation/AI execution remains the caller's final phase until the AI V2 bridge is attached.
        public static BattleTickResult Advance(BattleSnapshotSaveRecord snapshot)
        {
            if(snapshot==null)return Fail("BATTLE_TICK_SNAPSHOT_MISSING");
            var validation=BattleSnapshotValidation.Validate(snapshot);if(validation!=null)return Fail(validation);
            var result=new BattleTickResult{tick=snapshot.tick};
            foreach(var id in snapshot.fixedActorOrder)
            {
                var actor=snapshot.actors.Find(x=>x.actorId==id);if(actor==null)continue;
                TickPeriodic(actor,result);
                TickCooldowns(actor);
                ExpireEffects(actor,result);
                if(actor.alive&&actor.hp>0&&!(actor.cast?.active??false))actor.actionGauge=Math.Min(100,actor.actionGauge+ActionGaugeGs14.GainPerTick(actor.speed));
            }
            snapshot.tick++;result.tick=snapshot.tick;return result;
        }

        static void TickPeriodic(BattleActorSaveRecord actor,BattleTickResult result)
        {
            foreach(var e in (actor.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x!=null&&!x.consumed&&x.remainingTicks>0).OrderBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.instanceId,StringComparer.Ordinal))
            {
                if(e.kind==EffectLifecycleKind.DOT.ToString()&&actor.alive&&actor.hp>0)
                {
                    var damage=EffectLifecycleRuntime.DotDamage(e.value);var hp=DamageDefense.CommitHp(actor.hp,damage);actor.hp=hp.hpAfter;actor.alive=actor.hp>0;result.dotSources.Add(e.instanceId);
                }
                e.remainingTicks--;
            }
        }
        static void TickCooldowns(BattleActorSaveRecord actor){foreach(var c in actor.cooldowns??new List<CooldownSaveRecord>())if(c!=null&&c.remainingTicks>0)c.remainingTicks--;actor.cooldowns?.RemoveAll(x=>x==null||x.remainingTicks<=0);}
        static void ExpireEffects(BattleActorSaveRecord actor,BattleTickResult result){var expired=(actor.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x==null||x.consumed||x.remainingTicks<=0).ToList();foreach(var e in expired)if(e!=null)result.expiredEffects.Add(e.instanceId);actor.appliedEffects?.RemoveAll(x=>x==null||x.consumed||x.remainingTicks<=0);}
        static BattleTickResult Fail(string reason)=>new BattleTickResult{ok=false,reason=reason};
    }
}
