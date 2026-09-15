#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class SkillCastRuntimeFormalTests
    {
        sealed class FixedRng : IRandomSource { public double Next01(string purpose)=>0.99; }

        static BattleSnapshotSaveRecord Snapshot()
        {
            var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S",tick=10};
            s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,actionGauge=100,alive=true});
            s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,mp=0,maxMp=0,alive=true});
            s.fixedActorOrder.AddRange(new[]{"A","B"});
            return s;
        }

        [Test] public void CastStart_ReservesFixedTarget_WithoutPayingMpOrStartingCooldown()
        {
            var s=Snapshot();
            var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=5,castTicks=2,cooldownTicks=3};
            var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{
                reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,baseDamage=10}};
            var r=SkillActionTransaction.Execute(s,req,new FixedRng(),new FixedRng(),new FixedRng());
            Assert.IsTrue(r.ok); Assert.IsTrue(r.castingStarted);
            Assert.AreEqual(20,s.actors[0].mp);
            Assert.AreEqual(0,s.actors[0].cooldowns.Count);
            Assert.AreEqual("B",s.actionReservations[0].fixedTargetIds[0]);
            Assert.AreEqual(12,s.actionReservations[0].completeTick);
        }

        [Test] public void CastCompletion_UsesFixedTarget_AndSkipsDeadTargetAtEffectStart()
        {
            var s=Snapshot();
            var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=5,castTicks=1,cooldownTicks=3};
            var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{
                reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,baseDamage=10}};
            Assert.IsTrue(SkillActionTransaction.Execute(s,req,new FixedRng(),new FixedRng(),new FixedRng()).ok);
            SkillActionTransaction.AdvanceCastAndCooldowns(s.actors[0]);
            s.actors[1].hp=0; s.actors[1].alive=false;
            var changedAttack=new BattleAttackProposal{reservationId="OTHER",sourceId="A",targetId="A",skillId="S",damageType=DamageType.PHYSICAL,baseDamage=10};
            var r=SkillActionTransaction.CompleteCast(s,s.actors[0],skill,changedAttack,new FixedRng(),new FixedRng(),new FixedRng());
            Assert.IsTrue(r.ok); Assert.IsTrue(r.executionSkipped);
            Assert.AreEqual("TARGET_INVALID_AT_EFFECT_START",r.reason);
            Assert.AreEqual(20,s.actors[0].mp);
            Assert.AreEqual(1,s.actors[0].cooldowns.Count);
        }

        [Test] public void CastCompletion_RechecksMpBeforeExecution()
        {
            var s=Snapshot();
            var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=5,castTicks=1,cooldownTicks=3};
            var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{
                reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,baseDamage=10}};
            Assert.IsTrue(SkillActionTransaction.Execute(s,req,new FixedRng(),new FixedRng(),new FixedRng()).ok);
            SkillActionTransaction.AdvanceCastAndCooldowns(s.actors[0]);
            s.actors[0].mp=0;
            var r=SkillActionTransaction.CompleteCast(s,s.actors[0],skill,req.attack,new FixedRng(),new FixedRng(),new FixedRng());
            Assert.IsFalse(r.ok);
            Assert.AreEqual("mp_insufficient",r.reason);
        }
    }
}
#endif
