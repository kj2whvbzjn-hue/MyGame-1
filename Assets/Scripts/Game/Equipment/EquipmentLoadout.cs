using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Equipment
{
    public enum EquipmentSlot
    {
        WeaponMain,
        WeaponSub,
        Head,
        Body,
        Accessory1,
        Accessory2
    }

    [Serializable]
    public sealed class EquipmentDefinition
    {
        public string id;
        public string category;
        public string requiredJobId;
        public bool twoHanded;
    }

    [Serializable]
    public sealed class EquippedItem
    {
        public string instanceId;
        public string definitionId;
        public EquipmentSlot slot;
    }

    public sealed class EquipResult
    {
        public bool ok;
        public string reason;
        public List<EquippedItem> next;
    }

    public static class EquipmentLoadout
    {
        public static EquipResult Equip(
            IReadOnlyCollection<EquippedItem> current,
            string instanceId,
            EquipmentDefinition definition,
            EquipmentSlot targetSlot,
            string characterJobId)
        {
            if (current == null || definition == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(instanceId) || string.IsNullOrWhiteSpace(definition.id))
                return Fail("equipment_invalid");
            if (!string.IsNullOrWhiteSpace(definition.requiredJobId) &&
                definition.requiredJobId != characterJobId)
                return Fail("equipment_job_requirement");

            if (!SlotAccepts(targetSlot, definition.category))
                return Fail("equipment_slot_mismatch");

            var next=current.Select(x=>new EquippedItem{
                instanceId=x.instanceId,definitionId=x.definitionId,slot=x.slot}).ToList();

            // An equipment instance may exist in only one slot.
            next.RemoveAll(x=>x.instanceId==instanceId);

            // Target slot replacement is explicit and deterministic.
            next.RemoveAll(x=>x.slot==targetSlot);

            if (definition.twoHanded && targetSlot != EquipmentSlot.WeaponMain)
                return Fail("two_handed_requires_main");

            if (definition.twoHanded)
                next.RemoveAll(x=>x.slot==EquipmentSlot.WeaponSub);

            if (targetSlot==EquipmentSlot.WeaponSub &&
                next.Any(x=>x.slot==EquipmentSlot.WeaponMain &&
                            x.definitionId=="__TWO_HANDED_SENTINEL__"))
                return Fail("offhand_blocked_by_two_handed");

            next.Add(new EquippedItem{
                instanceId=instanceId,
                definitionId=definition.twoHanded ? "__TWO_HANDED_SENTINEL__" : definition.id,
                slot=targetSlot
            });
            return new EquipResult{ok=true,next=next};
        }

        public static bool SlotAccepts(EquipmentSlot slot,string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return false;
            switch(slot)
            {
                case EquipmentSlot.WeaponMain:
                case EquipmentSlot.WeaponSub: return category=="weapon";
                case EquipmentSlot.Head: return category=="head";
                case EquipmentSlot.Body: return category=="body";
                case EquipmentSlot.Accessory1:
                case EquipmentSlot.Accessory2: return category=="accessory";
                default: return false;
            }
        }

        private static EquipResult Fail(string reason)
            => new EquipResult{ok=false,reason=reason};
    }
}
