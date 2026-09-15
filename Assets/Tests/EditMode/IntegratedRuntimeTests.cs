#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Adventure;
using GuildAdventure.Game.Reward;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class IntegratedRuntimeTests
    {
        [Test] public void SaveRejectsEquipmentOwnerNotInCharacterDomain()
        {
            var s=new GameSaveState();
            s.inventory.capacity=10;
            s.runtimeDomain.equipmentInstances.Add(new EquipmentInstanceSaveRecord{instanceId="I",equipmentId="E",ownerId="UNKNOWN"});
            Assert.AreEqual("EQUIPMENT_OWNER_UNKNOWN",GameSaveState.Validate(s));
        }

        [Test] public void DeepCloneDoesNotShareCharacterSkillLists()
        {
            var s=new GameSaveState();s.inventory.capacity=10;
            s.runtimeDomain.characters.Add(new CharacterSaveRecord{characterId="C",learnedSkillIds=new List<string>{"S"}});
            var c=s.DeepClone();c.runtimeDomain.characters[0].learnedSkillIds.Add("S2");
            Assert.AreEqual(1,s.runtimeDomain.characters[0].learnedSkillIds.Count);
        }

        [Test] public void AdventureRunnerStopsWhenRewardCommitFails()
        {
            var timeline=new AdventureTimeline{steps=new List<AdventureStep>{new AdventureStep{kind=AdventureStepKind.REWARD}}};
            var run=new AdventureRunSnapshot{state=AdventureRunState.ACTIVE};
            var ctx=new AdventureDispatchContext{
                resolveReward=_=>new[]{new RewardItem{kind="item",refId="X",amount=1}},
                commitRewards=_=>false
            };
            var r=AdventureRuntimeRunner.DispatchCurrent(timeline,run,ctx);
            Assert.IsFalse(r.ok);Assert.AreEqual(0,timeline.cursor);Assert.IsFalse(timeline.steps[0].completed);
        }

        [Test] public void ReturnRequiresQuestSuccessPendingReturn()
        {
            var timeline=new AdventureTimeline{steps=new List<AdventureStep>{new AdventureStep{kind=AdventureStepKind.RETURN}}};
            var run=new AdventureRunSnapshot{state=AdventureRunState.ACTIVE};
            var r=AdventureRuntimeRunner.DispatchCurrent(timeline,run,new AdventureDispatchContext());
            Assert.IsFalse(r.ok);Assert.AreEqual("QUEST_SUCCESS_NOT_CONFIRMED",r.reason);
        }
    }
}
#endif
