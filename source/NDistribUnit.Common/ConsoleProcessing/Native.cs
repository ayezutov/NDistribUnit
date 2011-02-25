using System.Runtime.InteropServices;

namespace NDistribUnit.Common.ConsoleProcessing
{
    public static class Native
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(ConsoleExitHandler handler, bool add);
    }
}