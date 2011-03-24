using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Collections
{
    /// <summary>
    /// Implements a list with a predefined maximum number of elements in it.
    /// Once the limit is reached, the earliest items are deleted
    /// </summary>
    public class RollingList<TValue>: IEnumerable<TValue>
    {
        private RollingListItem<TValue> First { get; set; }
        private RollingListItem<TValue> Last { get; set; }

        /// <summary>
        /// Creates a new instance of rolling list
        /// </summary>
        /// <param name="maximumCount"></param>
        public RollingList(int maximumCount)
        {
            if (!(maximumCount > 0))
                throw new ArgumentException("The maximum count should be greater, than zero.");

            MaximumCount = maximumCount;
        }

        /// <summary>
        /// Gets the maximum number of elements, which can be stored in the list
        /// </summary>
        public int MaximumCount { get; private set; }

        /// <summary>
        /// Gets the count of elements, which were added to the list
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(TValue item)
        {
            var listItem = new RollingListItem<TValue> {Value = item};

            lock (this)
            {
                if (Count >= 1)
                {
                    if (Count == MaximumCount)
                    {
                        var oldFirst = First;
                        First = First.Next;
                        oldFirst.Next = null;
                        oldFirst.Previous = null;
                        First.Previous = Last;
                        Last.Next = First;
                        Count--;
                    }
                    listItem.Next = First;
                    listItem.Previous = Last;
                    listItem.Next.Previous = listItem;
                    listItem.Previous.Next = listItem;
                    Last = listItem;
                }
                else
                {
                    Last = First = listItem;
                    listItem.Next = listItem;
                    listItem.Previous = listItem;
                }

                Count++;
            }
        }

        /// <summary>
        /// Gets the value at the specified index. 
        /// </summary>
        /// <param name="index">The index of the item, starting from current beginning</param>
        /// <returns></returns>
        public TValue this[int index]
        {
            get 
            {
                lock (this)
                {
                    if (index >= Count)
                        throw new IndexOutOfRangeException(
                            string.Format(
                                "The index, you are trying to access ({0}) is greater, than the number of items in collection ({1})",
                                index, Count));

                    var i = 0;
                    var current = First;
                    while (index > i)
                    {
                        current = current.Next;
                        i++;
                    }

                    return current.Value;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TValue> GetEnumerator()
        {
            RollingListItem<TValue> item = null;

            do
            {
                item = item == null ? First : item.Next;

                if (item == null)
                    yield break;

                yield return item.Value;

            } while (item != Last);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the earliest items from list.
        /// </summary>
        /// <param name="maxReturnCount">The maximum count of items to be returned</param>
        /// <returns></returns>
        public IEnumerable<TValue> GetHead(int maxReturnCount)
        {
            return GetItemsStartingFrom(First, maxReturnCount);
        }

        /// <summary>
        /// Gets the items, starting with <paramref name="item"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="maxReturnCount">The max return count.</param>
        /// <returns></returns>
        public IEnumerable<TValue> GetItemsAfter(RollingListItem<TValue> item, int maxReturnCount)
        {
            if (item == Last)
                return new TValue[0];
            return GetItemsStartingFrom(item.Next, maxReturnCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public RollingListItem<TValue> FindFirst(Func<TValue , bool> condition)
        {
            lock (this)
            {

                RollingListItem<TValue> current = null;

                do
                {
                    current = current == null ? First : current.Next;

                    if (condition(current.Value))
                        return current;

                } while (current != Last);

                return null;
            }
        }

        /// <summary>
        /// Returns items, starting from given. It will return maximum <paramref name="maxReturnCount"/> items
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="maxReturnCount">The max return count.</param>
        /// <returns></returns>
        private IEnumerable<TValue> GetItemsStartingFrom(RollingListItem<TValue> item, int maxReturnCount)
        {
            lock (this)
            {
                if (Count == 0)
                    yield break;

                RollingListItem<TValue> current = null;
                var count = 0;
                do
                {
                    current = current == null ? (item??First) : current.Next;

                    yield return current.Value;
                } while (current != Last && ++count < maxReturnCount);
                yield break;
            }
        }
    }
}