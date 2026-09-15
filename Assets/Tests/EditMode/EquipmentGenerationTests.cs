#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Equipment;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class EquipmentGenerationTests
    {
        [Test]
        public void OneHandSword_Ilv1_MatchesStudioExportTrace()
        {
            var r=EquipmentGeneration.GenerateWeapon("片手剣",1,EquipmentGeneration.CurrentConfirmedConfig());
            Assert.AreEqual(6,r.requiredStr); Assert.AreEqual(3,r.requiredDex); Assert.AreEqual(1,r.requiredInt);
            Assert.AreEqual(12,r.attack); Assert.AreEqual(6,r.accuracy); Assert.AreEqual(8,r.magicAccuracy);
            Assert.AreEqual(6,r.magicWeaponBonus); Assert.AreEqual(0.05,r.weaponCriticalRate,0.000001);
        }

        [Test]
        public void Shield_Block_IsDataDrivenByItemLevel()
        {
            var r=EquipmentGeneration.GenerateWeapon("盾",3,EquipmentGeneration.CurrentConfirmedConfig());
            Assert.AreEqual(0.24,r.blockRate.Value,0.000001);
            Assert.AreEqual(0.30,r.blockDamageCutRate.Value,0.000001);
        }

        [Test]
        public void HeavyArmorBody_UsesConfirmedRequirementAndSlotCoefficients()
        {
            var r=EquipmentGeneration.GenerateArmor("重装","鎧",2,EquipmentGeneration.CurrentConfirmedConfig());
            Assert.AreEqual(12,r.requiredVit); Assert.AreEqual(6,r.requiredMnd); Assert.AreEqual(2,r.requiredAgi);
            Assert.AreEqual(240,r.hpBonus); Assert.AreEqual(120,r.mpBonus); Assert.AreEqual(2,r.evasion);
        }

        [Test]
        public void NormalRoute_BlocksUniqueAndAboveSpecialRarities()
        {
            Assert.IsFalse(ModRarityContracts.IsNormalRouteBlocked(EquipmentRarity.RARE));
            Assert.IsTrue(ModRarityContracts.IsNormalRouteBlocked(EquipmentRarity.UNIQUE));
            Assert.IsTrue(ModRarityContracts.IsNormalRouteBlocked(EquipmentRarity.LEGENDARY));
            Assert.IsTrue(ModRarityContracts.IsNormalRouteBlocked(EquipmentRarity.MYTHIC));
        }
    }
}
#endif
