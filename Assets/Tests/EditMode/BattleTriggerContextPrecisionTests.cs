#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleTriggerContextPrecisionTests
    {
        [Test]
        public void ResolvedHitTriggerContext_PreservesActualHpLossPrecision()
        {
            var hit=new ResolvedHitSaveRecord
            {
                actionId="ACT",
                sourceId="SRC",
                targetId="DST",
                judgement="HIT",
                actualHpLoss=2.5
            };

            var dispatches=BattleEffectLifecycle.DispatchResolvedHitEvents(hit,null,null);
            var damage=dispatches.Find(x=>x.trigger==TriggerEvent.ON_DAMAGE_DEALT);

            Assert.IsNotNull(damage);
            Assert.AreEqual(2.5,damage.context.actualHpLoss,0.000001);
        }
    }
}
#endif
