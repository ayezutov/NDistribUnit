using System.Collections.Generic;
using NDistribUnit.Client.Configuration;

namespace NDistribUnit.Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            ClientParameters parameters = new ClientParametersParser().Parse(args);

        }
    }
}
