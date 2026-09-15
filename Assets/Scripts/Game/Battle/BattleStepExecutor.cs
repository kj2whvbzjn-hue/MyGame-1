using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleAttackProposal
    {
        public string reservationId,hitId,sourceId,targetId,skillId;
        public DamageType damageType;
        public double criticalRatePercent,accuracy,evasion,magicAccuracy,magicResistance;
        public double baseDamage,damageResistance,criticalBonusDamagePercent=HitCritical.InitialCriticalBonusDamagePercent;
        public double criticalBonusReduction,formationMultiplier=1,randomMultiplier=1;
        public IEnumerable<ElementShare> elementShares;
        public IDictionary<Element,double> elementResistances;
        public bool blockEligible;
        public double blockRate,blockCutRate;
        public List<BarrierLayer> barriers=new List<BarrierLayer>();
        public Func<int,int,int?> fatalResolver;
        public double hpAbsorbRate,mpAbsorbRate,reflectionRate;
    }

    public sealed class BattleStepResult
    {
        public bool ok; public string reason;
        public ActionReservationSaveRecord reservation;
        public ResolvedHitSaveRecord resolvedHit;
        public List<BarrierLayer> remainingBarriers=new List<BarrierLayer>();
        public int barrierAbsorbed,hpAbsorbed,mpAbsorbed,reflectedDamage;
    }

    public static class BattleStepExecutor
    {
        public static BattleStepResult ExecuteAttack(
            BattleSnapshotSaveRecord snapshot,BattleAttackProposal p,
            IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            if(snapshot==null||p==null||criticalRng==null||hitRng==null)return Fail("BATTLE_STEP_INPUT_INVALID");
            var source=snapshot.actors.Find(x=>x.actorId==p.sourceId);
            var target=snapshot.actors.Find(x=>x.actorId==p.targetId);
            if(source==null||target==null)return Fail("BATTLE_STEP_ACTOR_MISSING");
            if(!source.alive||source.hp<=0)return Fail("BATTLE_STEP_SOURCE_DEAD");
            if(!target.alive||target.hp<=0)return Fail("BATTLE_STEP_TARGET_DEAD");
            if(string.IsNullOrWhiteSpace(p.reservationId)||string.IsNullOrWhiteSpace(p.hitId))return Fail("BATTLE_STEP_ID_MISSING");
            if(snapshot.actionReservations.Exists(x=>x.reservationId==p.reservationId)||snapshot.resolvedHits.Exists(x=>x.hitId==p.hitId))
                return Fail("BATTLE_STEP_ID_DUPLICATE");

            var reservation=new ActionReservationSaveRecord{
                reservationId=p.reservationId,actorId=p.sourceId,skillId=p.skillId,
                startTick=snapshot.tick,completeTick=snapshot.tick,
                fixedTargetIds=new List<string>{p.targetId},
                usageConditions=new UsageConditionsSaveRecord()
            };

            var hit=HitCritical.Resolve(p.criticalRatePercent,p.damageType,p.accuracy,p.evasion,p.magicAccuracy,p.magicResistance,
                criticalRng.NextDouble,hitRng.NextDouble);
            var resolved=new ResolvedHitSaveRecord{
                actionId=p.reservationId,hitIndex=snapshot.resolvedHits.FindAll(x=>x.actionId==p.reservationId).Count,
                sourceId=p.sourceId,targetId=p.targetId,
                judgement=hit.critical?"CRITICAL":hit.hit?"HIT":"MISS",
                perHitDamage=0,committedHp=target.hp,actualHpLoss=0
            };
            resolved.rngRolls.Add(hit.criticalRoll/100d);
            if(hit.hitRoll.HasValue)resolved.rngRolls.Add(hit.hitRoll.Value/100d);

            if(!hit.hit)
            {
                snapshot.actionReservations.Add(reservation); snapshot.resolvedHits.Add(resolved);
                return new BattleStepResult{ok=true,reservation=reservation,resolvedHit=resolved};
            }

            var damage=DamageDefense.ResolveFinalDamage(p.baseDamage,p.damageResistance,hit.critical,p.criticalBonusDamagePercent,
                p.criticalBonusReduction,p.elementShares,p.elementResistances,p.formationMultiplier,p.randomMultiplier);
            resolved.perHitDamage=damage.finalDamage;

            double? blockRoll=null;
            if(p.blockEligible&&p.blockRate>0)
            {
                if(blockRng==null)return Fail("BATTLE_STEP_BLOCK_RNG_MISSING");
                blockRoll=blockRng.NextDouble(); resolved.rngRolls.Add(blockRoll.Value);
            }
            var block=DamageDefense.ResolveBlock(damage.finalDamage,p.blockEligible,p.blockRate,p.blockCutRate,blockRoll);
            resolved.block=new OpaqueContractPayload{json=block.blocked?"{\"blocked\":true}":"{\"blocked\":false}"};

            var barrier=DamageDefense.ConsumeBarrierFifo(block.damage,p.barriers);
            var hp=DamageDefense.CommitHp(target.hp,barrier.hpDamageCandidate,p.fatalResolver);
            target.hp=hp.hpAfter; target.alive=target.hp>0;
            resolved.committedHp=hp.hpAfter; resolved.actualHpLoss=hp.actualHpLoss;
            resolved.barrier=new OpaqueContractPayload{json="{\"absorbed\":"+barrier.absorbed+"}"};

            // Studio contract: referenced effects are based on actual HP loss.
            // Absorb uses ceil; reflection uses floor; no recursive resolution is performed here.
            int hpAbsorb=(int)Math.Ceiling(hp.actualHpLoss*Math.Max(0,p.hpAbsorbRate));
            int mpAbsorb=(int)Math.Ceiling(hp.actualHpLoss*Math.Max(0,p.mpAbsorbRate));
            int reflected=(int)Math.Floor(hp.actualHpLoss*Math.Max(0,p.reflectionRate));
            source.hp=Math.Min(source.maxHp,source.hp+hpAbsorb);
            source.mp=Math.Min(source.maxMp,source.mp+mpAbsorb);
            if(reflected>0)
            {
                var reflectedCommit=DamageDefense.CommitHp(source.hp,reflected);
                source.hp=reflectedCommit.hpAfter; source.alive=source.hp>0;
            }

            snapshot.actionReservations.Add(reservation); snapshot.resolvedHits.Add(resolved);
            return new BattleStepResult{ok=true,reservation=reservation,resolvedHit=resolved,
                remainingBarriers=barrier.layers,barrierAbsorbed=barrier.absorbed,
                hpAbsorbed=hpAbsorb,mpAbsorbed=mpAbsorb,reflectedDamage=reflected};
        }

        static BattleStepResult Fail(string reason)=>new BattleStepResult{ok=false,reason=reason};
    }
}
