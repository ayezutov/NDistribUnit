using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NDistribUnit.Common.Common.PInvoke;
using NDistribUnit.Common.Common.Extensions;

namespace NDistribUnit.Common.Agent.Naming
{
    /// <summary>
    /// A name provider, which provides a prefix with the first free number for the current asssembly on this machine.
    /// </summary>
    public class InstanceTracker: IInstanceTracker
    {
        private static InstanceTrackerState state = new InstanceTrackerState();

// ReSharper disable NotAccessedField.Local
        private static Mutex instanceNumberMutex; // this reference should live till the end of the program. Mutex will be released on process termination.
        private static readonly Mutex machineLockMutex;
        private static Mutex instanceCountTrackingSemaphore;

// ReSharper restore NotAccessedField.Local


        static InstanceTracker()
        {
            var syncPrefix = GetSyncPrefix();
            machineLockMutex = new Mutex(false, syncPrefix+"::machine-mutex");
            Win32.SetHandleInformation(machineLockMutex.SafeWaitHandle.DangerousGetHandle(),
                                       Win32.SetInformationHandleFlags.HANDLE_FLAG_INHERIT, 0);
        }

       private static string GetSyncPrefix()
        {
            string mutexPrefix = "NDistribUnit.Agent";

            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                // in NUnit tests
                string directoryName = Path.GetDirectoryName(assembly.CodeBase);

                if (directoryName != null)
                    mutexPrefix = directoryName.Replace('\\', '_');
            }
            return mutexPrefix;
        }

       /// <summary>
       /// Gets the instance number.
       /// </summary>
       /// <returns></returns>
        public int GetInstanceNumber()
        {
            if (state.Number > 0)
                return state.Number;
            
            lock (this)
            {
                if (state.Number > 0)
                    return state.Number;

                return state.Number = ReserveInstanceNumber();
            }
        }

        /// <summary>
        /// Initializes this instance and executes firstInstanceActions.
        /// </summary>
        /// <param name="firstInstanceActions"></param>
        public void Init(Action<IInstanceTracker> firstInstanceActions)
        {
            lock (this)
            {
                if (state.IsInitialized)
                    return;

                LockMachineWide();

                if (state.Number == 0)
                    state.Number = ReserveInstanceNumber();

                try
                {
                    bool created;
                    instanceCountTrackingSemaphore = new Mutex(false, GetSyncPrefix() + "::first-time-run-mutex", out created);
                    Win32.SetHandleInformation(machineLockMutex.SafeWaitHandle.DangerousGetHandle(),
                                               Win32.SetInformationHandleFlags.HANDLE_FLAG_INHERIT, 0);

                    if (created) 
                        firstInstanceActions(this);

                }
                finally
                {
                    UnlockMachineWide();
                }

                state.IsInitialized = true;
            }
        }

        private void LockMachineWide()
        {
            machineLockMutex.WaitOne();
        }

        private void UnlockMachineWide()
        {
            machineLockMutex.ReleaseMutex();
        }
        
        private int ReserveInstanceNumber()
        {
            var result = 0;

            var mutexPrefix = GetSyncPrefix();

            while (true)
            {
                string name = string.Format("{0}::{1}", mutexPrefix, ++result);

                bool wasCreated;
                instanceNumberMutex = new Mutex(true, name, out wasCreated);

                if (wasCreated)
                    break;
            }

            Win32.SetHandleInformation(instanceNumberMutex.SafeWaitHandle.DangerousGetHandle(),
                                       Win32.SetInformationHandleFlags.HANDLE_FLAG_INHERIT, 0);

            return result;
        }

        /// <summary>
        /// Inits to state.
        /// </summary>
        /// <param name="instanceTrackerState">State of the instance tracker.</param>
        public static void InitToState(InstanceTrackerState instanceTrackerState)
        {
            state = instanceTrackerState;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <returns></returns>
        public static InstanceTrackerState GetState()
        {
            return state.DeepClone();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IInstanceTracker
    {
        /// <summary>
        /// Gets the instance number.
        /// </summary>
        /// <returns></returns>
        int GetInstanceNumber();

        /// <summary>
        /// Initializes this instance and executes firstInstanceActions.
        /// </summary>
        void Init(Action<IInstanceTracker> firstInstanceActions);
    }
}