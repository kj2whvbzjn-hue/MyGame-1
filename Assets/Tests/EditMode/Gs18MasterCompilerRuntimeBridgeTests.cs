#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Skills;

namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs18MasterCompilerRuntimeBridgeTests
 {
  [Test] public void ApplyEffect_UsesLifecycleValuesFromStatusMaster()
  {
   const string statusJson="{\"schema_version\":\"1.0.0\",\"data\":[{\"id\":\"FUTURE_DOT\",\"status\":\"active\",\"lifecycle_kind\":\"DOT\",\"max_stacks\":3,\"resistance_cap_percent\":0,\"removable\":true,\"normal_cleanse_eligible\":false,\"action_disabled\":false}]}";
   var statuses=StatusMasterLoader.Load(statusJson);
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY",range="SINGLE"},effects=new[]{new SkillEffect{type="APPLY",statusId="FUTURE_DOT",power=7,duration=4}}};
   var compiled=FormalSkillCompiler.Compile(skill,statuses).effects[0];
   var request=FormalSkillEffectRuntimeBridge.BuildApplyRequest(compiled,"SRC","I1",25);
   Assert.AreEqual(EffectLifecycleKind.DOT,request.lifecycleKind);
   Assert.AreEqual(3,request.maxStacks);
   Assert.AreEqual(7,request.power,1e-9);
   Assert.AreEqual(4,request.durationTicks);
  }

  [Test] public void ApplyStatus_PropagatesResistanceAndCapabilityWithoutIdLogic()
  {
   const string statusJson="{\"schema_version\":\"1.0.0\",\"data\":[{\"id\":\"FUTURE_DISABLE\",\"status\":\"active\",\"lifecycle_kind\":\"STATUS\",\"max_stacks\":0,\"resistance_cap_percent\":50,\"removable\":true,\"normal_cleanse_eligible\":true,\"action_disabled\":true}]}";
   var statuses=StatusMasterLoader.Load(statusJson);
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY",range="SINGLE"},effects=new[]{new SkillEffect{type="APPLY",statusId="FUTURE_DISABLE",duration=4}}};
   var compiled=FormalSkillCompiler.Compile(skill,statuses).effects[0];
   var request=FormalSkillEffectRuntimeBridge.BuildApplyRequest(compiled,"SRC","I1",80);
   Assert.AreEqual(50,request.statusResistanceCapPercent,1e-9);
   Assert.IsTrue(request.actionDisabled);
   Assert.IsTrue(request.normalCleanseEligible);
   var actor=new GuildAdventure.Game.Save.BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};
   var applied=EffectLifecycleRuntime.Apply(actor,new EffectApplyRequest{instanceId=request.instanceId,sourceId=request.sourceId,effectId=request.effectId,kind=request.lifecycleKind,baseDurationTicks=request.durationTicks,statusResistancePercent=request.statusResistancePercent,statusResistanceCapPercent=request.statusResistanceCapPercent,maxStacks=request.maxStacks,removable=request.removable,protectedEffect=request.protectedEffect,normalCleanseEligible=request.normalCleanseEligible,actionDisabled=request.actionDisabled});
   Assert.IsTrue(applied.ok,applied.reason);
   Assert.AreEqual(2,applied.effectiveDurationTicks);
   Assert.IsTrue(applied.applied.actionDisabled);
  }

  [Test] public void ApplyEffect_RejectsUnknownStatusBeforeRuntime()
  {
   var skill=new SkillExportRow{id="S",schemaVersion=1,target=new SkillTarget{side="ENEMY"},effects=new[]{new SkillEffect{type="APPLY",statusId="UNKNOWN"}}};
   var ex=Assert.Throws<System.ArgumentException>(()=>FormalSkillCompiler.Compile(skill,new System.Collections.Generic.Dictionary<string,StatusMasterRow>()));
   StringAssert.Contains("STATUS_ID_UNKNOWN",ex.Message);
  }
 }
}
#endif
