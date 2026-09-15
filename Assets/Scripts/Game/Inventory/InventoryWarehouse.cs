using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Inventory
{
    [Serializable]
    public sealed class InventoryItem
    {
        public string instanceId,masterId,kind;
        public int amount=1;
    }

    [Serializable]
    public sealed class InventoryState
    {
        public int capacity;
        public List<InventoryItem> items=new List<InventoryItem>();
    }

    public sealed class InventoryResult
    {
        public bool ok; public string reason;
        public InventoryState next;
    }

    public static class InventoryWarehouse
    {
        public static InventoryResult Add(InventoryState source,InventoryItem item)
        {
            if(source==null||item==null)throw new ArgumentNullException();
            if(string.IsNullOrWhiteSpace(item.instanceId)||string.IsNullOrWhiteSpace(item.masterId)||item.amount<=0)
                return Fail("INVENTORY_ITEM_INVALID");
            if(source.items.Any(x=>x.instanceId==item.instanceId))
                return Fail("INVENTORY_INSTANCE_DUPLICATE");
            if(source.items.Count>=source.capacity)
                return Fail("INVENTORY_CAPACITY");

            var next=Clone(source);
            next.items.Add(Clone(item));
            return new InventoryResult{ok=true,next=next};
        }

        public static InventoryResult Remove(InventoryState source,string instanceId)
        {
            if(source==null)throw new ArgumentNullException(nameof(source));
            if(!source.items.Any(x=>x.instanceId==instanceId))return Fail("INVENTORY_NOT_FOUND");
            var next=Clone(source);next.items.RemoveAll(x=>x.instanceId==instanceId);
            return new InventoryResult{ok=true,next=next};
        }

        private static InventoryState Clone(InventoryState s)=>new InventoryState{
            capacity=s.capacity,items=s.items.Select(Clone).ToList()};
        private static InventoryItem Clone(InventoryItem x)=>new InventoryItem{
            instanceId=x.instanceId,masterId=x.masterId,kind=x.kind,amount=x.amount};
        private static InventoryResult Fail(string r)=>new InventoryResult{ok=false,reason=r};
    }
}
