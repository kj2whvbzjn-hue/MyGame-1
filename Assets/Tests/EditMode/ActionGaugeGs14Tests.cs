#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Data;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class ActionGaugeGs14Tests
    {
        static ActionGaugeSettings Settings(double max=100,double reevaluate=.1,double success=1,double failure=.5,double agiCoefficient=1,double speedMultiplier=1)
            => new ActionGaugeSettings{maxGauge=max,aiReevaluationRatio=reevaluate,successfulActionConsumeRatio=success,failedExecutionConsumeRatio=failure,agiGaugeCoefficient=agiCoefficient,actionSpeedMultiplier=speedMultiplier};

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
            ActionGauge.Advance(s,Settings());
            Assert.AreEqual(100,s.gauge,1e-9);
        }

        [Test] public void SuccessfulAndFailedConsumption_UseConfiguredRatios()
        {
            var cfg=Settings(120,.2,.75,.25);
            var success=new ActionGaugeState{gauge=120};
            var failure=new ActionGaugeState{gauge=120};
            ActionGauge.ConsumeSuccessful(success,cfg);
            ActionGauge.ConsumeFailedExecution(failure,cfg);
            Assert.AreEqual(30,success.gauge,1e-9);
            Assert.AreEqual(90,failure.gauge,1e-9);
        }

        [Test] public void DeadOrCastingActor_DoesNotGainGauge()
        {
            var cfg=Settings();
            var dead=new ActionGaugeState{agi=50,gauge=10,alive=false};
            var casting=new ActionGaugeState{agi=50,gauge=10,alive=true,casting=true};
            ActionGauge.Advance(dead,cfg);
            ActionGauge.Advance(casting,cfg);
            Assert.AreEqual(10,dead.gauge,1e-9);
            Assert.AreEqual(10,casting.gauge,1e-9);
        }

        [Test] public void ExportLoader_ReadsAllBattleFlowActionGaugeBalance()
        {
            const string json="{\"data\":[{\"id\":\"ADV-0001\",\"status\":\"active\",\"enabled\":true,\"params\":{\"game_runtime\":{\"battle_flow\":{\"action_gauge\":{\"max\":120,\"ai_reevaluation_ratio\":0.2,\"successful_action_consume_ratio\":0.75,\"failed_execution_consume_ratio\":0.25,\"agi_gauge_coefficient\":1.25,\"action_speed_multiplier\":0.8}}}}}]}";
            var cfg=AdventureBattleSettingsJsonLoader.LoadActionGaugeSettings(json);
            Assert.AreEqual(120,cfg.maxGauge,1e-9);
            Assert.AreEqual(24,cfg.AiReevaluationInterval,1e-9);
            Assert.AreEqual(90,cfg.SuccessfulActionConsume,1e-9);
            Assert.AreEqual(30,cfg.FailedExecutionConsume,1e-9);
            Assert.AreEqual(1.25,cfg.agiGaugeCoefficient,1e-9);
            Assert.AreEqual(.8,cfg.actionSpeedMultiplier,1e-9);
        }

        [Test] public void ExportLoader_RejectsMissingBalanceInsteadOfDefaulting()
        {
            const string json="{\"data\":[{\"id\":\"ADV-0001\",\"status\":\"active\",\"enabled\":true,\"params\":{\"game_runtime\":{\"battle_flow\":{\"action_gauge\":{\"max\":100,\"ai_reevaluation_ratio\":0.1,\"successful_action_consume_ratio\":1,\"failed_execution_consume_ratio\":0.5}}}}}]}";
            var ex=Assert.Throws<System.ArgumentException>(()=>AdventureBattleSettingsJsonLoader.LoadActionGaugeSettings(json));
            Assert.AreEqual("ACTION_GAUGE_AGI_COEFFICIENT_INVALID",ex.Message);
        }

        [Test] public void TickRuntime_UsesInjectedGaugeMaximum()
        {
            var actor=new BattleActorSaveRecord{actorId="A",teamId="ALLY",hp=10,maxHp=10,mp=0,maxMp=0,alive=true,speed=100,actionGauge=45};
            var snapshot=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed=1,actors=new System.Collections.Generic.List<BattleActorSaveRecord>{actor},fixedActorOrder=new System.Collections.Generic.List<string>{"A"}};
            var result=BattleTickRuntime.Advance(snapshot,new BattleTickOptions{actionGaugeSettings=Settings(50)});
            Assert.IsTrue(result.ok,result.reason);
            Assert.AreEqual(50,actor.actionGauge,1e-9);
        }

        [Test] public void TickRuntime_RejectsMissingGaugeBalance()
        {
            var actor=new BattleActorSaveRecord{actorId="A",teamId="ALLY",hp=10,maxHp=10,alive=true};
            var snapshot=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed=1,actors=new System.Collections.Generic.List<BattleActorSaveRecord>{actor},fixedActorOrder=new System.Collections.Generic.List<string>{"A"}};
            var result=BattleTickRuntime.Advance(snapshot);
            Assert.IsFalse(result.ok);Assert.AreEqual("ACTION_GAUGE_SETTINGS_MISSING",result.reason);
        }
    }
}
#endif
