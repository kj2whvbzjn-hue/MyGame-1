#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Character;
using GuildAdventure.Game.Data;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class JobMasterAndRecruitDismissTests
    {
        [Test]
        public void StudioJobsJson_LoadsSevenFormalJobs()
        {
            var json=@"{""schema_version"":""1"",""data"":[
{""id"":""1"",""name"":""a"",""status"":""active"",""params"":{""aptitudes"":{""STR"":15,""VIT"":13,""AGI"":4,""DEX"":10,""INT"":7,""MND"":8,""LUK"":3}}},
{""id"":""2"",""name"":""b"",""status"":""active"",""params"":{""aptitudes"":{""STR"":10,""VIT"":15,""AGI"":3,""DEX"":7,""INT"":8,""MND"":13,""LUK"":4}}},
{""id"":""3"",""name"":""c"",""status"":""active"",""params"":{""aptitudes"":{""STR"":7,""VIT"":8,""AGI"":15,""DEX"":13,""INT"":3,""MND"":4,""LUK"":10}}},
{""id"":""4"",""name"":""d"",""status"":""active"",""params"":{""aptitudes"":{""STR"":8,""VIT"":7,""AGI"":10,""DEX"":15,""INT"":4,""MND"":3,""LUK"":13}}},
{""id"":""5"",""name"":""e"",""status"":""active"",""params"":{""aptitudes"":{""STR"":4,""VIT"":3,""AGI"":8,""DEX"":10,""INT"":15,""MND"":13,""LUK"":7}}},
{""id"":""6"",""name"":""f"",""status"":""active"",""params"":{""aptitudes"":{""STR"":3,""VIT"":4,""AGI"":10,""DEX"":7,""INT"":13,""MND"":15,""LUK"":8}}},
{""id"":""7"",""name"":""g"",""status"":""active"",""params"":{""aptitudes"":{""STR"":13,""VIT"":10,""AGI"":8,""DEX"":3,""INT"":4,""MND"":7,""LUK"":15}}}]}";
            Assert.IsNotNull(JobMasterJsonLoader.LoadCatalog(json).Resolve("5"));
        }

        [Test]
        public void Hire_IsFreeDomainOperationAndCreatesNewIdentity()
        {
            var result=RecruitDismiss.Hire(new List<RecruitCharacter>(),5,
                new HashSet<string>{"TYPE-A"},new HashSet<string>{"JOB-0001"},
                "TYPE-A","JOB-0001","Alice","now",
                new RecruitDefaults{level=1,initialSkillPoints=0,formationPosition="front"},
                Jobs(),()=> "CHAR-NEW");
            Assert.IsTrue(result.ok);
            Assert.AreEqual("CHAR-NEW",result.character.id);
            Assert.AreEqual(1,result.character.level);
            Assert.AreEqual(10,result.character.stats[CharacterStat.STR]);
        }

        [Test]
        public void Dismiss_FailsAtomicallyWhenInventoryWouldOverflow()
        {
            var chars=new[]{new RecruitCharacter{id="C1"}};
            var inv=new[]{new EquipmentInstance{instanceId="I1",equipmentId="E1"}};
            var equipped=new[]{new EquipmentInstance{instanceId="I2",equipmentId="E2",ownerId="C1",slot="weapon1"}};
            var r=RecruitDismiss.Dismiss(chars,"C1",new HashSet<string>(),inv,equipped,1);
            Assert.IsFalse(r.ok);
            Assert.AreEqual("dismiss_inventory_full",r.reason);
        }

        [Test]
        public void Dismiss_ReturnsSameEquipmentInstanceIdThenDeletesCharacterInProposal()
        {
            var chars=new[]{new RecruitCharacter{id="C1"},new RecruitCharacter{id="C2"}};
            var equipped=new[]{new EquipmentInstance{instanceId="I2",equipmentId="E2",ownerId="C1",slot="weapon1"}};
            var r=RecruitDismiss.Dismiss(chars,"C1",new HashSet<string>(),new EquipmentInstance[0],equipped,10);
            Assert.IsTrue(r.ok);
            Assert.AreEqual(1,r.nextCharacters.Count);
            Assert.AreEqual("C2",r.nextCharacters[0].id);
            Assert.AreEqual("I2",r.nextInventory[0].instanceId);
            Assert.IsNull(r.nextInventory[0].ownerId);
        }

        private static JobCatalog Jobs()=>new JobCatalog(new[]{
            J("JOB-0001","a",15,13,4,10,7,8,3),J("2","b",10,15,3,7,8,13,4),J("3","c",7,8,15,13,3,4,10),
            J("4","d",8,7,10,15,4,3,13),J("5","e",4,3,8,10,15,13,7),J("6","f",3,4,10,7,13,15,8),J("7","g",13,10,8,3,4,7,15)
        });
        private static JobDefinition J(string id,string n,int a,int b,int c,int d,int e,int f,int g)=>new JobDefinition{id=id,name=n,growth=new JobGrowth{STR=a,VIT=b,AGI=c,DEX=d,INT=e,MND=f,LUK=g}};
    }
}
#endif
