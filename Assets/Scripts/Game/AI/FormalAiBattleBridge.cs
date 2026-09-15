using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.AI
{
 public sealed class FormalAiBattleContext
 {
  public FormalAiProgram program;public FormalAiExecutionContext execution;public IRandomSource tieRng;
  public Func<FormalAiNode,SkillDefinition> resolveSkill;public Func<FormalAiNode,IEnumerable<string>> resolveTargetCandidates;public Func<FormalAiNode,SkillDefinition,string,BattleAttackProposal> buildAttack;
 }
 public sealed class FormalAiBattleDecision{public bool ok;public string reason;public FormalAiNode actionNode;public SkillActionRequest request;public List<string> visitedNodeIds=new List<string>();}
 public static class FormalAiBattleBridge
 {
  public static FormalAiBattleDecision Decide(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord actor,FormalAiBattleContext context)
  {
   if(snapshot==null||actor==null||context==null)return Fail("AI_BATTLE_CONTEXT_INVALID");if(!actor.alive||actor.hp<=0)return Fail("AI_BATTLE_ACTOR_DEAD");if(actor.actionGauge<new ActionGaugeSettings().maxGauge)return Fail("AI_BATTLE_GAUGE_NOT_READY");
   var graph=FormalAiV2Executor.Execute(context.program,context.execution);if(!graph.ok)return Fail(graph.reason,graph.visitedNodeIds);var node=graph.action;if(node==null)return Fail("AI_BATTLE_ACTION_MISSING",graph.visitedNodeIds);var skill=context.resolveSkill?.Invoke(node);if(skill==null)return Fail("AI_BATTLE_SKILL_MISSING",graph.visitedNodeIds);var target=AiDecisionTargetRuntime.Select(snapshot,context.resolveTargetCandidates?.Invoke(node),context.tieRng);if(!target.ok)return Fail(target.reason,graph.visitedNodeIds);var attack=context.buildAttack?.Invoke(node,skill,target.targetId);if(attack==null)return Fail("AI_BATTLE_ATTACK_PROPOSAL_MISSING",graph.visitedNodeIds);
   attack.sourceId=actor.actorId;attack.targetId=target.targetId;attack.skillId=skill.id;if(string.IsNullOrWhiteSpace(attack.reservationId))attack.reservationId="AI:"+snapshot.tick+":"+actor.actorId+":"+skill.id;
   return new FormalAiBattleDecision{ok=true,actionNode=node,request=new SkillActionRequest{skill=skill,attack=attack},visitedNodeIds=graph.visitedNodeIds};
  }
  static FormalAiBattleDecision Fail(string reason,List<string> visited=null)=>new FormalAiBattleDecision{ok=false,reason=reason,visitedNodeIds=visited??new List<string>()};
 }
}
