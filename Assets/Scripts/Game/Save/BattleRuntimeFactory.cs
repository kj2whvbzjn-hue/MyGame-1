using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Game.Save
{
    public sealed class RestoredBattleRuntime
    {
        public string battleId;
        public int tick;
        public List<string> fixedActorOrder=new List<string>();
        public Dictionary<string,RestoredBattleActor> actors=new Dictionary<string,RestoredBattleActor>();
        public List<ActionReservationSaveRecord> reservations=new List<ActionReservationSaveRecord>();
        public List<ResolvedHitSaveRecord> resolvedHits=new List<ResolvedHitSaveRecord>();
    }

    public static class BattleRuntimeFactory
    {
        public static RestoredBattleRuntime Restore(BattleSnapshotSaveRecord snapshot)
        {
            var error=BattleSnapshotValidation.Validate(snapshot);
            if(error!=null)throw new InvalidOperationException(error);
            var r=new RestoredBattleRuntime{
                battleId=snapshot.battleId,tick=snapshot.tick,
                fixedActorOrder=new List<string>(snapshot.fixedActorOrder),
                reservations=CloneReservations(snapshot.actionReservations),
                resolvedHits=CloneHits(snapshot.resolvedHits)
            };
            foreach(var a in snapshot.actors)r.actors.Add(a.actorId,BattleResumeAdapter.RestoreActor(a));
            return r;
        }

        static List<ActionReservationSaveRecord> CloneReservations(List<ActionReservationSaveRecord> xs)
            =>(xs??new List<ActionReservationSaveRecord>()).Select(x=>new ActionReservationSaveRecord{
                contract=x.contract,schemaVersion=x.schemaVersion,reservationId=x.reservationId,actorId=x.actorId,skillId=x.skillId,
                startTick=x.startTick,completeTick=x.completeTick,
                fixedTargetIds=x.fixedTargetIds==null?new List<string>():new List<string>(x.fixedTargetIds),
                usageConditions=new UsageConditionsSaveRecord{json=x.usageConditions?.json??"{}"}}).ToList();

        static List<ResolvedHitSaveRecord> CloneHits(List<ResolvedHitSaveRecord> xs)
            =>(xs??new List<ResolvedHitSaveRecord>()).Select(x=>new ResolvedHitSaveRecord{
                contract=x.contract,schemaVersion=x.schemaVersion,actionId=x.actionId,hitIndex=x.hitIndex,
                sourceId=x.sourceId,targetId=x.targetId,judgement=x.judgement,perHitDamage=x.perHitDamage,
                committedHp=x.committedHp,actualHpLoss=x.actualHpLoss,
                block=new OpaqueContractPayload{json=x.block?.json??"{}"},
                barrier=new OpaqueContractPayload{json=x.barrier?.json??"{}"},
                triggerContext=new OpaqueContractPayload{json=x.triggerContext?.json??"{}"},
                rngRolls=x.rngRolls==null?new List<double>():new List<double>(x.rngRolls)}).ToList();
    }
}
