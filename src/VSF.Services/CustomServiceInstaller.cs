using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace VSF.Services
{
    /// <summary>
    /// Class is utilized to perform specific command line options and other 
    /// specific service type work.
    /// </summary>
    public class CustomServiceInstaller
    {
        #region Constants

        /// <summary>
        /// The specified service does not exist as an installed service.
        /// </summary>
        private const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

        /// <summary>
        ///  Includes STANDARD_RIGHTS_REQUIRED, in addition to all access rights in this table. 
        /// </summary>
        private const uint SC_MANAGER_ALL_ACCESS = 0xF003;
        /// <summary>
        ///  Required to call the CreateService function to create a service object and add it to the database. 
        /// </summary>
        private const uint SC_MANAGER_CREATE_SERVICE = 0x0002;
        /// <summary>
        ///  Required to connect to the service control manager. 
        /// </summary>
        private const uint SC_MANAGER_CONNECT = 0x0001;
        /// <summary>
        ///  Required to call the EnumServicesStatusEx function to list the services that are in the database. 
        /// </summary>
        private const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        /// <summary>
        ///  Required to call the LockServiceDatabase function to acquire a lock on the database. 
        /// </summary>
        private const uint SC_MANAGER_LOCK = 0x0008;
        /// <summary>
        ///  Required to call the NotifyBootConfigStatus function. 
        /// </summary>
        private const uint SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020;
        /// <summary>
        ///  Required to call the QueryServiceLockStatus function to retrieve the lock status information for the database. 
        /// </summary>
        private const uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;

        private const uint STANDARD_RIGHTS_READ = READ_CONTROL;
        private const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;
        private const uint STANDARD_RIGHTS_EXECUTE = READ_CONTROL;

        private const uint READ_CONTROL = 0x00020000;

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_EXECUTE = 0x20000000;
        private const uint GENERIC_ALL = 0x10000000;

        #endregion

        #region DLLImport

        [DllImport("Advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
                                            int dwDesiredAccess, int dwServiceType, SERVICE_START_TYPE dwStartType,
                                            int dwErrorControl, string lpPathName, string lpLoadOrderGroup,
                                            int lpdwTagId, string lpDependencies, string lpServiceStartName,
                                            string lpPassword);

        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl, ref SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, SERVICE_ACCESS dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenSCManager(string machineName, string databaseName, SCM_ACCESS dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteService(IntPtr hService);

        /// <summary>
        /// Changes the optional configuration parameters of a service
        /// </summary>
        /// <param name="hService">
        /// A handle to the service. This handle is returned by the OpenService or CreateService function and must have the 
        /// SERVICE_CHANGE_CONFIG access right. For more information, see Service Security and Access Rights. 
        /// 
        /// If the service controller handles the SC_ACTION_RESTART action, hService must have the SERVICE_START access right.
        /// </param>
        /// <param name="dwInfoLevel">
        /// The configuration information to be changed
        /// SERVICE_CONFIG_DESCRIPTION (1)
        /// The lpInfo parameter is a pointer to a SERVICE_DESCRIPTION structure. 
        /// </param>
        /// <param name="lpInfo">
        /// A pointer to the new value to be set for the configuration information. The format of this data depends on the 
        /// value of the dwInfoLevel parameter. If this value is NULL, the information remains unchanged.
        /// </param>
        /// <returns></returns>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2A(IntPtr hService, InfoLevel dwInfoLevel, ref SERVICE_DESCRIPTION lpInfo);

        #endregion DLLImport

        #region Structures and Enums

        [Flags]
        public enum SERVICE_TYPES : int
        {
            SERVICE_KERNEL_DRIVER = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
            SERVICE_ADAPTER = 0x00000004,
            SERVICE_RECOGNIZER_DRIVER = 0x00000008,
            SERVICE_DRIVER = (SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER),
            SERVICE_WIN32_OWN_PROCESS = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_WIN32 = (SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS),
            SERVICE_INTERACTIVE_PROCESS = 0x00000100
        }

        [Flags]
        public enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007
        }

        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001,
            PAUSE = 0x00000002,
            CONTINUE = 0x00000003,
            INTERROGATE = 0x00000004,
            SHUTDOWN = 0x00000005,
            PARAMCHANGE = 0x00000006,
            NETBINDADD = 0x00000007,
            NETBINDREMOVE = 0x00000008,
            NETBINDENABLE = 0x00000009,
            NETBINDDISABLE = 0x0000000A,
            DEVICEEVENT = 0x0000000B,
            HARDWAREPROFILECHANGE = 0x0000000C,
            POWEREVENT = 0x0000000D,
            SESSIONCHANGE = 0x0000000E
        }

        [Flags]
        public enum SERVICE_ACCEPT : uint
        {
            STOP = 0x00000001,
            PAUSE_CONTINUE = 0x00000002,
            SHUTDOWN = 0x00000004,
            PARAMCHANGE = 0x00000008,
            NETBINDCHANGE = 0x00000010,
            HARDWAREPROFILECHANGE = 0x00000020,
            POWEREVENT = 0x00000040,
            SESSIONCHANGE = 0x00000080,
        }

        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL),
            SERVICE_GENERIC_READ = (STANDARD_RIGHTS_READ | SERVICE_QUERY_CONFIG | SERVICE_QUERY_STATUS | SERVICE_INTERROGATE | SERVICE_ENUMERATE_DEPENDENTS),
            SERVICE_GENERIC_WRITE = (STANDARD_RIGHTS_WRITE | SERVICE_CHANGE_CONFIG),
            SERVICE_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_USER_DEFINED_CONTROL)
        }

        [Flags]
        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_GENERIC_READ = (STANDARD_RIGHTS_READ | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS),
            SC_MANAGER_GENERIC_WRITE = (STANDARD_RIGHTS_WRITE | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_MODIFY_BOOT_CONFIG),
            SC_MANAGER_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE | SC_MANAGER_CONNECT | SC_MANAGER_LOCK),
            SC_MANAGER_GENERIC_ALL = (SC_MANAGER_ALL_ACCESS),
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            SC_MANAGER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SC_MANAGER_CONNECT | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_LOCK | SC_MANAGER_QUERY_LOCK_STATUS | SC_MANAGER_MODIFY_BOOT_CONFIG
        }

        /// <summary>
        /// This is the 
        /// </summary>
        public enum SERVICE_START_TYPE : int
        {
            //	Service Start Type
            SERVICE_BOOT_START = 0x00000000,
            SERVICE_SYSTEM_START = 0x00000001,
            SERVICE_AUTO_START = 0x00000002,
            SERVICE_DEMAND_START = 0x00000003,
            SERVICE_DISABLED = 0x00000004
        }

        public enum InfoLevel : int
        {
            SERVICE_CONFIG_DESCRIPTION = 1,
            SERVICE_CONFIG_FAILURE_ACTIONS = 2
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SERVICE_STATUS
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(SERVICE_STATUS));
            public SERVICE_TYPES dwServiceType;
            public SERVICE_STATE dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SERVICE_DESCRIPTION
        {
            public string lpDescription;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="svcPath">The complete path of the service.</param>
        /// <param name="svcName">Name of the service.</param>
        /// <param name="svcDispName">Display name of the service.</param>
        /// <param name="svcDescription">The English description of the service.</param>
        /// <param name="svcStartType">The start type of the service (automatic, manual, etc)</param>
        /// <returns>True if the process went thro successfully. False if there was any error.</returns>
        public bool InstallService(string svcPath, string svcName, string svcDispName, string svcDescription, SERVICE_START_TYPE svcStartType, string dependencies)
        {
            var bSuccess = false;

            #region Constants declaration.

            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            int SERVICE_INTERACTIVE_PROCESS = 0x00000100;
            int SERVICE_ERROR_NORMAL = 0x00000001;

            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;

            int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL);

            #endregion Constants declaration.

            try
            {
                var hServiceCtrlMgr = OpenSCManager(null, null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE);
                var servDesc = new SERVICE_DESCRIPTION()
                    {
                        lpDescription = svcDescription
                    };

                if (hServiceCtrlMgr.ToInt32() != 0)
                {
                    IntPtr hService = CreateService(hServiceCtrlMgr, svcName, svcDispName,
                                                        SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS | SERVICE_INTERACTIVE_PROCESS,
                                                        svcStartType, SERVICE_ERROR_NORMAL,
                                                        svcPath, null, 0, dependencies, null, null);

                    if (hService.ToInt32() == 0)
                    {
                        CloseServiceHandle(hServiceCtrlMgr);
                        throw new Exception("Unable to start the service due to some error.  Is the service already running?");
                    }
                    else
                    {
                        //  Add the description to the service
                        bSuccess = ChangeServiceConfig2A(hService, InfoLevel.SERVICE_CONFIG_DESCRIPTION, ref servDesc); 
                                                        
                        //  Now trying to start the service
                        int nStartSuccess = StartService(hService, 0, null);

                        //  If the value i is zero, then there was an error starting the service.
                        //  NOTE: error may arise if the service is already running or some other problem.
                        if (nStartSuccess != 0)
                        {
                            CloseServiceHandle(hServiceCtrlMgr);
                            bSuccess = true;
                        }
                        else
                            throw new Exception("Unable to start the service due to some error.  Is the service already running?");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return bSuccess;
        }

        /// <summary>
        /// This method uninstalls the service from the service control manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        /// <param name="nTimeout">The amount of time to wait before timing out (and throwing exception)</param>
        /// <param name="nPoll">The amount of time allowed to pass before polling for the status of the service again</param>
        public bool UnInstallService(string svcName, int nTimeout, int nPoll)
        {
            var bSuccess = false;

            try
            {
                var hServiceCtrlMgr = OpenSCManager(null, null, SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                var arEvent = new AutoResetEvent(false);
                var dtTimeout = DateTime.Now.AddMilliseconds(nTimeout);

                if (hServiceCtrlMgr != IntPtr.Zero)
                {
                    // Open the Service
                    var hService = OpenService(hServiceCtrlMgr, svcName, SERVICE_ACCESS.SERVICE_ALL_ACCESS);

                    // Stop the Service
                    if (hService != IntPtr.Zero)
                    {
                        var status = new SERVICE_STATUS();
                        bool QueryStatus = ControlService(hService, SERVICE_CONTROL.INTERROGATE, ref status);
                        Console.WriteLine(Environment.NewLine + "Current status of the service: " + status.dwCurrentState.ToString("f") + Environment.NewLine);
                        if (QueryStatus)
                        {
                            //  If the service is currently running
                            if (status.dwCurrentState.Equals(SERVICE_STATE.SERVICE_RUNNING))
                            {
                                Console.Write("Attempting to stop the service");

                                while ((dtTimeout.CompareTo(DateTime.Now) > 0) & (status.dwCurrentState != SERVICE_STATE.SERVICE_STOPPED))
                                {
                                    //  Stop the service
                                    ControlService(hService, SERVICE_CONTROL.STOP, ref status);
                                    Console.Write(".");

                                    //  Wait for N seconds before polling for the status again.
                                    arEvent.WaitOne(nPoll);

                                    //  Interrogate the service again
                                    ControlService(hService, SERVICE_CONTROL.INTERROGATE, ref status);                                    
                                }

                                //  Current status
                                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Current status of the service: " + 
                                                    status.dwCurrentState.ToString("f") + Environment.NewLine);
                            }
                        }

                        if (status.dwCurrentState.Equals(SERVICE_STATE.SERVICE_STOPPED))
                        {
                            Console.WriteLine("Attempting to remove the service" + Environment.NewLine);

                            //  Delete the Service					
                            if (DeleteService(hService) == false)
                            {
                                var sDeleteFailedMsg = String.Format("Unable to remove the service due to the following error\nDeleteService failed {0}", Marshal.GetLastWin32Error());

                                Console.WriteLine(sDeleteFailedMsg);
                                throw new Exception(sDeleteFailedMsg);

                            }
                        }
                        else
                        {
                            Console.WriteLine("Unable to remove the service because it is still running" + Environment.NewLine);
                            throw new Exception("Unable to remove the service because it is still running");
                        }
                    }

                    // Close the handle to the SCManager
                    CloseServiceHandle(hServiceCtrlMgr);

                    // if you don't close this handle, Services control panel
                    // shows the service as "disabled", and you'll get 1072 errors
                    // trying to reuse this service's name
                    CloseServiceHandle(hService);

                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return bSuccess;
        }

        /// <summary>
        /// This method will query the existence of the service
        /// </summary>
        /// <param name="svcName">The name of the service you are querying for.</param>
        /// <returns>True if the service exists, otherwise false.</returns>
        public bool DoesServiceExists(string svcName)
        {
            //  Declare the return variable
            bool bSuccess = false;

            try
            {
                //  Declare variables
                IntPtr ptrSCMManager = IntPtr.Zero;
                IntPtr ptrService = IntPtr.Zero;
                int nLastErrorCode = 0;


                //	Open the service control manager
                ptrSCMManager = OpenSCManager(null, null, SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (ptrSCMManager != IntPtr.Zero)
                {
                    // Open the Service
                    ptrService = OpenService(ptrSCMManager, svcName, SERVICE_ACCESS.SERVICE_ALL_ACCESS);
                    if (ptrService != IntPtr.Zero)
                    {
                        //	Set your return value
                        bSuccess = true;
                    }
                    else
                    {
                        //	Get the last error code.
                        nLastErrorCode = Marshal.GetLastWin32Error();

                        //	If the service does not exist
                        if (nLastErrorCode != ERROR_SERVICE_DOES_NOT_EXIST)
                        {
                            throw new Exception("Unable to open the service.\n" +
                                                    "Error Code:" + nLastErrorCode.ToString());
                        }
                    }
                }
                else
                {
                    throw new Exception("Unable to open the service control manager.\n" +
                                        "Error Code:" + Marshal.GetLastWin32Error().ToString());
                }
            }
            catch (Exception ex)
            {
                //	Notify all of the trace listeners of the error
                Trace.WriteLine("Source:" + ex.Source + ", Description:" + ex.Message);

                //	Throw the exception up
                throw ex;
            }

            //  Return
            return bSuccess;
        }

        /// <summary>
        /// This method will get the current status of the service
        /// </summary>
        /// <param name="svcName">The name of the service that you want to get the status for</param>
        /// <param name="bOutputToConsole">If set to True, it will output the current status to the standard out (console)</param>
        /// <returns>The service status</returns>
        public SERVICE_STATUS GetServiceStatus(string svcName, bool bOutputToConsole)
        {
            var serviceStatus = new SERVICE_STATUS();

            try
            {
                //  Declare variables
                var hSCMManager = IntPtr.Zero;
                var hService = IntPtr.Zero;
                var nLastErrorCode = 0;


                //  Open the service control manager
                hSCMManager = OpenSCManager(null, null, SCM_ACCESS.SC_MANAGER_GENERIC_READ);
                if (hSCMManager != IntPtr.Zero)
                {
                    //  Open the Service
                    hService = OpenService(hSCMManager, svcName, SERVICE_ACCESS.SERVICE_QUERY_STATUS);
                    if (hService != IntPtr.Zero)
                    {
                        //  Query the service to get the status
                        ControlService(hService, SERVICE_CONTROL.INTERROGATE, ref serviceStatus);

                        //  Output the service status
                        if (bOutputToConsole)
                            Console.WriteLine(Environment.NewLine + 
                                                "Current status of the service: " + serviceStatus.dwCurrentState.ToString("f") + 
                                                Environment.NewLine);
                    }
                    else
                    {
                        //	Get the last error code.
                        nLastErrorCode = Marshal.GetLastWin32Error();

                        //	If the service does not exist
                        if (nLastErrorCode != ERROR_SERVICE_DOES_NOT_EXIST)
                            throw new Exception("Unable to open the service." + Environment.NewLine +
                                                    "Error Code:" + nLastErrorCode.ToString());
                        else
                            throw new Exception("The specified service does not exist!");
                    }
                }
                else
                    throw new Exception("Unable to open the service control manager.\n" +
                                        "Error Code:" + Marshal.GetLastWin32Error().ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                if (bOutputToConsole)
                    Console.WriteLine(ex.Message);
                throw ex;
            }
            
            return serviceStatus;
        }

        #endregion
    }
}
