#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Skills;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class SkillPassiveTriggerTests
    {
        [Test]
        public void FormalCompiler_RejectsUnknownEffect()
        {
            var row=new SkillExportRow{id="S",effects=new[]{new SkillEffect{type="UNKNOWN"}}};
            Assert.Throws<System.ArgumentException>(()=>FormalSkillCompiler.Compile(row));
        }

        [Test]
        public void ApplyEffect_RequiresStatusId()
        {
            var row=new SkillExportRow{id="S",effects=new[]{new SkillEffect{type="APPLY"}}};
            Assert.Throws<System.ArgumentException>(()=>FormalSkillCompiler.Compile(row));
        }

        [Test]
        public void Passive_DuplicateSeries_IsRejected()
        {
            var r=PassiveRuntime.Compile(new[]{
                new PassiveContribution{passiveId="P1",seriesId="SERIES-A"},
                new PassiveContribution{passiveId="P2",seriesId="SERIES-A"}
            });
            Assert.IsFalse(r.ok);
        }

        [Test]
        public void Passive_CanGrantDualWieldCapability()
        {
            var r=PassiveRuntime.Compile(new[]{
                new PassiveContribution{passiveId="P1",capability="DUAL_WIELD"}
            });
            Assert.IsTrue(r.capabilities.Contains("DUAL_WIELD"));
        }

        [Test]
        public void Trigger_OrderUsesPriorityThenFixedBattleOrder()
        {
            var regs=new[]{
                new TriggerRegistration{id="A",ownerId="X",trigger=TriggerEvent.ON_HIT,priority=1},
                new TriggerRegistration{id="B",ownerId="Y",trigger=TriggerEvent.ON_HIT,priority=2},
                new TriggerRegistration{id="C",ownerId="X",trigger=TriggerEvent.ON_HIT,priority=2}
            };
            var order=new Dictionary<string,int>{{"X",0},{"Y",1}};
            var r=TriggerRuntime.Resolve(regs,TriggerEvent.ON_HIT,order);
            Assert.AreEqual("C",r[0].id);
            Assert.AreEqual("B",r[1].id);
            Assert.AreEqual("A",r[2].id);
        }

        [Test]
        public void OnceTrigger_IsConsumed()
        {
            var t=new TriggerRegistration{id="T",once=true};
            TriggerRuntime.MarkConsumed(t);
            Assert.IsTrue(t.consumed);
        }
    }
}
#endif
