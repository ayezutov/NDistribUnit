namespace NDistribUnit.Common.TestExecution.DistributedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public class DistributedConfigurationVariablesValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedConfigurationVariablesValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public DistributedConfigurationVariablesValue(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stringified value of a variable (as it will be inserted in the target config).
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}