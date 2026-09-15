#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FatalDamageTriggerRuntimeTests
    {
        [Test] public void FatalTrigger_CanPreventDeathBeforeCommit()
        {
            var a=new TriggerActionContext{actionId="ACT"};
            var regs=new[]{new TriggerRegistration{id="ENDURE",trigger=TriggerEvent.ON_FATAL_DAMAGE,activationChance=1}};
            var r=FatalDamageTriggerRuntime.Resolve(10,-5,"SRC","SRC","DST","SK",0,regs,null,a,null,(q,b,p)=>1);
            Assert.IsTrue(r.prevented); Assert.AreEqual(1,r.hpAfter); Assert.AreEqual(1,r.executed);
            Assert.AreEqual(1,a.activationCount); Assert.AreEqual(1,a.history.Count);
        }

        [Test] public void ChanceFailure_DoesNotConsumeOnceOrActionCount()
        {
            var a=new TriggerActionContext{actionId="ACT"};
            var reg=new TriggerRegistration{id="ENDURE",trigger=TriggerEvent.ON_FATAL_DAMAGE,activationChance=0,once=true};
            var r=FatalDamageTriggerRuntime.Resolve(10,-5,"SRC","SRC","DST","SK",0,new[]{reg},null,a,null,(q,b,p)=>1);
            Assert.IsFalse(r.prevented); Assert.IsFalse(reg.consumed); Assert.AreEqual(0,a.activationCount);
        }
    }
}
#endif
