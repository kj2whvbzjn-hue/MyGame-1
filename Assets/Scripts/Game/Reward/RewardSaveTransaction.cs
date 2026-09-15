using System;
using System.Collections.Generic;
using GuildAdventure.Game.Inventory;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Reward
{
    public static class RewardInventoryProposal
    {
        public static string Apply(GameSaveState save,IEnumerable<RewardItem> rewards,Func<RewardItem,string> instanceIdFactory)
        {
            if(save==null||rewards==null||instanceIdFactory==null)return "REWARD_INPUT_INVALID";
            var inv=save.inventory;
            foreach(var r in rewards)
            {
                if(r==null||r.amount<=0)return "REWARD_ITEM_INVALID";
                // Keep reward units as one inventory instance unless the master later defines stack semantics.
                var item=new InventoryItem{
                    instanceId=instanceIdFactory(r),masterId=r.refId,kind=r.kind,amount=r.amount};
                var proposal=InventoryWarehouse.Add(inv,item);
                if(!proposal.ok)return proposal.reason;
                inv=proposal.next;
            }
            save.inventory=inv;
            return null;
        }
    }

    public static class RewardSaveTransaction
    {
        public static bool Commit(
            SaveTransaction<GameSaveState> transaction,
            GameSaveState current,
            IEnumerable<RewardItem> rewards,
            Func<RewardItem,string> instanceIdFactory,
            out GameSaveState committed,
            out string error)
        {
            committed=null; error=null;
            if(transaction==null){error="SAVE_TRANSACTION_MISSING";return false;}
            return transaction.Execute(
                current,
                draft=>{
                    var e=RewardInventoryProposal.Apply(draft,rewards,instanceIdFactory);
                    if(e!=null)throw new InvalidOperationException(e);
                },
                s=>GameSaveState.Validate(s)==null,
                out committed,
                out error);
        }
    }
}
