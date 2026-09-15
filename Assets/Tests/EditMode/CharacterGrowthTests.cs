#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Character;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class CharacterGrowthTests
    {
        [TestCase(15, 0.49d, 2)]
        [TestCase(15, 0.50d, 1)]
        [TestCase(10, 0.99d, 1)]
        [TestCase(3, 0.29d, 1)]
        [TestCase(3, 0.30d, 0)]
        [TestCase(0, 0.00d, 0)]
        [TestCase(20, 0.99d, 2)]
        public void CalculateIncrement_MatchesGs02(int growth, double roll, int expected)
        {
            Assert.AreEqual(expected, CharacterGrowth.CalculateIncrement(growth, roll));
        }

        [Test]
        public void ExpectedStat_KeepsFraction()
        {
            Assert.AreEqual(23.5d, CharacterGrowth.CalculateExpectedStat(10, 15), 0.000001d);
        }

        [Test]
        public void Level50_DoesNotGrow()
        {
            var stats = Seven(10);
            var growth = Seven(15);
            var rng = new SequenceRandomSource(new [] { 0.1d });
            var result = CharacterGrowth.ResolveLevelUp(50, 50, "swordsman", stats, growth, rng);
            Assert.IsFalse(result.grew);
            Assert.AreEqual("max_level", result.reason);
            Assert.AreEqual(50, result.toLevel);
            Assert.AreEqual(0, result.stats.Count);
        }

        [Test]
        public void ResolveLevelUp_RecordsAllSevenRollsAndResults()
        {
            var result = CharacterGrowth.ResolveLevelUp(
                1, 50, "swordsman", Seven(10), Seven(15),
                new SequenceRandomSource(new [] { 0.49d,0.50d,0.49d,0.50d,0.49d,0.50d,0.49d }));

            Assert.IsTrue(result.grew);
            Assert.AreEqual(2, result.toLevel);
            Assert.AreEqual(7, result.stats.Count);
            Assert.AreEqual(2, result.stats[0].finalIncrease);
            Assert.AreEqual(1, result.stats[1].finalIncrease);
        }

        private static Dictionary<CharacterStat,int> Seven(int value)
        {
            var d = new Dictionary<CharacterStat,int>();
            foreach (CharacterStat s in System.Enum.GetValues(typeof(CharacterStat))) d[s] = value;
            return d;
        }
    }
}
#endif
