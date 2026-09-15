#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Equipment;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class SaveTransactionAndEquipmentTests
    {
        private sealed class State : IDeepCloneable<State>
        {
            public int value;
            public State DeepClone()=>new State{value=value};
        }

        private sealed class Store : ISaveStore<State>
        {
            public State state=new State();
            public bool failWrite;
            public State Load()=>state;
            public void Write(State value)
            {
                if(failWrite) throw new Exception("disk");
                state=value;
            }
        }

        [Test]
        public void SaveTransaction_DoesNotCommitInvalidProposal()
        {
            var store=new Store{state=new State{value=10}};
            var r=SaveTransaction.Execute(store,s=>{s.value=99;return s;},
                s=>s.value>50 ? "too_large" : null);
            Assert.IsFalse(r.ok);
            Assert.AreEqual(10,store.state.value);
        }

        [Test]
        public void SaveTransaction_CommitsOnlyAfterValidation()
        {
            var store=new Store{state=new State{value=10}};
            var r=SaveTransaction.Execute(store,s=>{s.value=20;return s;},s=>null);
            Assert.IsTrue(r.ok);
            Assert.AreEqual(20,store.state.value);
        }

        [Test]
        public void Equipment_RejectsWrongSlotWithoutChangingInput()
        {
            var current=new List<EquippedItem>();
            var def=new EquipmentDefinition{id="HELM",category="head"};
            var r=EquipmentLoadout.Equip(current,"I1",def,EquipmentSlot.WeaponMain,"JOB");
            Assert.IsFalse(r.ok);
            Assert.AreEqual(0,current.Count);
        }

        [Test]
        public void TwoHandedWeapon_ClearsOffhandInProposal()
        {
            var current=new List<EquippedItem>{
                new EquippedItem{instanceId="OFF",definitionId="DAGGER",slot=EquipmentSlot.WeaponSub}
            };
            var def=new EquipmentDefinition{id="GREAT",category="weapon",twoHanded=true};
            var r=EquipmentLoadout.Equip(current,"MAIN",def,EquipmentSlot.WeaponMain,"JOB");
            Assert.IsTrue(r.ok);
            Assert.AreEqual(1,r.next.Count);
            Assert.AreEqual(EquipmentSlot.WeaponMain,r.next[0].slot);
            Assert.AreEqual(1,current.Count); // original proposal source remains untouched
        }
    }
}
#endif
