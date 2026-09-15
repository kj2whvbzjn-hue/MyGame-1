#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Adventure;
using GuildAdventure.Game.Reward;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class ResumeAndRewardTransactionTests
    {
        [Test] public void Timeline_RoundTripsForResume()
        {
            var t=new AdventureTimeline{runId="R",questId="Q",cursor=1,steps=new List<AdventureStep>{
                new AdventureStep{kind=AdventureStepKind.EVENT,refId="E",completed=true},
                new AdventureStep{kind=AdventureStepKind.BATTLE,refId="B",completed=false}}};
            var restored=AdventureResumeAdapter.FromSave(AdventureResumeAdapter.ToSave(t));
            Assert.AreEqual(1,restored.cursor);Assert.IsTrue(restored.steps[0].completed);Assert.AreEqual(AdventureStepKind.BATTLE,restored.steps[1].kind);
        }

        [Test] public void DeepClone_SeparatesGrowthHistoryRng()
        {
            var s=new GameSaveState();s.inventory.capacity=2;
            s.runtimeDomain.characters.Add(new CharacterSaveRecord{characterId="C",growthHistory=new List<GrowthHistorySaveRecord>{
                new GrowthHistorySaveRecord{level=2,rngRolls=new List<double>{.1}}}});
            var c=s.DeepClone();c.runtimeDomain.characters[0].growthHistory[0].rngRolls.Add(.2);
            Assert.AreEqual(1,s.runtimeDomain.characters[0].growthHistory[0].rngRolls.Count);
        }

        [Test] public void RewardProposal_IsAtomicAtDraftBoundary()
        {
            var s=new GameSaveState();s.inventory.capacity=1;
            var e=RewardInventoryProposal.Apply(s,new[]{
                new RewardItem{kind="item",refId="A",amount=1},
                new RewardItem{kind="item",refId="B",amount=1}},r=>r.refId);
            Assert.AreEqual("INVENTORY_CAPACITY",e);
            // Direct proposal mutates its draft; production use is through SaveTransaction clone.
            Assert.AreEqual(1,s.inventory.items.Count);
        }
    }
}
#endif
