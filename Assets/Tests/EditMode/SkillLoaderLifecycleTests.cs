#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Skills;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class SkillLoaderLifecycleTests
    {
        [Test]
        public void ExportSkillLoader_MapsMpCastAndCooldown()
        {
            var json=@"{""schema_version"":""1.0.0"",""data"":[{""schemaVersion"":1,""id"":""SKL-0008"",""name"":""戦意高揚"",""target"":{""side"":""ALLY"",""range"":""ALL""},""resource"":{""mpCost"":72,""cooldown"":14,""activationPriority"":0,""castTime"":7}}]}";
            var row=SkillExportLoader.Load(json)[0];
            var r=SkillExportLoader.ToRuntime(row);
            Assert.AreEqual(72,r.resourceCost);Assert.AreEqual(14,r.cooldownTicks);Assert.AreEqual(7,r.castTicks);
            Assert.AreEqual(SkillTargetKind.ALL_ALLIES,r.target);
        }

        [Test]
        public void Lifecycle_DoesNotMutateSourceProposal()
        {
            var src=new List<AppliedEffect>();
            var r=ApplyLifecycle.Apply(src,new AppliedEffect{instanceId="1",effectId="POISON",kind=ApplyKind.STATUS,remainingTicks=3});
            Assert.IsTrue(r.ok);Assert.AreEqual(0,src.Count);Assert.AreEqual(1,r.next.Count);
        }

        [Test]
        public void Lifecycle_ExpiresAtZero()
        {
            var src=new[]{new AppliedEffect{instanceId="1",effectId="POISON",kind=ApplyKind.STATUS,remainingTicks=1}};
            Assert.AreEqual(0,ApplyLifecycle.AdvanceAndExpire(src).Count);
        }

        [Test]
        public void CurrentConditionEngineContract_TargetPoisoned()
        {
            var c=new CompiledCondition{scope="TARGET",property=ConditionProperty.TARGET_POISONED,enginePredicate="target_poisoned",expected=true};
            var effects=new[]{new AppliedEffect{instanceId="1",effectId="POISON",kind=ApplyKind.STATUS,remainingTicks=3}};
            Assert.IsTrue(ConditionRuntime.Evaluate(c,effects));
        }
    }
}
#endif
