#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FatalDamageInterruptTests
    {
        [Test] public void FatalCandidate_IsExposedBeforeHpCommit_AndCanBePrevented()
        {
            var called=false;
            var r=DamageDefense.CommitHp(10,20,(before,projected)=>{
                called=true;
                Assert.AreEqual(10,before);
                Assert.AreEqual(-10,projected);
                return 1;
            });
            Assert.IsTrue(called);
            Assert.IsTrue(r.fatalCandidate);
            Assert.IsTrue(r.fatalPrevented);
            Assert.AreEqual(1,r.hpAfter);
            Assert.AreEqual(9,r.actualHpLoss);
        }

        [Test] public void FatalCandidate_CommitsZero_WhenInterruptDoesNotResolve()
        {
            var r=DamageDefense.CommitHp(10,20,(before,projected)=>null);
            Assert.IsTrue(r.fatalCandidate);
            Assert.IsFalse(r.fatalPrevented);
            Assert.AreEqual(0,r.hpAfter);
            Assert.AreEqual(10,r.actualHpLoss);
        }

        [Test] public void NonFatalDamage_DoesNotInvokeFatalInterrupt()
        {
            var called=false;
            var r=DamageDefense.CommitHp(10,3,(before,projected)=>{called=true;return 10;});
            Assert.IsFalse(called);
            Assert.IsFalse(r.fatalCandidate);
            Assert.AreEqual(7,r.hpAfter);
        }
    }
}
#endif
