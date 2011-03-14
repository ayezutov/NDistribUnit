using System;
using NDistribUnit.Common.Collections;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.Collections
{
    [TestFixture]
    public class RollingListTests
    {
        [Test]
        public void ListIsSuccessfullyCreated()
        {
            Assert.That(() => new RollingList<int>(5), Throws.Nothing);
        }

        [Test]
        public void ListIsNotCreatedWhenZeroIsPassedAsMaximumItemsCount()
        {
            Assert.That(() => new RollingList<int>(0), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void ListIsNotCreatedWhenNegativeNumberIsPassedAsMaximumItemsCount()
        {
            Assert.That(() => new RollingList<int>(-284), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void SingleItemIsAddedToList()
        {
            var list = new RollingList<int>(5) {1};

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(1));
        }

        [Test]
        public void TwoItemsAreAddedToList()
        {
            var list = new RollingList<int>(5) {2, 1};

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(2));
            Assert.That(list[1], Is.EqualTo(1));
        }

        [Test]
        public void MultipleItemsAreAddedToList()
        {
            var list = new RollingList<int>(5) {3, 2, 1, 0};

            Assert.That(list.Count, Is.EqualTo(4));
            Assert.That(list, Is.EquivalentTo(new[]{3,2,1,0}));
        }

        [Test]
        public void TenItemsAreAddedButOnlyLastFiveWillRemain()
        {
            var list = new RollingList<int>(5) {999, 998, 997, 996, 995, 994, 993, 992, 991, 990};

            Assert.That(list.Count, Is.EqualTo(5));
            Assert.That(list, Is.EquivalentTo(new[] { 994, 993, 992, 991, 990 }));
        }
    }
}