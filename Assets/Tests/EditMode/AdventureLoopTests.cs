#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Adventure;
using GuildAdventure.Game.Reward;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class AdventureLoopTests
    {
        [Test] public void AdventureExp_IsTemporaryUntilReturnConfirmation()
        {
            var r=new AdventureRunSnapshot{runId="R",state=AdventureRunState.ACTIVE};
            AdventureRun.AddTemporaryExperience(r,100);
            Assert.AreEqual(100,r.temporaryExperience);
            AdventureRun.MarkQuestSuccess(r);
            Assert.AreEqual(100,AdventureRun.ConfirmReturn(r));
            Assert.AreEqual(0,r.temporaryExperience);
            Assert.AreEqual(AdventureRunState.RETURNED,r.state);
        }

        [Test] public void Failure_DiscardsTemporaryExp()
        {
            var r=new AdventureRunSnapshot{state=AdventureRunState.ACTIVE,temporaryExperience=99};
            AdventureRun.FailOrAbandon(r,false);
            Assert.AreEqual(0,r.temporaryExperience);
            Assert.AreEqual(AdventureRunState.FAILED,r.state);
        }

        [Test] public void WeightedDrop_UsesDeterministicInjectedRng()
        {
            var t=new DropTableRow{entries=new[]{
                new DropEntry{kind="A",ref_id="1",weight=1,min=1,max=1},
                new DropEntry{kind="B",ref_id="2",weight=3,min=2,max=2}
            }};
            var calls=0;
            var r=RewardDropRuntime.Roll(t,()=>calls++==0?.5:0);
            Assert.AreEqual("B",r.kind);Assert.AreEqual(2,r.amount);
        }
    }
}
#endif
