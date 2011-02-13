using System.Collections.Generic;
using NDistribUnit.Common.Options;

namespace NDistribUnit.Client.Configuration
{
    public class ClientParametersParser
    {
        public ClientParameters Parse(IEnumerable<string> strings)
        {
            var parameters = new ClientParameters();

            var set = new ConsoleParametersParser
                {
// Disabling is required as far as inspection warning is incorrect and compilation doesn't succeed, if fixed
// ReSharper disable RedundantLambdaParameterType
                    {"xml", (string xmlFileName) => parameters.XmlFileName = xmlFileName, false },
                    {"noshadow", (bool noShadow) => parameters.NoShadow = noShadow, true },
                    {ConsoleOption.UnnamedOptionName, (string assembly) => parameters.AssembliesToTest.Add(assembly), false}
// ReSharper restore RedundantLambdaParameterType
                };

            parameters.UnknownOption.AddRange(set.Parse(strings));
            return parameters;
        }
    }
}