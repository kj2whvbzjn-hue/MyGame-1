#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class TriggerActionContextTests
    {
        sealed class Rng:IRandomSource { public int calls; public double value; public double Next01(string p){calls++;Assert.AreEqual("PASSIVE_TRIGGER",p);return value;} }

        [Test] public void ChanceZeroAndOne_DoNotConsumeRng()
        {
            var a=new TriggerActionContext(); var rng=new Rng{value=.5};
            Assert.IsFalse(TriggerActivationRuntime.TryActivate(a,"P0",0,rng).ok);
            Assert.AreEqual(0,a.activationCount);
            Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P1",1,rng).ok);
            Assert.AreEqual(1,a.activationCount); Assert.AreEqual(0,rng.calls);
        }

        [Test] public void FailedChance_DoesNotCommitActivation()
        {
            var a=new TriggerActionContext(); var rng=new Rng{value=.9};
            var r=TriggerActivationRuntime.TryActivate(a,"P",.5,rng);
            Assert.IsFalse(r.ok); Assert.AreEqual(0,a.activationCount); Assert.AreEqual(0,a.history.Count);
        }

        [Test] public void SamePassiveCannotReenterUntilReleased()
        {
            var a=new TriggerActionContext();
            Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P",1,null).ok);
            Assert.AreEqual("TRIGGER_SELF_REENTRY",TriggerActivationRuntime.CanActivate(a,"P").reason);
            TriggerActivationRuntime.Release(a,"P");
            Assert.IsTrue(TriggerActivationRuntime.CanActivate(a,"P").ok);
            Assert.AreEqual(1,a.activationCount);
        }

        [Test] public void ActionLimitIsCumulativeAndReleaseDoesNotDecrement()
        {
            var a=new TriggerActionContext{activationLimit=2};
            Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P1",1,null).ok); TriggerActivationRuntime.Release(a,"P1");
            Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P2",1,null).ok); TriggerActivationRuntime.Release(a,"P2");
            Assert.AreEqual(2,a.activationCount);
            Assert.AreEqual(TriggerActivationRuntime.LimitReached,TriggerActivationRuntime.CanActivate(a,"P3").reason);
        }
    }
}
#endif
