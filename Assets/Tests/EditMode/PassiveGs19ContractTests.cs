#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
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
 }
}
#endif
