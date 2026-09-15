using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Equipment
{
    [Serializable] public sealed class RequirementCoefficients { public double str,dex,intel,vit,mnd,agi; }
    [Serializable] public sealed class WeaponPerformanceConfig {
        public double attackMultiplier=2, accuracyMultiplier=2, weaponCriticalRate=0.05;
        public double blockRateBase=0.2, blockRatePerItemLevel=0.02, blockDamageCutRate=0.3;
    }
    [Serializable] public sealed class EquipmentGenerationConfig {
        public int minItemLevel=1,maxItemLevel=11;
        public Dictionary<string,RequirementCoefficients> weapon=new Dictionary<string,RequirementCoefficients>();
        public Dictionary<string,RequirementCoefficients> armor=new Dictionary<string,RequirementCoefficients>();
        public Dictionary<string,double> armorSlotCoefficient=new Dictionary<string,double>();
        public WeaponPerformanceConfig weaponPerformance=new WeaponPerformanceConfig();
    }
    [Serializable] public sealed class GeneratedWeapon {
        public string baseItemType; public int itemLevel;
        public double requiredStr,requiredDex,requiredInt,attack,accuracy,magicAccuracy,magicWeaponBonus,weaponCriticalRate;
        public double? blockRate,blockDamageCutRate;
    }
    [Serializable] public sealed class GeneratedArmor {
        public string baseItemType,armorSlot; public int itemLevel;
        public double requiredVit,requiredMnd,requiredAgi,hpBonus,mpBonus,evasion,magicResistance;
    }

    public static class EquipmentGeneration
    {
        public static GeneratedWeapon GenerateWeapon(string type,int itemLevel,EquipmentGenerationConfig cfg)
        {
            ValidateLevel(itemLevel,cfg);
            if(!cfg.weapon.TryGetValue(type,out var c)) throw new ArgumentException("WEAPON_TYPE_UNKNOWN");
            var p=cfg.weaponPerformance;
            var r=new GeneratedWeapon{
                baseItemType=type,itemLevel=itemLevel,
                requiredStr=itemLevel*c.str,requiredDex=itemLevel*c.dex,requiredInt=itemLevel*c.intel,
            };
            r.attack=r.requiredStr*p.attackMultiplier;
            r.accuracy=r.requiredDex*p.accuracyMultiplier;
            r.magicAccuracy=(r.requiredInt+r.requiredDex)*2;
            r.magicWeaponBonus=r.requiredInt*c.str;
            r.weaponCriticalRate=p.weaponCriticalRate;
            if(type=="盾"){
                r.blockRate=p.blockRateBase+p.blockRatePerItemLevel*(itemLevel-1);
                r.blockDamageCutRate=p.blockDamageCutRate;
            }
            return r;
        }

        public static GeneratedArmor GenerateArmor(string type,string slot,int itemLevel,EquipmentGenerationConfig cfg)
        {
            ValidateLevel(itemLevel,cfg);
            if(!cfg.armor.TryGetValue(type,out var c)) throw new ArgumentException("ARMOR_TYPE_UNKNOWN");
            if(!cfg.armorSlotCoefficient.TryGetValue(slot,out var slotC)) throw new ArgumentException("ARMOR_SLOT_UNKNOWN");
            var r=new GeneratedArmor{
                baseItemType=type,armorSlot=slot,itemLevel=itemLevel,
                requiredVit=itemLevel*c.vit,requiredMnd=itemLevel*c.mnd,requiredAgi=itemLevel*c.agi
            };
            // Current active config: slot coefficient affects HP/MP only; evasion is not slot-multiplied.
            r.hpBonus=r.requiredVit*slotC;
            r.mpBonus=r.requiredMnd*slotC;
            r.evasion=r.requiredAgi;
            // Generation rules require this field, but the inspected active balance config does not define its numeric formula.
            r.magicResistance=0;
            return r;
        }

        private static void ValidateLevel(int i,EquipmentGenerationConfig cfg)
        {
            if(cfg==null) throw new ArgumentNullException(nameof(cfg));
            if(i<cfg.minItemLevel||i>cfg.maxItemLevel) throw new ArgumentOutOfRangeException(nameof(i),"ITEM_LEVEL_OUT_OF_RANGE");
        }

        public static EquipmentGenerationConfig CurrentConfirmedConfig()
        {
            var c=new EquipmentGenerationConfig();
            void W(string n,double s,double d,double i)=>c.weapon[n]=new RequirementCoefficients{str=s,dex=d,intel=i};
            W("片手剣",6,3,1);W("大剣",12,3,1);W("短剣",3,6,1);W("片手斧",6,3,1);W("大斧",12,3,1);
            W("槍",6,3,1);W("弓",3,6,1);W("大弓",6,3,1);W("杖",3,1,6);W("ワンド",1,3,6);
            W("魔導書",1,3,6);W("盾",6,3,1);W("矢筒",3,6,1);
            c.armor["重装"]=new RequirementCoefficients{vit=6,mnd=3,agi=1};
            c.armor["軽装"]=new RequirementCoefficients{vit=3,mnd=1,agi=6};
            c.armor["ローブ"]=new RequirementCoefficients{vit=1,mnd=6,agi=3};
            c.armorSlotCoefficient["鎧"]=20;c.armorSlotCoefficient["頭"]=10;c.armorSlotCoefficient["手"]=10;c.armorSlotCoefficient["足"]=10;
            return c;
        }
    }
}
