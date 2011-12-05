using System.IO;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Util;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDistributedConfigurationOperator
    {
        /// <summary>
        /// Reads the configuration setup.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="nUnitParameters">The n unit parameters.</param>
        /// <returns></returns>
        DistributedConfigurationSetup ReadConfigurationSetup(TestProject project, NUnitParameters nUnitParameters);

        /// <summary>
        /// Gets the substituted configuration file.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="nUnitParameters">The n unit parameters.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <returns></returns>
        string GetSubstitutedConfigurationFile(TestProject project, NUnitParameters nUnitParameters, DistributedConfigurationSubstitutions configurationSubstitutions);
    }
}
