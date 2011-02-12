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
                    {"run", (string testName) => parameters.TestToRun = testName, false},
                    {"xml", (string xmlFileName) => parameters.XmlFileName = xmlFileName, false },
                    {"noshadow", (bool noShadow) => parameters.NoShadow = noShadow, true },
                    {ConsoleParametersParser.UnnamedOptionName, (string assembly) => parameters.AssembliesToTest.Add(assembly), false}
                };

            set.Parse(strings);

            return parameters;
        }
    }
}