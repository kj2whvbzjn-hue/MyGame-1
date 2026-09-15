using System;
using System.Collections.Generic;
using GuildAdventure.Game.Guild;
using GuildAdventure.Game.Inventory;
using GuildAdventure.Game.Adventure;

namespace GuildAdventure.Game.Save
{
    [Serializable]
    public sealed class GameSaveState : IDeepCloneable<GameSaveState>
    {
        public string schemaVersion="1";
        public string saveId;
        public GuildState guild=new GuildState();
        public InventoryState inventory=new InventoryState();
        public AdventureRunSnapshot activeRun;
        public RuntimeDomainState runtimeDomain=new RuntimeDomainState();
        public AdventureTimelineSaveRecord activeTimeline;
        public BattleSnapshotSaveRecord activeBattle;

        public GameSaveState DeepClone()
        {
            return new GameSaveState{
                schemaVersion=schemaVersion,
                saveId=saveId,
                guild=guild==null?null:new GuildState{
                    rank=guild.rank,points=guild.points,
                    memberCapacity=guild.memberCapacity,warehouseCapacity=guild.warehouseCapacity},
                inventory=inventory==null?null:new InventoryState{
                    capacity=inventory.capacity,
                    items=inventory.items==null?new List<InventoryItem>():inventory.items.ConvertAll(x=>new InventoryItem{
                        instanceId=x.instanceId,masterId=x.masterId,kind=x.kind,amount=x.amount})},
                activeRun=CloneRun(activeRun),
                runtimeDomain=CloneRuntime(runtimeDomain),
                activeTimeline=CloneTimeline(activeTimeline),
                activeBattle=CloneBattle(activeBattle)
            };
        }

        public static string Validate(GameSaveState s)
        {
            if(s==null)return "SAVE_NULL";
            if(string.IsNullOrWhiteSpace(s.schemaVersion))return "SAVE_SCHEMA_MISSING";
            if(s.guild==null||s.inventory==null)return "SAVE_DOMAIN_MISSING";
            var battleError=BattleSnapshotValidation.Validate(s.activeBattle);
            if(!string.IsNullOrEmpty(battleError))return battleError;
            var runtimeError=RuntimeDomainValidation.Validate(s.runtimeDomain);
            if(!string.IsNullOrEmpty(runtimeError))return runtimeError;
            if(s.inventory.capacity<0||s.inventory.items.Count>s.inventory.capacity)return "SAVE_INVENTORY_INVALID";
            var ids=new HashSet<string>();
            foreach(var x in s.inventory.items)
                if(x==null||string.IsNullOrWhiteSpace(x.instanceId)||!ids.Add(x.instanceId))return "SAVE_INVENTORY_DUPLICATE";
            return null;
        }




