using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Character
{
    [Serializable]
    public sealed class RecruitDefaults
    {
        public int level=1;
        public int initialSkillPoints=0;
        public string formationPosition;
        public Dictionary<CharacterStat,int> stats;
    }

    [Serializable]
    public sealed class EquipmentInstance
    {
        public string instanceId;
        public string equipmentId;
        public string ownerId;
        public string slot;

        public EquipmentInstance Clone() => (EquipmentInstance)MemberwiseClone();
    }

    [Serializable]
    public sealed class RecruitCharacter
    {
        public string id,name,adventurerType,jobId,formationPosition,createdAt;
        public int level,skillPoints;
        public Dictionary<CharacterStat,int> stats;
        public List<string> skills=new List<string>(), passives=new List<string>();
    }

    public sealed class RecruitResult
    {
        public bool ok; public string reason; public RecruitCharacter character;
    }

    public sealed class DismissResult
    {
        public bool ok; public string reason;
        public List<RecruitCharacter> nextCharacters;
        public List<EquipmentInstance> nextInventory;
        public List<string> returnedEquipmentInstanceIds=new List<string>();
    }

    public static class RecruitDismiss
    {
        public static RecruitResult Hire(
            IReadOnlyCollection<RecruitCharacter> characters, int memberCapacity,
            ISet<string> unlockedTypes, ISet<string> unlockedJobs,
            string typeId,string jobId,string name,string now,
            RecruitDefaults defaults, JobCatalog jobs, Func<string> issueCharacterId)
        {
            if(characters==null||unlockedTypes==null||unlockedJobs==null||defaults==null||jobs==null||issueCharacterId==null)
                throw new ArgumentNullException();
            if(characters.Count>=memberCapacity) return FailHire("guild_member_capacity_reached");
            if(!unlockedTypes.Contains(typeId)) return FailHire("adventurer_type_locked");
            if(!unlockedJobs.Contains(jobId)) return FailHire("recruit_job_locked");
            if(jobs.Resolve(jobId)==null) return FailHire("recruit_job_unknown");
            if(string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(now)) return FailHire("input_invalid");

            string id=issueCharacterId();
            if(string.IsNullOrWhiteSpace(id)||characters.Any(x=>x.id==id)) return FailHire("duplicate_character_id");
            var stats=defaults.stats ?? Seven(10);
            return new RecruitResult {ok=true,character=new RecruitCharacter{
                id=id,name=name,adventurerType=typeId,jobId=jobId,level=defaults.level,
                skillPoints=defaults.initialSkillPoints,formationPosition=defaults.formationPosition,
                createdAt=now,stats=new Dictionary<CharacterStat,int>(stats)
            }};
        }

        public static DismissResult Dismiss(
            IReadOnlyCollection<RecruitCharacter> characters, string characterId,
            ISet<string> partyIds, IReadOnlyCollection<EquipmentInstance> inventory,
            IReadOnlyCollection<EquipmentInstance> equippedByTarget, int inventoryCapacity)
        {
            if(characters==null||partyIds==null||inventory==null||equippedByTarget==null) throw new ArgumentNullException();
            var target=characters.FirstOrDefault(x=>x.id==characterId);
            if(target==null) return FailDismiss("dismiss_character_not_found");
            if(partyIds.Contains(characterId)) return FailDismiss("dismiss_party_member");
            if(inventory.Count+equippedByTarget.Count>inventoryCapacity) return FailDismiss("dismiss_inventory_full");

            var ids=new HashSet<string>();
            foreach(var x in inventory) if(x==null||string.IsNullOrWhiteSpace(x.instanceId)||!ids.Add(x.instanceId)) return FailDismiss("duplicate_equipment_instance");
            foreach(var x in equippedByTarget)
            {
                if(x==null||x.ownerId!=characterId) return FailDismiss("equipment_owner_mismatch");
                if(string.IsNullOrWhiteSpace(x.instanceId)||!ids.Add(x.instanceId)) return FailDismiss("duplicate_equipment_instance");
            }

            var nextChars=characters.Where(x=>x.id!=characterId).ToList();
            var nextInv=inventory.Select(x=>x.Clone()).ToList();
            var returned=new List<string>();
            foreach(var x in equippedByTarget)
            {
                var copy=x.Clone(); copy.ownerId=null; copy.slot=null;
                nextInv.Add(copy); returned.Add(copy.instanceId);
            }
            return new DismissResult{ok=true,nextCharacters=nextChars,nextInventory=nextInv,returnedEquipmentInstanceIds=returned};
        }

        private static RecruitResult FailHire(string r)=>new RecruitResult{ok=false,reason=r};
        private static DismissResult FailDismiss(string r)=>new DismissResult{ok=false,reason=r};
        private static Dictionary<CharacterStat,int> Seven(int v) {
            var d=new Dictionary<CharacterStat,int>(); foreach(CharacterStat s in Enum.GetValues(typeof(CharacterStat))) d[s]=v; return d;
        }
    }
}
