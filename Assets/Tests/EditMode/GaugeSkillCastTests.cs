#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class GaugeSkillCastTests
    {
        [Test]
        public void Gauge_ReachesReadyAt100_AndKeepsOverflow()
        {
            var cfg=new ActionGaugeSettings();
            var s=new ActionGaugeState{actorId="A",gauge=95,agi=100};
            ActionGauge.Advance(s,cfg);
            Assert.IsTrue(ActionGauge.IsReady(s,cfg));
            ActionGauge.ConsumeSuccessful(s,cfg);
            Assert.AreEqual(0,s.gauge,0.000001);
        }

        [Test]
        public void CastingActor_DoesNotAdvanceActionGauge()
        {
            var cfg=new ActionGaugeSettings();
            var s=new ActionGaugeState{gauge=50,agi=10,casting=true};
            ActionGauge.Advance(s,cfg);
            Assert.AreEqual(50,s.gauge);
        }

        [Test]
        public void MpSkill_RejectsInsufficientMp()
        {
            var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=11};
            var actor=new SkillActorState{alive=true,hp=10,mp=10};
            var r=SkillUseCondition.CanUse(skill,actor);
            Assert.IsFalse(r.ok);
            Assert.AreEqual("mp_insufficient",r.reason);
        }

        [Test]
        public void HpCost_CannotKillCaster()
        {
            var skill=new SkillDefinition{id="S",resource=SkillResourceKind.HP,resourceCost=10};
            var actor=new SkillActorState{alive=true,hp=10};
            Assert.IsFalse(SkillUseCondition.CanUse(skill,actor).ok);
        }

        [Test]
        public void Cast_CompletesAfterConfiguredTicks()
        {
            var skill=new SkillDefinition{id="S",castTicks=2};
            var c=CastRuntime.Begin("A",skill,"B");
            Assert.IsFalse(CastRuntime.Advance(c));
            Assert.IsTrue(CastRuntime.Advance(c));
            Assert.IsFalse(c.active);
        }
    }
}
#endif
