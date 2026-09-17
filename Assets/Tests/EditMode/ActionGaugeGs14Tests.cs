#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Data;
using GuildAdventure.Game.Save;

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

        [Test] public void ExportLoader_ReadsBattleFlowActionGauge()
        {
            const string json="{\"data\":[{\"id\":\"ADV-0001\",\"status\":\"active\",\"enabled\":true,\"params\":{\"game_runtime\":{\"battle_flow\":{\"action_gauge\":{\"max\":120,\"ai_reevaluation_ratio\":0.2,\"successful_action_consume_ratio\":0.75,\"failed_execution_consume_ratio\":0.25}}}}}]}";
            var cfg=AdventureBattleSettingsJsonLoader.LoadActionGaugeSettings(json);
            Assert.AreEqual(120,cfg.maxGauge,1e-9);
            Assert.AreEqual(24,cfg.AiReevaluationInterval,1e-9);
            Assert.AreEqual(90,cfg.SuccessfulActionConsume,1e-9);
            Assert.AreEqual(30,cfg.FailedExecutionConsume,1e-9);
            Assert.AreEqual(1,cfg.agiGaugeCoefficient,1e-9);
            Assert.AreEqual(1,cfg.actionSpeedMultiplier,1e-9);
        }

        [Test] public void TickRuntime_UsesInjectedGaugeMaximum()
        {
            var actor=new BattleActorSaveRecord{actorId="A",teamId="ALLY",hp=10,maxHp=10,mp=0,maxMp=0,alive=true,speed=100,actionGauge=45};
            var snapshot=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed=1,actors=new System.Collections.Generic.List<BattleActorSaveRecord>{actor},fixedActorOrder=new System.Collections.Generic.List<string>{"A"}};
            var result=BattleTickRuntime.Advance(snapshot,new BattleTickOptions{actionGaugeSettings=new ActionGaugeSettings{maxGauge=50,aiReevaluationRatio=.1,successfulActionConsumeRatio=1,failedExecutionConsumeRatio=.5}});
            Assert.IsTrue(result.ok,result.reason);
            Assert.AreEqual(50,actor.actionGauge,1e-9);
        }
    }
}
#endif
