using EpochGame.Population;
using EpochGame.Progress;
using NUnit.Framework;

namespace EpochGame.Tests
{
    public sealed class PlayerLevelServiceTests
    {
        private readonly PlayerLevelRequirement[] _requirements =
        {
            new PlayerLevelRequirement(2, 15, 10)
        };

        [Test]
        public void PartialProgressUsesTheNextLevelRequirement()
        {
            var progress = new PlayerLevelService(_requirements).Evaluate(1, 3, 1);

            Assert.AreEqual(1, progress.Level);
            Assert.AreEqual(3f / 15f, progress.PopulationFill, 0.0001f);
            Assert.AreEqual(1f / 10f, progress.TerritoryFill, 0.0001f);
        }

        [Test]
        public void ProgressStopsAtFullWhenTheRequirementIsMet()
        {
            var progress = new PlayerLevelService(_requirements).Evaluate(1, 40, 10);

            Assert.AreEqual(1f, progress.PopulationFill, 0.0001f);
            Assert.AreEqual(1f, progress.TerritoryFill, 0.0001f);
        }

        [Test]
        public void ZeroProgressStaysEmpty()
        {
            var progress = new PlayerLevelService(_requirements).Evaluate(1, 0, 0);

            Assert.AreEqual(0f, progress.PopulationFill, 0.0001f);
            Assert.AreEqual(0f, progress.TerritoryFill, 0.0001f);
        }

        [Test]
        public void MissingNextLevelLeavesTheBarsFull()
        {
            var progress = new PlayerLevelService(_requirements).Evaluate(2, 0, 0);

            Assert.AreEqual(2, progress.Level);
            Assert.AreEqual(1f, progress.PopulationFill, 0.0001f);
            Assert.AreEqual(1f, progress.TerritoryFill, 0.0001f);
        }
    }

    public sealed class PopulationAndTerritoryTests
    {
        [Test]
        public void AddClampsAtZero()
        {
            var population = new PopulationState(3);
            population.Add(-10);
            Assert.AreEqual(0, population.CurrentPopulation);

            var territory = new TerritoryState(1);
            territory.Add(4);
            territory.Add(-2);
            Assert.AreEqual(3, territory.Area);
        }
    }
}
