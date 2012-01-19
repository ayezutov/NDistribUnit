namespace NDistribUnit.Common.TestExecution.DistributedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DistributedConfigurationVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedConfigurationVariable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected DistributedConfigurationVariable(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the next value.
        /// </summary>
        /// <returns></returns>
        public abstract string GetNextValue();
    }
}