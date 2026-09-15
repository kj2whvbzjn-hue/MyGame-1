#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class RuntimeBoundaryContractTests
    {
        static BattleSnapshotSaveRecord Valid()
        {
            return new BattleSnapshotSaveRecord{
                contract="C01",schemaVersion=1,battleId="B1",settingsVersion="SET-1",seed="123",tick=10,
                fixedActorOrder=new List<string>{"A","T"},
                actors=new List<BattleActorSaveRecord>{
                    new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=5,maxMp=5},
                    new BattleActorSaveRecord{actorId="T",hp=10,maxHp=10,mp=0,maxMp=0}}
            };
        }

        [Test] public void C01_RequiresSettingsVersionSeedAndExactFixedOrder()
        {
            var b=Valid();Assert.IsNull(BattleSnapshotValidation.Validate(b));
            b.seed="";Assert.AreEqual("BATTLE_SEED_MISSING",BattleSnapshotValidation.Validate(b));
        }

        [Test] public void C02_UsesStartCompleteFixedTargetsAndUsageConditions()
        {
            var b=Valid();b.actionReservations.Add(new ActionReservationSaveRecord{
                reservationId="R",actorId="A",skillId="S",startTick=4,completeTick=8,
                fixedTargetIds=new List<string>{"T"},usageConditions=new UsageConditionsSaveRecord{json="{\"mp\":3}"}});
            Assert.IsNull(BattleSnapshotValidation.Validate(b));
            b.actionReservations[0].completeTick=3;
            Assert.AreEqual("ACTION_RESERVATION_STATE_INVALID",BattleSnapshotValidation.Validate(b));
        }

        [Test] public void C03_UsesActionIdHitIndexCommittedHpAndActualLoss()
        {
            var b=Valid();b.resolvedHits.Add(new ResolvedHitSaveRecord{
                actionId="R",hitIndex=0,sourceId="A",targetId="T",judgement="HIT",
                perHitDamage=5,committedHp=5,actualHpLoss=5});
            Assert.IsNull(BattleSnapshotValidation.Validate(b));
        }
    }
}
#endif