        private static BattleSnapshotSaveRecord CloneBattle(BattleSnapshotSaveRecord b)
        {
            if(b==null)return null;
            var r=new BattleSnapshotSaveRecord{
                contract=b.contract,schemaVersion=b.schemaVersion,battleId=b.battleId,settingsVersion=b.settingsVersion,seed=b.seed,tick=b.tick,
                formation=b.formation==null?new List<string>():new List<string>(b.formation),
                fixedActorOrder=b.fixedActorOrder==null?new List<string>():new List<string>(b.fixedActorOrder)};
            foreach(var a in b.actors??new List<BattleActorSaveRecord>())
            {
                var x=new BattleActorSaveRecord{actorId=a.actorId,aiProgramId=a.aiProgramId,hp=a.hp,maxHp=a.maxHp,mp=a.mp,maxMp=a.maxMp,actionGauge=a.actionGauge,speed=a.speed,alive=a.alive,
                    cast=a.cast==null?null:new CastSaveRecord{skillId=a.cast.skillId,targetId=a.cast.targetId,remainingTicks=a.cast.remainingTicks,active=a.cast.active}};
                foreach(var c in a.cooldowns??new List<CooldownSaveRecord>())x.cooldowns.Add(new CooldownSaveRecord{skillId=c.skillId,remainingTicks=c.remainingTicks});
                foreach(var e in a.appliedEffects??new List<AppliedEffectSaveRecord>())x.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId=e.instanceId,sourceId=e.sourceId,effectId=e.effectId,kind=e.kind,remainingTicks=e.remainingTicks,value=e.value,consumed=e.consumed});
                r.actors.Add(x);
            }
            foreach(var s in b.rngStreams??new List<RngStreamSaveRecord>())r.rngStreams.Add(new RngStreamSaveRecord{purpose=s.purpose,cursor=s.cursor,recordedRolls=s.recordedRolls==null?new List<double>():new List<double>(s.recordedRolls)});
            foreach(var x in b.actionReservations??new List<ActionReservationSaveRecord>())r.actionReservations.Add(new ActionReservationSaveRecord{
                contract=x.contract,schemaVersion=x.schemaVersion,reservationId=x.reservationId,actorId=x.actorId,skillId=x.skillId,startTick=x.startTick,completeTick=x.completeTick,
                fixedTargetIds=x.fixedTargetIds==null?new List<string>():new List<string>(x.fixedTargetIds),usageConditions=new UsageConditionsSaveRecord{json=x.usageConditions?.json??"{}"}});
            foreach(var x in b.resolvedHits??new List<ResolvedHitSaveRecord>())r.resolvedHits.Add(new ResolvedHitSaveRecord{
                contract=x.contract,schemaVersion=x.schemaVersion,actionId=x.actionId,hitIndex=x.hitIndex,sourceId=x.sourceId,targetId=x.targetId,judgement=x.judgement,
                perHitDamage=x.perHitDamage,committedHp=x.committedHp,actualHpLoss=x.actualHpLoss,
                block=new OpaqueContractPayload{json=x.block?.json??"{}"},barrier=new OpaqueContractPayload{json=x.barrier?.json??"{}"},triggerContext=new OpaqueContractPayload{json=x.triggerContext?.json??"{}"},
                rngRolls=x.rngRolls==null?new List<double>():new List<double>(x.rngRolls)});
            return r;
        }

        private static List<GrowthHistorySaveRecord> CloneGrowth(List<GrowthHistorySaveRecord> rows)
        {
            var r=new List<GrowthHistorySaveRecord>();
            foreach(var x in rows??new List<GrowthHistorySaveRecord>())
                r.Add(new GrowthHistorySaveRecord{level=x.level,jobId=x.jobId,strGain=x.strGain,vitGain=x.vitGain,agiGain=x.agiGain,dexGain=x.dexGain,intGain=x.intGain,mndGain=x.mndGain,lukGain=x.lukGain,rngRolls=x.rngRolls==null?new List<double>():new List<double>(x.rngRolls)});
            return r;
        }

        private static AdventureTimelineSaveRecord CloneTimeline(AdventureTimelineSaveRecord x)
        {
            if(x==null)return null;
            var r=new AdventureTimelineSaveRecord{runId=x.runId,questId=x.questId,cursor=x.cursor};
            foreach(var s in x.steps??new List<AdventureTimelineStepSaveRecord>())
                r.steps.Add(new AdventureTimelineStepSaveRecord{kind=s.kind,refId=s.refId,completed=s.completed});
            return r;
        }

        private static RuntimeDomainState CloneRuntime(RuntimeDomainState s)
        {
            if(s==null)return null;
            var r=new RuntimeDomainState();
            foreach(var c in s.characters??new List<CharacterSaveRecord>())
                r.characters.Add(new CharacterSaveRecord{
                    characterId=c.characterId,name=c.name,currentJobId=c.currentJobId,level=c.level,skillPoints=c.skillPoints,alive=c.alive,
                    learnedSkillIds=c.learnedSkillIds==null?new List<string>():new List<string>(c.learnedSkillIds),
                    passiveIds=c.passiveIds==null?new List<string>():new List<string>(c.passiveIds),
                    aiProgramId=c.aiProgramId,
                    stats=c.stats==null?null:new CharacterStatsSaveRecord{STR=c.stats.STR,VIT=c.stats.VIT,AGI=c.stats.AGI,DEX=c.stats.DEX,INT=c.stats.INT,MND=c.stats.MND,LUK=c.stats.LUK},
                    growthHistory=CloneGrowth(c.growthHistory),
                    equipment=c.equipment==null?null:new EquipmentLoadoutSaveRecord{
                        weaponStyle=c.equipment.weaponStyle,weapon1=c.equipment.weapon1,weapon2=c.equipment.weapon2,
                        head=c.equipment.head,armor=c.equipment.armor,gloves=c.equipment.gloves,feet=c.equipment.feet,
                        amulet=c.equipment.amulet,ring1=c.equipment.ring1,ring2=c.equipment.ring2,belt=c.equipment.belt}});
            foreach(var i in s.equipmentInstances??new List<EquipmentInstanceSaveRecord>())
                r.equipmentInstances.Add(new EquipmentInstanceSaveRecord{instanceId=i.instanceId,equipmentId=i.equipmentId,ownerId=i.ownerId});
            return r;
        }

        private static AdventureRunSnapshot CloneRun(AdventureRunSnapshot r)
        {
            if(r==null)return null;
            return new AdventureRunSnapshot{
                runId=r.runId,questId=r.questId,state=r.state,elapsedSeconds=r.elapsedSeconds,
                temporaryExperience=r.temporaryExperience,
                partyCharacterIds=r.partyCharacterIds==null?new List<string>():new List<string>(r.partyCharacterIds),
                defeatedMonsterIds=r.defeatedMonsterIds==null?new List<string>():new List<string>(r.defeatedMonsterIds)
            };
        }
    }
}
