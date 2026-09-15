#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Equipment;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FormalEquipmentExactContractTests
    {
        static Dictionary<FormalEquipmentSlot,string> EmptySlots()
        {
            var d=new Dictionary<FormalEquipmentSlot,string>();
            foreach(var s in FormalEquipmentLoadout.SlotIds)d[s]=null;
            return d;
        }
        static Dictionary<string,int> Stats(int str=100)=>new Dictionary<string,int>{
            {"STR",str},{"VIT",100},{"AGI",100},{"DEX",100},{"INT",100},{"MND",100},{"LUK",100}};

        [Test] public void TwoHand_UsesSameInstanceInBothWeaponSlots_AndAllowsStrRelief()
        {
            var slots=EmptySlots();slots[FormalEquipmentSlot.weapon1]="I";slots[FormalEquipmentSlot.weapon2]="I";
            var inst=new Dictionary<string,FormalEquipmentInstance>{{"I",new FormalEquipmentInstance{instanceId="I",equipmentId="E",ownerId="C"}}};
            var def=new FormalEquipmentDefinition{id="E",slot="weapon",baseItemType="大剣",required=new EquipmentRequirements{STR=20}};
            var r=FormalEquipmentLoadout.Resolve("C",Stats(10),new HashSet<string>(),inst,slots,WeaponStyle.two_hand,_=>def,2);
            Assert.IsTrue(r.ok);Assert.AreEqual(1,r.uniqueInstanceIds.Count);Assert.IsTrue(r.requirements[0].usedTwoHandStrRelief);
        }

        [Test] public void DualWield_RequiresCapability()
        {
            var slots=EmptySlots();slots[FormalEquipmentSlot.weapon1]="A";slots[FormalEquipmentSlot.weapon2]="B";
            var inst=new Dictionary<string,FormalEquipmentInstance>{
                {"A",new FormalEquipmentInstance{equipmentId="EA",ownerId="C"}},
                {"B",new FormalEquipmentInstance{equipmentId="EB",ownerId="C"}}};
            var defs=new Dictionary<string,FormalEquipmentDefinition>{
                {"EA",new FormalEquipmentDefinition{id="EA",slot="weapon",baseItemType="片手剣"}},
                {"EB",new FormalEquipmentDefinition{id="EB",slot="weapon",baseItemType="短剣"}}};
            var r=FormalEquipmentLoadout.Resolve("C",Stats(),new HashSet<string>(),inst,slots,WeaponStyle.dual_wield,id=>defs[id]);
            Assert.IsFalse(r.ok);Assert.AreEqual("DUAL_WIELD_CAPABILITY_REQUIRED",r.reason);
        }

        [Test] public void BowQuiver_ProducesBowMainStrike()
        {
            var slots=EmptySlots();slots[FormalEquipmentSlot.weapon1]="B";slots[FormalEquipmentSlot.weapon2]="Q";
            var inst=new Dictionary<string,FormalEquipmentInstance>{
                {"B",new FormalEquipmentInstance{instanceId="B",equipmentId="EB",ownerId="C"}},
                {"Q",new FormalEquipmentInstance{instanceId="Q",equipmentId="EQ",ownerId="C"}}};
            var defs=new Dictionary<string,FormalEquipmentDefinition>{
                {"EB",new FormalEquipmentDefinition{id="EB",slot="weapon",baseItemType="弓"}},
                {"EQ",new FormalEquipmentDefinition{id="EQ",slot="weapon",baseItemType="矢筒"}}};
            var r=FormalEquipmentLoadout.Resolve("C",Stats(),new HashSet<string>(),inst,slots,WeaponStyle.bow_quiver,id=>defs[id]);
            Assert.IsTrue(r.ok);Assert.IsTrue(r.bowActionReady);Assert.AreEqual("EB",r.strikes[0].equipmentId);
        }
    }
}
#endif
