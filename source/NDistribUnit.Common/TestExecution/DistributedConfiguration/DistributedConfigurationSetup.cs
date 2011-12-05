using System.Collections.Generic;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// Hold all the distributed configuration in one place
    /// </summary>
    public class DistributedConfigurationSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedConfigurationSetup"/> class.
        /// </summary>
        public DistributedConfigurationSetup()
        {
            Variables = new List<DistributedConfigurationVariable>();
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        public IList<DistributedConfigurationVariable> Variables { get; private set; }
    }
}