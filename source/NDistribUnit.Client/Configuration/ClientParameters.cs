using System;
using System.Collections.Generic;

namespace NDistribUnit.Client.Configuration
{
    public class ClientParameters
    {
        public ClientParameters()
        {
            AssembliesToTest = new List<string>();
            NoShadow = false;
        }

        public IList<string> AssembliesToTest { get; set; }

        public string TestToRun { get; set; }

        public string XmlFileName { get; set; }

        public bool NoShadow { get; set; }
    }
}