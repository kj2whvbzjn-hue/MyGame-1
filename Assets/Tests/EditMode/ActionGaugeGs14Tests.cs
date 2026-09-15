#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class ActionGaugeGs14Tests
    {
        [TestCase(10,11)]
        [TestCase(50,15)]
        [TestCase(100,20)]
        public void BaseGainPerTick_FollowsGs14(double agi,double expected)
        {
            Assert.AreEqual(expected,ActionGauge.BaseGainPerTick(agi),1e-9);
        }

        [Test] public void Advance_StopsAtConfiguredMaximum()
        {
            var s=new ActionGaugeState{agi=100,gauge=95,alive=true};
            var cfg=new ActionGaugeSettings();
            ActionGauge.Advance(s,cfg);
            Assert.AreEqual(100,s.gauge,1e-9);
        }

        [Test] public void SuccessfulAndFailedConsumption_UseConfiguredRatios()
        {
            var cfg=new ActionGaugeSettings{maxGauge=100,successfulActionConsumeRatio=1,failedExecutionConsumeRatio=.5};
            var success=new ActionGaugeState{gauge=100};
            var failure=new ActionGaugeState{gauge=100};
            ActionGauge.ConsumeSuccessful(success,cfg);
            ActionGauge.ConsumeFailedExecution(failure,cfg);
            Assert.AreEqual(0,success.gauge,1e-9);
            Assert.AreEqual(50,failure.gauge,1e-9);
        }

        [Test] public void DeadOrCastingActor_DoesNotGainGauge()
        {
            var cfg=new ActionGaugeSettings();
            var dead=new ActionGaugeState{agi=50,gauge=10,alive=false};
            var casting=new ActionGaugeState{agi=50,gauge=10,alive=true,casting=true};
            ActionGauge.Advance(dead,cfg);
            ActionGauge.Advance(casting,cfg);
            Assert.AreEqual(10,dead.gauge,1e-9);
            Assert.AreEqual(10,casting.gauge,1e-9);
        }
    }
}
#endif
