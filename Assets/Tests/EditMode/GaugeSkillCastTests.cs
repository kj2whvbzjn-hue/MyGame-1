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
            var s=new ActionGaugeState{actorId="A",gauge=95,speed=10};
            ActionGauge.Advance(s);
            Assert.IsTrue(ActionGauge.IsReady(s));
            ActionGauge.Consume(s);
            Assert.AreEqual(5,s.gauge,0.000001);
        }

        [Test]
        public void CastingActor_DoesNotAdvanceActionGauge()
        {
            var s=new ActionGaugeState{gauge=50,speed=10,casting=true};
            ActionGauge.Advance(s);
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
