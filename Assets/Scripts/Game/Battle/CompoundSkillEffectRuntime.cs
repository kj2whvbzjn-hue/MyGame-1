using System;
using System.Collections.Generic;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class CompoundSkillEffectEntry
 {
  public int order;
  public SkillEffectRequest effect;
 }
 public sealed class CompoundSkillEffectResult
 {
  public bool ok;
  public string reason;
  public int executedCount;
  public readonly List<SkillEffectResult> results=new List<SkillEffectResult>();
 }
 public static class CompoundSkillEffectRuntime
 {
  public static CompoundSkillEffectResult Execute(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord target,IEnumerable<CompoundSkillEffectEntry> entries)
  {
   if(snapshot==null||target==null||entries==null)return Fail("COMPOUND_EFFECT_INPUT_INVALID");
   var indexed=new List<Tuple<int,CompoundSkillEffectEntry>>();var index=0;
   foreach(var entry in entries){if(entry==null||entry.effect==null)return Fail("COMPOUND_EFFECT_ENTRY_INVALID");indexed.Add(Tuple.Create(index++,entry));}
   indexed.Sort((a,b)=>{var c=a.Item2.order.CompareTo(b.Item2.order);return c!=0?c:a.Item1.CompareTo(b.Item1);});
   var result=new CompoundSkillEffectResult{ok=true};
   foreach(var row in indexed)
   {
    var one=SkillEffectRuntime.Execute(snapshot,target,row.Item2.effect);result.results.Add(one);
    if(!one.ok){result.ok=false;result.reason=one.reason;return result;}
    result.executedCount++;
   }
   return result;
  }
  static CompoundSkillEffectResult Fail(string reason)=>new CompoundSkillEffectResult{ok=false,reason=reason};
 }
}
