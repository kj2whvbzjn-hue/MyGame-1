#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;\nusing GuildAdventure.Game.Data;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PassiveGs19ContractTests
 {
  static PassiveContribution P(string id,string series)=>new PassiveContribution{passiveId=id,seriesId=series,property="ATK",value=1};
  static PassiveRuntimeSettings Settings(int slots=7)=>new PassiveRuntimeSettings{maxPassiveSlots=slots,periodicRecoveryIntervalTicks=13};

  [Test] public void Compile_RejectsMissingSeriesId(){var r=PassiveRuntime.Compile(new[]{P("P1",null)},Settings());Assert.IsFalse(r.ok);Assert.AreEqual("PASSIVE_SERIES_ID_MISSING",r.reason);}

  [Test] public void Compile_UsesConfiguredSlotLimit(){var rows=new List<PassiveContribution>();for(var i=0;i<8;i++)rows.Add(P("P"+i,"S"+i));var r=PassiveRuntime.Compile(rows,Settings(7));Assert.IsFalse(r.ok);Assert.AreEqual("PASSIVE_SLOT_LIMIT_EXCEEDED",r.reason);}

  [Test] public void Compile_AcceptsConfiguredUniqueSeries(){var rows=new List<PassiveContribution>();for(var i=0;i<7;i++)rows.Add(P("P"+i,"S"+i));var r=PassiveRuntime.Compile(rows,Settings(7));Assert.IsTrue(r.ok);Assert.AreEqual(7,r.contributions.Count);}

  [Test] public void Compile_RejectsMissingRuntimeSettings(){var r=PassiveRuntime.Compile(new[]{P("P1","S1")},null);Assert.IsFalse(r.ok);Assert.AreEqual("PASSIVE_SETTINGS_MISSING",r.reason);}
  [Test] public void AdventureSettings_OwnsPassiveSlotLimit()
  {
   const string json="{\"data\":[{\"status\":\"active\",\"enabled\":true,\"params\":{\"game_runtime\":{\"skill_loadout\":{\"passive_slots\":9}}}}]}";
   var settings=AdventureBattleSettingsJsonLoader.LoadPassiveRuntimeSettings(json);
   Assert.AreEqual(9,settings.maxPassiveSlots);
  }

  [Test] public void PeriodicTiming_IsOwnedByEachPassiveContribution()
  {
   var settings=new PassiveRuntimeSettings{maxPassiveSlots=2};
   var c=PassiveRuntime.Compile(new[]{new PassiveContribution{passiveId="P",seriesId="S",property=PassiveRuntime.PeriodicHpRecoveryPercent,value=10,periodicIntervalTicks=7,periodicInitialDelayTicks=3}},settings);
   var a=new GuildAdventure.Game.Save.BattleActorSaveRecord{actorId="A",hp=50,maxHp=100,alive=true};
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,2,settings));
   Assert.AreEqual(10,PassiveRuntime.RecoverPeriodic(a,c,3,settings));
   Assert.AreEqual(0,PassiveRuntime.RecoverPeriodic(a,c,9,settings));
   Assert.AreEqual(10,PassiveRuntime.RecoverPeriodic(a,c,10,settings));
  }
 }
}
#endif
