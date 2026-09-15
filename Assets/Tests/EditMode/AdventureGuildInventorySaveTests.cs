#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Adventure;
using GuildAdventure.Game.Guild;
using GuildAdventure.Game.Inventory;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class AdventureGuildInventorySaveTests
    {
        [Test] public void Orchestrator_AdvancesInFixedTimelineOrder()
        {
            var t=new AdventureTimeline{steps=new System.Collections.Generic.List<AdventureStep>{
                new AdventureStep{kind=AdventureStepKind.EVENT},
                new AdventureStep{kind=AdventureStepKind.BATTLE},
                new AdventureStep{kind=AdventureStepKind.RETURN}
            }};
            Assert.AreEqual(AdventureStepKind.EVENT,AdventureOrchestrator.Current(t).kind);
            Assert.AreEqual(AdventureStepKind.BATTLE,AdventureOrchestrator.CompleteCurrent(t).kind);
            Assert.IsFalse(AdventureOrchestrator.IsReadyForReturn(t));
            AdventureOrchestrator.CompleteCurrent(t);
            Assert.IsTrue(AdventureOrchestrator.IsReadyForReturn(t));
        }

        [Test] public void GuildRankUp_ConsumesRequiredPointsAndUpdatesCapacity()
        {
            var s=new GuildState{rank=1,points=120,memberCapacity=5,warehouseCapacity=10};
            var n=GuildProgression.RankUp(s,new GuildRankRule{rank=2,requiredPoints=100,memberCapacity=7,warehouseCapacity=20});
            Assert.AreEqual(2,n.rank);Assert.AreEqual(20,n.points);Assert.AreEqual(7,n.memberCapacity);
        }

        [Test] public void InventoryCapacityFailure_DoesNotMutateSource()
        {
            var s=new InventoryState{capacity=0};
            var r=InventoryWarehouse.Add(s,new InventoryItem{instanceId="I",masterId="M",kind="equipment"});
            Assert.IsFalse(r.ok);Assert.AreEqual(0,s.items.Count);
        }

        [Test] public void GameSaveValidation_DetectsDuplicateInstanceIds()
        {
            var s=new GameSaveState{inventory=new InventoryState{capacity=2,items=new System.Collections.Generic.List<InventoryItem>{
                new InventoryItem{instanceId="I",masterId="A"},new InventoryItem{instanceId="I",masterId="B"}
            }}};
            Assert.AreEqual("SAVE_INVENTORY_DUPLICATE",GameSaveState.Validate(s));
        }
    }
}
#endif
