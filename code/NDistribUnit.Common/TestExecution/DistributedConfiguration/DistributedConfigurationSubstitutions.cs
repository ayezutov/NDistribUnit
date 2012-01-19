using System;
using System.Collections.Generic;
using System.Text;

namespace NDistribUnit.Common.TestExecution.DistributedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class DistributedConfigurationSubstitutions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedConfigurationSubstitutions"/> class.
        /// </summary>
        public DistributedConfigurationSubstitutions()
        {
            Variables = new List<DistributedConfigurationVariablesValue>();
        }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IList<DistributedConfigurationVariablesValue> Variables { get; set; }
        
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var result = 17;
            foreach (var variable in Variables)
            {
                if (variable.Name != null)
                    result = result*23 + variable.Name.GetHashCode();

                if (variable.Value != null)
                    result = result*13 + variable.Value.GetHashCode();
            }
            return result;
        }
    }
}