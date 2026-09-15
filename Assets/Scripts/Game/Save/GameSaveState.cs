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
                activeTimeline=CloneTimeline(activeTimeline)
            };
        }

        public static string Validate(GameSaveState s)
        {
            if(s==null)return "SAVE_NULL";
            if(string.IsNullOrWhiteSpace(s.schemaVersion))return "SAVE_SCHEMA_MISSING";
            if(s.guild==null||s.inventory==null)return "SAVE_DOMAIN_MISSING";
            var runtimeError=RuntimeDomainValidation.Validate(s.runtimeDomain);
            if(!string.IsNullOrEmpty(runtimeError))return runtimeError;
            if(s.inventory.capacity<0||s.inventory.items.Count>s.inventory.capacity)return "SAVE_INVENTORY_INVALID";
            var ids=new HashSet<string>();
            foreach(var x in s.inventory.items)
                if(x==null||string.IsNullOrWhiteSpace(x.instanceId)||!ids.Add(x.instanceId))return "SAVE_INVENTORY_DUPLICATE";
            return null;
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
