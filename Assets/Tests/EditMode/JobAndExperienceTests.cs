#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Character;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class JobAndExperienceTests
    {
        [Test]
        public void JobCatalog_RequiresSevenJobsAndGrowthTotal60()
        {
            Assert.DoesNotThrow(() => new JobCatalog(Jobs()));
        }

        [Test]
        public void Transfer_PreservesCharacterDataByReturningOnlyJobChangeAndHistory()
        {
            var r=JobTransfer.Prepare("JOB-0001","JOB-0005",10,"base","",
                "2026-09-15T12:00:00+09:00",new JobCatalog(Jobs()));
            Assert.IsTrue(r.ok);
            Assert.AreEqual("JOB-0005",r.nextJobId);
            Assert.AreEqual(0,r.history.costGold);
            Assert.AreEqual(10,r.history.level);
        }

        [Test]
        public void Transfer_IsRejectedDuringAdventure()
        {
            var r=JobTransfer.Prepare("JOB-0001","JOB-0005",10,"adventure","Q1","now",new JobCatalog(Jobs()));
            Assert.IsFalse(r.ok);
        }

        [Test]
        public void Experience_IsEvenlyDistributedAndRemainderDiscarded()
        {
            var states=new Dictionary<string,AdventureEndState> {
                {"A",new AdventureEndState{alive=true,hp=1}},
                {"B",new AdventureEndState{alive=true,hp=1}},
                {"C",new AdventureEndState{alive=false,hp=0}}
            };
            var r=ExperienceProgression.Distribute(101,new[]{"A","B","C"},states);
            Assert.AreEqual(50,r.awards["A"]);
            Assert.AreEqual(50,r.awards["B"]);
            Assert.IsFalse(r.awards.ContainsKey("C"));
            Assert.AreEqual(1,r.discardedRemainder);
        }

        [Test]
        public void MultipleLevelUps_UseCurrentJobGrowthAndGrantOneSpEach()
        {
            var c=new CharacterProgressionState {
                id="A",jobId="JOB-0005",level=1,experience=0,skillPoints=0,
                stats=Stats(10)
            };
            var table=new Dictionary<int,int>{{1,10},{2,20},{3,30}};
            var rng=new SequenceRandomSource(new[]{
                .99,.99,.99,.99,.99,.99,.99, .99,.99,.99,.99,.99,.99,.99
            });
            var r=ExperienceProgression.Apply(c,30,table,50,new JobCatalog(Jobs()),rng);
            Assert.IsTrue(r.ok);
            Assert.AreEqual(3,r.next.level);
            Assert.AreEqual(2,r.levelsGained);
            Assert.AreEqual(2,r.next.skillPoints);
            // Mage STR growth=4 and rolls .99 => no STR increase.
            Assert.AreEqual(10,r.next.stats[CharacterStat.STR]);
            // Mage INT growth=15 and rolls .99 => guaranteed +1 each level.
            Assert.AreEqual(12,r.next.stats[CharacterStat.INT]);
        }

        private static Dictionary<CharacterStat,int> Stats(int v) {
            var d=new Dictionary<CharacterStat,int>();
            foreach(CharacterStat s in System.Enum.GetValues(typeof(CharacterStat))) d[s]=v;
            return d;
        }

        private static JobDefinition[] Jobs() => new[] {
            J("JOB-0001","剣士",15,13,4,10,7,8,3),
            J("JOB-0002","騎士",10,15,3,7,8,13,4),
            J("JOB-0003","盗賊",7,8,15,13,3,4,10),
            J("JOB-0004","狩人",8,7,10,15,4,3,13),
            J("JOB-0005","魔術師",4,3,8,10,15,13,7),
            J("JOB-0006","神官",3,4,10,7,13,15,8),
            J("JOB-0007","冒険家",13,10,8,3,4,7,15)
        };
        private static JobDefinition J(string id,string name,int a,int b,int c,int d,int e,int f,int g)
            => new JobDefinition{id=id,name=name,growth=new JobGrowth{STR=a,VIT=b,AGI=c,DEX=d,INT=e,MND=f,LUK=g}};
    }
}
#endif
