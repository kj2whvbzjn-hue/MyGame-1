#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.AI;
using GuildAdventure.Game.Monster;
using GuildAdventure.Game.Adventure;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class AiMonsterQuestTests
    {
        [Test] public void CurrentAiProgram_ResolvesAttackAction()
        {
            var json=@"{""schema_version"":""2.0.0"",""data"":[{""program_id"":""AIP-0001"",""entry_instruction"":""I-0001"",""instructions"":[{""instruction_id"":""I-0001"",""op"":""ACTION"",""evaluator"":""action.attack"",""target_selector"":{""selector_id"":""ATS-0001""}}]}]}";
            var p=AiRuntime.Load(json)["AIP-0001"];var d=AiRuntime.Decide(p);
            Assert.IsTrue(d.ok);Assert.AreEqual("action.attack",d.evaluator);Assert.AreEqual("ATS-0001",d.selectorId);
        }

        [Test] public void RequiredMonsterOverride_IsExact()
        {
            var monsters=new Dictionary<string,MonsterRow>{{"MON-1",new MonsterRow{id="MON-1",@params=new MonsterParams{enemy_budget_cost=1}}}};
            var o=new EncounterOverride{mode="required_monsters",required_monsters=new[]{new RequiredMonster{monster_id="MON-1",count=2}}};
            var r=QuestEncounterRuntime.ResolveRequired(o,monsters);
            Assert.AreEqual(1,r.Count);Assert.AreEqual(2,r[0].count);
        }

        [Test] public void BudgetFormation_RespectsAllSpawnTagsAndBudget()
        {
            var m=new MonsterRow{id="M",status="active",@params=new MonsterParams{enemy_budget_cost=2,spawn_weight=1,spawn_tags=new SpawnTags{all=new[]{"ops-test"}}}};
            var r=QuestEncounterRuntime.GenerateBudget(5,new[]{m},new HashSet<string>{"ops-test"},()=>0,10);
            Assert.AreEqual(1,r.Count);Assert.AreEqual(2,r[0].count);
        }
    }
}
#endif
