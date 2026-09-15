#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class C02ReservationOwnershipTests
    {
        sealed class Rng:IRandomSource { public double Next01(string purpose)=>0; }

        [Test] public void InstantAction_CommitsExactlyOneReservation()
        {
            var s=Snapshot(); var request=Request(0,"R-I"); var rng=new Rng();
            var r=SkillActionTransaction.Execute(s,request,rng,rng,rng);
            Assert.IsTrue(r.ok,r.reason); Assert.AreEqual(1,s.actionReservations.Count);
            Assert.AreSame(r.reservation,s.actionReservations[0]);
        }

        [Test] public void CastCompletion_ReusesSameReservationObject()
        {
            var s=Snapshot(); var request=Request(2,"R-C"); var rng=new Rng();
            var started=SkillActionTransaction.Execute(s,request,rng,rng,rng);
            Assert.IsTrue(started.ok); Assert.IsTrue(started.castingStarted); Assert.AreEqual(1,s.actionReservations.Count);
            var reserved=s.actionReservations[0];
            s.actors[0].cast.active=false; s.actors[0].cast.remainingTicks=0;
            var completed=SkillActionTransaction.CompleteCast(s,s.actors[0],request.skill,request.attack,rng,rng,rng);
            Assert.IsTrue(completed.ok,completed.reason); Assert.AreEqual(1,s.actionReservations.Count); Assert.AreSame(reserved,s.actionReservations[0]);
        }

        static BattleSnapshotSaveRecord Snapshot()
        {
            var s=new BattleSnapshotSaveRecord(); s.fixedActorOrder.AddRange(new[]{"A","B"});
            s.actors.Add(new BattleActorSaveRecord{actorId="A",alive=true,hp=100,maxHp=100,mp=100,maxMp=100,actionGauge=100});
            s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=100,maxHp=100,mp=0,maxMp=0}); return s;
        }
        static SkillActionRequest Request(int cast,string id)=>new SkillActionRequest{
            skill=new SkillDefinition{id="S",castTicks=cast,resource=SkillResourceKind.MP,resourceCost=0,cooldownTicks=0},
            attack=new BattleAttackProposal{reservationId=id,sourceId="A",targetId="B",skillId="S",hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=1}
        };
    }
}
#endif
