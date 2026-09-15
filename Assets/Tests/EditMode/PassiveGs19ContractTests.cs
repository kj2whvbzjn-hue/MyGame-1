#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PassiveGs19ContractTests
 {
  static PassiveContribution P(string id,string series)=>new PassiveContribution{passiveId=id,seriesId=series,property="ATK",value=1};

  [Test] public void Compile_RejectsMissingSeriesId(){var r=PassiveRuntime.Compile(new[]{P("P1",null)});Assert.IsFalse(r.ok);Assert.AreEqual("PASSIVE_SERIES_ID_MISSING",r.reason);}

  [Test] public void Compile_RejectsMoreThanFiveSlots(){var rows=new List<PassiveContribution>();for(var i=0;i<6;i++)rows.Add(P("P"+i,"S"+i));var r=PassiveRuntime.Compile(rows);Assert.IsFalse(r.ok);Assert.AreEqual("PASSIVE_SLOT_LIMIT_EXCEEDED",r.reason);}

  [Test] public void Compile_AcceptsFiveUniqueSeries(){var rows=new List<PassiveContribution>();for(var i=0;i<5;i++)rows.Add(P("P"+i,"S"+i));var r=PassiveRuntime.Compile(rows);Assert.IsTrue(r.ok);Assert.AreEqual(5,r.contributions.Count);}
 }
}
#endif
