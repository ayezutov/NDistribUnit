using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.TestExecution.Configuration;

namespace NDistribUnit.Common.Contracts.DataContracts
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class TestRun
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRun"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
	    public TestRun(Guid id)
	    {
	        Id = id;
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="TestRun"/> class.
        /// </summary>
	    public TestRun(): this(Guid.NewGuid())
	    {}

	    /// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>
		/// The id.
		/// </value>
		public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
	    public TestRunParameters Parameters { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
	    public NUnitParameters NUnitParameters { get; set; }

	    /// <summary>
	    /// Gets the alias.
	    /// </summary>
        public string Alias { get; set; }

	    /// <summary>
		/// Equalses the specified other.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals(TestRun other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Id.Equals(Id);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (TestRun)) return false;
			return Equals((TestRun) obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
	    public override string ToString()
	    {
	        return string.Format("{0}{1}", Id, string.IsNullOrEmpty(Alias) ? string.Format(", Alias: {0}", Alias) : string.Empty);
	    }
	}
}