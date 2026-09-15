using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Game.Battle
{
    public sealed class BattleAttackProposal
    {
        public string reservationId,sourceId,targetId,skillId;
        public int hitCount=1;
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
        public IEnumerable<TriggerRegistration> triggerRegistrations;
    }

    public sealed class BattleStepResult
    {
        public bool ok; public string reason;
        public ActionReservationSaveRecord reservation;
        public ResolvedHitSaveRecord resolvedHit; // compatibility: last hit
        public List<ResolvedHitSaveRecord> resolvedHits=new List<ResolvedHitSaveRecord>();
        public List<BarrierLayer> remainingBarriers=new List<BarrierLayer>();
        public int barrierAbsorbed,hpAbsorbed,mpAbsorbed,reflectedDamage;
        public List<BattleTriggerDispatch> triggerDispatches=new List<BattleTriggerDispatch>();
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
            if(string.IsNullOrWhiteSpace(p.reservationId))return Fail("BATTLE_STEP_ID_MISSING");
            if(p.hitCount<=0)return Fail("BATTLE_STEP_HIT_COUNT_INVALID");
            if(snapshot.actionReservations.Exists(x=>x.reservationId==p.reservationId))
                return Fail("BATTLE_STEP_ID_DUPLICATE");

            var reservation=new ActionReservationSaveRecord{
                reservationId=p.reservationId,actorId=p.sourceId,skillId=p.skillId,
                startTick=snapshot.tick,completeTick=snapshot.tick,
                fixedTargetIds=new List<string>{p.targetId},
                usageConditions=new UsageConditionsSaveRecord()
            };
            snapshot.actionReservations.Add(reservation);

            var result=new BattleStepResult{ok=true,reservation=reservation};
            var barriers=p.barriers==null?new List<BarrierLayer>():new List<BarrierLayer>(p.barriers);

            for(var i=0;i<p.hitCount;i++)
            {
                // Every hit is its own formal C03 record. A target killed by an earlier hit receives no later hits.
                if(!target.alive||target.hp<=0)break;

                var one=ResolveHit(snapshot,p,source,target,i,barriers,criticalRng,hitRng,blockRng);
                if(!one.ok)
                {
                    snapshot.actionReservations.Remove(reservation);
                    foreach(var h in result.resolvedHits)snapshot.resolvedHits.Remove(h);
                    return Fail(one.reason);
                }

                result.resolvedHit=one.resolvedHit;
                result.resolvedHits.Add(one.resolvedHit);
                result.remainingBarriers=one.remainingBarriers;
                result.barrierAbsorbed+=one.barrierAbsorbed;
                result.hpAbsorbed+=one.hpAbsorbed;
                result.mpAbsorbed+=one.mpAbsorbed;
                result.reflectedDamage+=one.reflectedDamage;
                barriers=one.remainingBarriers;
                snapshot.resolvedHits.Add(one.resolvedHit);
                result.triggerDispatches.Add(BattleEffectLifecycle.DispatchResolvedHit(
                    one.resolvedHit,p.triggerRegistrations,BattleEffectLifecycle.BuildFixedOrder(snapshot)));

                if(!source.alive||source.hp<=0)break; // reflection can end the action.
            }
            return result;
        }

        static BattleStepResult ResolveHit(
            BattleSnapshotSaveRecord snapshot,BattleAttackProposal p,
            BattleActorSaveRecord source,BattleActorSaveRecord target,int hitIndex,List<BarrierLayer> barriers,
            IRandomSource criticalRng,IRandomSource hitRng,IRandomSource blockRng)
        {
            var hit=HitCritical.Resolve(p.criticalRatePercent,p.damageType,p.accuracy,p.evasion,p.magicAccuracy,p.magicResistance,
                ()=>criticalRng.Next01("CRITICAL"),()=>hitRng.Next01("HIT"));
            var resolved=new ResolvedHitSaveRecord{
                actionId=p.reservationId,hitIndex=hitIndex,
                sourceId=p.sourceId,targetId=p.targetId,
                judgement=hit.critical?"CRITICAL":hit.hit?"HIT":"MISS",
                perHitDamage=0,committedHp=target.hp,actualHpLoss=0
            };
            resolved.rngRolls.Add(hit.criticalRoll/100d);
            if(hit.hitRoll.HasValue)resolved.rngRolls.Add(hit.hitRoll.Value/100d);

            if(!hit.hit)
                return new BattleStepResult{ok=true,resolvedHit=resolved,remainingBarriers=barriers};

            var damage=DamageDefense.ResolveFinalDamage(p.baseDamage,p.damageResistance,hit.critical,p.criticalBonusDamagePercent,
                p.criticalBonusReduction,p.elementShares,p.elementResistances,p.formationMultiplier,p.randomMultiplier);
            resolved.perHitDamage=damage.finalDamage;

            double? blockRoll=null;
            if(p.blockEligible&&p.blockRate>0)
            {
                if(blockRng==null)return Fail("BATTLE_STEP_BLOCK_RNG_MISSING");
                blockRoll=blockRng.Next01("BLOCK"); resolved.rngRolls.Add(blockRoll.Value);
            }
            var block=DamageDefense.ResolveBlock(damage.finalDamage,p.blockEligible,p.blockRate,p.blockCutRate,blockRoll);
            resolved.block=new OpaqueContractPayload{json=block.blocked?"{\"blocked\":true}":"{\"blocked\":false}"};

            var barrier=DamageDefense.ConsumeBarrierFifo(block.damage,barriers);
            var hp=DamageDefense.CommitHp(target.hp,barrier.hpDamageCandidate,p.fatalResolver);
            target.hp=hp.hpAfter; target.alive=target.hp>0;
            resolved.committedHp=hp.hpAfter; resolved.actualHpLoss=hp.actualHpLoss;
            resolved.barrier=new OpaqueContractPayload{json="{\"absorbed\":"+barrier.absorbed+"}"};

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

            return new BattleStepResult{ok=true,resolvedHit=resolved,
                remainingBarriers=barrier.layers,barrierAbsorbed=barrier.absorbed,
                hpAbsorbed=hpAbsorb,mpAbsorbed=mpAbsorb,reflectedDamage=reflected};
        }

        static BattleStepResult Fail(string reason)=>new BattleStepResult{ok=false,reason=reason};
    }
}
