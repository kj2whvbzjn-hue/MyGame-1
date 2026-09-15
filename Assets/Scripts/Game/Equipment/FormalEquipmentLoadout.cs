using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Equipment
{
    public enum FormalEquipmentSlot { weapon1, weapon2, head, armor, gloves, feet, amulet, ring1, ring2, belt }
    public enum WeaponStyle { single, two_hand, dual_wield, weapon_shield, bow_quiver }

    [Serializable] public sealed class EquipmentRequirements {
        public int STR,VIT,AGI,DEX,INT,MND,LUK;
    }

    [Serializable] public sealed class FormalEquipmentDefinition {
        public string id,slot,baseItemType;
        public EquipmentRequirements required=new EquipmentRequirements();
    }

    [Serializable] public sealed class FormalEquipmentInstance {
        public string instanceId,equipmentId,ownerId;
    }

    [Serializable] public sealed class FormalSlotAssignment {
        public FormalEquipmentSlot slot;
        public string instanceId;
    }

    public enum StrikeHand { MAIN, OFF }
    public sealed class FormalStrike { public StrikeHand hand; public string instanceId,equipmentId; }
    public sealed class RequirementResolution { public string instanceId; public bool usedTwoHandStrRelief; }

    public sealed class FormalLoadoutResult {
        public bool ok; public string reason;
        public List<FormalSlotAssignment> slots=new List<FormalSlotAssignment>();
        public List<FormalStrike> strikes=new List<FormalStrike>();
        public List<RequirementResolution> requirements=new List<RequirementResolution>();
        public List<string> uniqueInstanceIds=new List<string>();
        public bool bowActionReady;
    }

    public static class FormalEquipmentLoadout
    {
        public static readonly FormalEquipmentSlot[] SlotIds=(FormalEquipmentSlot[])Enum.GetValues(typeof(FormalEquipmentSlot));

        public static FormalLoadoutResult Resolve(
            string characterId, IReadOnlyDictionary<string,int> stats, ISet<string> capabilities,
            IReadOnlyDictionary<string,FormalEquipmentInstance> instances,
            IReadOnlyDictionary<FormalEquipmentSlot,string> assignments,
            WeaponStyle style, Func<string,FormalEquipmentDefinition> resolveEquipment,
            int twoHandStrMultiplier=2)
        {
            if(string.IsNullOrWhiteSpace(characterId)||stats==null||instances==null||assignments==null||resolveEquipment==null)
                return Fail("LOADOUT_INPUT_INVALID");
            if(assignments.Count!=SlotIds.Length||SlotIds.Any(s=>!assignments.ContainsKey(s)))
                return Fail("LOADOUT_SLOTS_INCOMPLETE");

            var defs=new Dictionary<string,FormalEquipmentDefinition>();
            foreach(var kv in assignments.Where(x=>!string.IsNullOrWhiteSpace(x.Value)))
            {
                if(!instances.TryGetValue(kv.Value,out var inst))return Fail("EQUIPMENT_INSTANCE_MISSING");
                if(inst.ownerId!=characterId)return Fail("EQUIPMENT_OWNER_MISMATCH");
                var def=resolveEquipment(inst.equipmentId);
                if(def==null)return Fail("EQUIPMENT_DEFINITION_MISSING");
                if(!Accepts(kv.Key,def.slot))return Fail("EQUIPMENT_SLOT_MISMATCH");
                defs[kv.Value]=def;
            }

            var weaponIds=new[]{assignments[FormalEquipmentSlot.weapon1],assignments[FormalEquipmentSlot.weapon2]}
                .Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray();
            var uniqueWeapons=weaponIds.Distinct().ToArray();

            if(style!=WeaponStyle.two_hand && weaponIds.Length!=uniqueWeapons.Length)return Fail("EQUIPMENT_INSTANCE_DUPLICATE");
            var allNonEmpty=assignments.Values.Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray();
            if(style==WeaponStyle.two_hand)
            {
                if(uniqueWeapons.Length!=1||weaponIds.Length!=2||weaponIds[0]!=weaponIds[1])return Fail("TWO_HAND_REQUIRES_SAME_INSTANCE_BOTH_SLOTS");
                if(IsShield(defs[uniqueWeapons[0]])||IsQuiver(defs[uniqueWeapons[0]]))return Fail("TWO_HAND_TYPE_INVALID");
                if(allNonEmpty.GroupBy(x=>x).Any(g=>g.Count()>1&&g.Key!=uniqueWeapons[0]))return Fail("EQUIPMENT_INSTANCE_DUPLICATE");
            }
            else if(allNonEmpty.GroupBy(x=>x).Any(g=>g.Count()>1)) return Fail("EQUIPMENT_INSTANCE_DUPLICATE");

            if(style==WeaponStyle.single && uniqueWeapons.Length!=1)return Fail("SINGLE_REQUIRES_ONE_WEAPON");
            if(style==WeaponStyle.dual_wield)
            {
                if(capabilities==null||!capabilities.Contains("DUAL_WIELD"))return Fail("DUAL_WIELD_CAPABILITY_REQUIRED");
                if(uniqueWeapons.Length!=2||uniqueWeapons.Any(id=>IsShield(defs[id])||IsQuiver(defs[id])))return Fail("DUAL_WIELD_WEAPONS_INVALID");
            }
            if(style==WeaponStyle.weapon_shield && (uniqueWeapons.Length!=2||uniqueWeapons.Count(id=>IsShield(defs[id]))!=1))
                return Fail("WEAPON_SHIELD_INVALID");
            if(style==WeaponStyle.bow_quiver && (uniqueWeapons.Length!=2||uniqueWeapons.Count(id=>IsBow(defs[id]))!=1||uniqueWeapons.Count(id=>IsQuiver(defs[id]))!=1))
                return Fail("BOW_QUIVER_INVALID");

            var result=new FormalLoadoutResult{ok=true,bowActionReady=style==WeaponStyle.bow_quiver};
            result.slots=assignments.Select(x=>new FormalSlotAssignment{slot=x.Key,instanceId=x.Value}).ToList();
            result.uniqueInstanceIds=allNonEmpty.Distinct().ToList();

            foreach(var id in result.uniqueInstanceIds)
            {
                bool relief=style==WeaponStyle.two_hand&&uniqueWeapons.Contains(id);
                if(!CheckRequirements(stats,defs[id],relief,twoHandStrMultiplier,out var used))return Fail("EQUIPMENT_REQUIREMENT_FAILED");
                result.requirements.Add(new RequirementResolution{instanceId=id,usedTwoHandStrRelief=used});
            }

            string w1=assignments[FormalEquipmentSlot.weapon1],w2=assignments[FormalEquipmentSlot.weapon2];
            if(style==WeaponStyle.dual_wield){AddStrike(result,StrikeHand.MAIN,w1,instances);AddStrike(result,StrikeHand.OFF,w2,instances);}
            else if(style==WeaponStyle.weapon_shield){var id=uniqueWeapons.First(x=>!IsShield(defs[x]));AddStrike(result,StrikeHand.MAIN,id,instances);}
            else if(style==WeaponStyle.bow_quiver){var id=uniqueWeapons.First(x=>IsBow(defs[x]));AddStrike(result,StrikeHand.MAIN,id,instances);}
            else {var id=uniqueWeapons.FirstOrDefault();if(id!=null&&!IsShield(defs[id])&&!IsQuiver(defs[id]))AddStrike(result,StrikeHand.MAIN,id,instances);}
            return result;
        }

        public static bool Accepts(FormalEquipmentSlot slot,string expected)
        {
            if(expected=="weapon")return slot==FormalEquipmentSlot.weapon1||slot==FormalEquipmentSlot.weapon2;
            if(expected=="ring")return slot==FormalEquipmentSlot.ring1||slot==FormalEquipmentSlot.ring2;
            return string.Equals(slot.ToString(),expected,StringComparison.Ordinal);
        }

        static bool CheckRequirements(IReadOnlyDictionary<string,int> stats,FormalEquipmentDefinition d,bool relief,int mult,out bool used)
        {
            used=false; var r=d.required??new EquipmentRequirements();
            foreach(var p in new[]{("STR",r.STR),("VIT",r.VIT),("AGI",r.AGI),("DEX",r.DEX),("INT",r.INT),("MND",r.MND),("LUK",r.LUK)})
            {
                int actual=stats.TryGetValue(p.Item1,out var v)?v:0;
                if(actual>=p.Item2)continue;
                if(p.Item1=="STR"&&relief&&actual*Math.Max(1,mult)>=p.Item2){used=true;continue;}
                return false;
            }
            return true;
        }
        static bool IsShield(FormalEquipmentDefinition d)=>d.baseItemType=="盾";
        static bool IsBow(FormalEquipmentDefinition d)=>d.baseItemType=="弓"||d.baseItemType=="大弓";
        static bool IsQuiver(FormalEquipmentDefinition d)=>d.baseItemType=="矢筒";
        static void AddStrike(FormalLoadoutResult r,StrikeHand hand,string id,IReadOnlyDictionary<string,FormalEquipmentInstance> instances)
        { if(string.IsNullOrWhiteSpace(id))return; var i=instances[id];r.strikes.Add(new FormalStrike{hand=hand,instanceId=id,equipmentId=i.equipmentId}); }
        static FormalLoadoutResult Fail(string reason)=>new FormalLoadoutResult{ok=false,reason=reason};
    }
}
