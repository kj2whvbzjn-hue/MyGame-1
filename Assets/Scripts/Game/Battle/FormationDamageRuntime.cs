using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.Battle
{
 public static class FormationDamageRuntime
 {
  public static double Resolve(BattleSnapshotSaveRecord snapshot,string sourceId,SkillTargetRange range)
  {
   if(snapshot==null)return 1d;
   var source=snapshot.actors.Find(x=>x.actorId==sourceId);
   if(source==null)return 1d;
   var allies=snapshot.actors.FindAll(x=>x.alive&&x.hp>0&&x.teamId==source.teamId);
   if(allies.Count==0)return 1d;
   var frontRow=int.MaxValue;
   foreach(var a in allies)if(a.formationRow<frontRow)frontRow=a.formationRow;
   if(source.formationRow<=frontRow)return 1d;
   return range==SkillTargetRange.BACK||range==SkillTargetRange.ALL?1d:0.5d;
  }
 }
}
