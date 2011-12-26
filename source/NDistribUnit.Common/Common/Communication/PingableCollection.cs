using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.ServiceContracts;
using System.Linq;

namespace NDistribUnit.Common.Common.Communication
{
	/// <summary>
	/// A collection of IPingable. Each element is pinged with the given interval
	/// and is removed from collection, when the ping fails
	/// </summary>
	/// <typeparam name="TPingable">The type of the pingable.</typeparam>
	public class PingableCollection<TPingable>: ICollection<TPingable> where TPingable: IPingable
	{
		private readonly IConnectionsHostOptions options;
	    private readonly ILog log;
	    readonly SynchronizedCollection<PingableMetadata> metadatas = new SynchronizedCollection<PingableMetadata>();

	    /// <summary>
	    /// Occurs when an item is removed.
	    /// </summary>
	    public event EventHandler<EventArgs<TPingable>> Removed;

	    /// <summary>
	    /// Occurs when an item is added.
	    /// </summary>
	    public event EventHandler<EventArgs<TPingable>> Added;

	    /// <summary>
	    /// Occurs when an item is pinged successfully.
	    /// </summary>
	    public event EventHandler<EventArgs<Tuple<TPingable, PingResult>>> SuccessfullyPinged;

	    private class PingableMetadata
		{
			public TPingable Item { get; set; }
			public Timer Timer { get; set; }

			private bool Equals(PingableMetadata other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Equals(other.Item, Item);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != typeof (PingableMetadata)) return false;
				return Equals((PingableMetadata) obj);
			}

			public override int GetHashCode()
			{
				return Item.GetHashCode();
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="PingableCollection&lt;TPingable&gt;"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
		public PingableCollection(IConnectionsHostOptions options, ILog log)
        {
            this.options = options;
            this.log = log;
        }

	    /// <summary>
		/// Adds the specified item. After adding pinging is started.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(TPingable item)
		{
            lock (metadatas.SyncRoot)
            {
                var existing = metadatas.FirstOrDefault(m => m.Item.Equals(item));
                if (existing != null)
                    return;
            }
			var metadata = new PingableMetadata { Item = item };
			metadata.Timer = new Timer(OnItemPing, metadata, 0, Timeout.Infinite);
			metadatas.Add(metadata);
            Added.SafeInvoke(this, item);
		}

	    /// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		public void Clear()
		{
			lock (metadatas.SyncRoot)
			{
				for (int i = Count - 1; i >= 0; i--)
				{
					Remove(metadatas[i]);
				}
			}
		}

	    /// <summary>
	    /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
	    /// </summary>
	    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
	    /// <returns>
	    /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
	    /// </returns>
	    public bool Contains(TPingable item)
	    {
	        return metadatas.Contains(new PingableMetadata {Item = item});
	    }

	    /// <summary>
	    /// Copies to.
	    /// </summary>
	    /// <param name="array">The array.</param>
	    /// <param name="arrayIndex">Index of the array.</param>
	    public void CopyTo(TPingable[] array, int arrayIndex)
	    {
	        lock (metadatas.SyncRoot)
	        {
	            for (int i = arrayIndex; i < array.Length; i++)
	            {
	                array[i] = metadatas[i - arrayIndex].Item;
	            }
	        }
	    }

	    /// <summary>
	    /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    /// </summary>
	    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
	    /// <returns>
	    /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    /// </returns>
	    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
	    public bool Remove(TPingable item)
	    {
	        lock (metadatas.SyncRoot)
	        {
	            var metadata = metadatas.FirstOrDefault(m => m.Item.Equals(item));
	            if (metadata == null)
	                return false;
	            Remove(metadata);
	            return true;
	        }
	    }

	    /// <summary>
	    /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    /// </summary>
	    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
	    public int Count
	    {
	        get { return metadatas.Count; }
	    }

	    /// <summary>
	    /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
	    /// </summary>
	    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
	    public bool IsReadOnly
	    {
	        get { return false; }
	    }

	    /// <summary>
	    /// Returns an enumerator that iterates through the collection.
	    /// </summary>
	    /// <returns>
	    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
	    /// </returns>
	    public IEnumerator<TPingable> GetEnumerator()
	    {
	        return metadatas.Select(m => m.Item).GetEnumerator();
	    }

	    /// <summary>
	    /// Returns an enumerator that iterates through a collection.
	    /// </summary>
	    /// <returns>
	    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
	    /// </returns>
	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }

	    private void OnItemPing(object state)
	    {
	        var metadata = state as PingableMetadata;
	        Debug.Assert(metadata != null);
	        try
	        {
	            if (metadata.Timer != null)
	                metadata.Timer.Change(Timeout.Infinite, Timeout.Infinite);
	        }
	        catch (ObjectDisposedException)
	        {
	            return;
	        }
	        try
	        {
	            var result = metadata.Item.Ping(TimeSpan.FromMilliseconds(options.PingIntervalInMiliseconds));
	            if (metadata.Timer != null)
	                metadata.Timer.Change(options.PingIntervalInMiliseconds, Timeout.Infinite);

	            SuccessfullyPinged.SafeInvoke(this, new Tuple<TPingable, PingResult>(metadata.Item, result));
	        }
	        catch (Exception ex)
	        {
                log.Error("Error while pinging", ex);
	            Remove(metadata);
	        }
	    }

	    /// <summary>
		/// Removes the specified metadata.
		/// </summary>
		/// <param name="metadata">The metadata.</param>
		private void Remove(PingableMetadata metadata)
		{
			lock (metadatas.SyncRoot)
			{
				try
				{
					if (metadata.Timer != null)
						metadata.Timer.Change(Timeout.Infinite, Timeout.Infinite);
				}
				catch (ObjectDisposedException)
				{
					return;
				}

				metadata.Timer = null;

				metadatas.Remove(metadata);
			}
			Removed.SafeInvoke(this, metadata.Item);
		}
	}
}