using System;

namespace NDistribUnit.Common
{
    public class Callbacks : ICallbacks
    {
        public bool MyCallbackFunction(string callbackValue)
        {
            Console.WriteLine("Callback Received: {0}", callbackValue);
            return true;
        }
    }
}