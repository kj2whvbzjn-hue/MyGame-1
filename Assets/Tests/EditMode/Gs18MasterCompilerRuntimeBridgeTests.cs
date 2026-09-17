#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Skills;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs18MasterCompilerRuntimeBridgeTests
 {
  [Test] public void ApplyEffect_UsesLifecycleValuesFromStatusMaster()
  {
   const string statusJson="{\"schema_version\":\"1.0.0\",\"data\":[{\"id\":\"FUTURE_DOT\",\"status\":\"active\",\"lifecycle_kind\":\"DOT\",\"stack_policy\":\"STACK_SUM\",\"refresh_rule\":\"KEEP\",\"snapshot_policy\":\"SNAPSHOT\",\"dispel_category\":\"DOT\",\"max_stacks\":3,\"resistance_cap_percent\":0,\"removable\":true,\"normal_cleanse_eligible\":false,\"action_disabled\":false,\"remove_on_death\":true,\"remove_on_battle_end\":true}]}";
   var statuses=StatusMasterLoader.Load(statusJson);
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY",range="SINGLE"},effects=new[]{new SkillEffect{type="APPLY",statusId="FUTURE_DOT",power=7,duration=4}}};
   var compiled=FormalSkillCompiler.Compile(skill,statuses).effects[0];
   var request=FormalSkillEffectRuntimeBridge.BuildApplyRequest(compiled,"SRC","I1",25);
   Assert.AreEqual(EffectLifecycleKind.DOT,request.lifecycleKind);
   Assert.AreEqual(EffectStackRule.STACK_SUM,request.stackRule);
   Assert.AreEqual(3,request.maxStacks);
   Assert.AreEqual("KEEP",request.refreshRule);
   Assert.AreEqual("SNAPSHOT",request.snapshotPolicy);
   Assert.AreEqual("DOT",request.dispelCategory);
   Assert.AreEqual(7,request.power,1e-9);
   Assert.AreEqual(4,request.durationTicks);
  }

  [Test] public void ApplyStatus_PropagatesResistanceAndCapabilityWithoutIdLogic()
  {
   const string statusJson="{\"schema_version\":\"1.0.0\",\"data\":[{\"id\":\"FUTURE_DISABLE\",\"status\":\"active\",\"lifecycle_kind\":\"STATUS\",\"stack_policy\":\"UNIQUE_REFRESH\",\"refresh_rule\":\"REFRESH\",\"snapshot_policy\":\"SNAPSHOT\",\"dispel_category\":\"STATUS\",\"max_stacks\":0,\"resistance_cap_percent\":50,\"removable\":true,\"normal_cleanse_eligible\":true,\"action_disabled\":true,\"remove_on_death\":true,\"remove_on_battle_end\":true}]}";
   var statuses=StatusMasterLoader.Load(statusJson);
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY",range="SINGLE"},effects=new[]{new SkillEffect{type="APPLY",statusId="FUTURE_DISABLE",duration=4}}};
   var compiled=FormalSkillCompiler.Compile(skill,statuses).effects[0];
   var request=FormalSkillEffectRuntimeBridge.BuildApplyRequest(compiled,"SRC","I1",80);
   Assert.AreEqual(50,request.statusResistanceCapPercent,1e-9);
   Assert.AreEqual(EffectStackRule.UNIQUE_REFRESH,request.stackRule);
   Assert.AreEqual("REFRESH",request.refreshRule);
   Assert.AreEqual("STATUS",request.dispelCategory);
   Assert.IsTrue(request.actionDisabled);
   Assert.IsTrue(request.normalCleanseEligible);
   var actor=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};
   var snapshot=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="1"};snapshot.actors.Add(actor);snapshot.fixedActorOrder.Add("A");
   var applied=SkillEffectRuntime.Execute(snapshot,actor,request);
   Assert.IsTrue(applied.ok,applied.reason);
   Assert.AreEqual(2,applied.applied.remainingTicks);
   Assert.IsTrue(applied.applied.actionDisabled);
   Assert.AreEqual("STATUS",applied.applied.dispelCategory);
  }

  [Test] public void ApplyEffect_RejectsUnknownStatusBeforeRuntime()
  {
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY"},effects=new[]{new SkillEffect{type="APPLY",statusId="UNKNOWN"}}};
   var ex=Assert.Throws<System.ArgumentException>(()=>FormalSkillCompiler.Compile(skill,new System.Collections.Generic.Dictionary<string,StatusMasterRow>()));
   StringAssert.Contains("STATUS_ID_UNKNOWN",ex.Message);
  }

  [Test] public void Runtime_RejectsMissingStackRuleInsteadOfInferringFromKind()
  {
   var actor=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};
   var r=EffectLifecycleRuntime.Apply(actor,new EffectApplyRequest{instanceId="I",sourceId="SRC",effectId="FUTURE_DOT",kind=EffectLifecycleKind.DOT,baseDurationTicks=4,maxStacks=3});
   Assert.IsFalse(r.ok);
   Assert.AreEqual("EFFECT_STACK_RULE_MISSING",r.reason);
  }
 }
}
#endif
