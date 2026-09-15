using System;

namespace GuildAdventure.Game.Equipment
{
    [Serializable] public sealed class EquipmentExportFile { public string schema_version,data_version; public EquipmentExportRow[] data; }
    [Serializable] public sealed class EquipmentExportRow {
        public string id,name,status,description;
        public string[] tags,mod_ids;
        public int item_level;
        public double required_str,required_dex,required_int,required_vit,required_mnd,required_agi;
        public double attack,accuracy,magic_accuracy,magic_weapon_bonus,weapon_critical_rate;
        public double hp_bonus,mp_bonus,evasion,magic_resistance,block_rate,block_damage_cut_rate;
        public EquipmentGenerationMeta generation;
    }
    [Serializable] public sealed class EquipmentGenerationMeta {
        public string generator_version,generation_rules_version,config_id,config_version,source_spec_version,seed,base_item_type,armor_slot;
    }
}
