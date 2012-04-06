using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSF.Services.Windows
{
    /// <summary>
    /// The constants used for the Windows Service
    /// </summary>
    public class WindowsServiceConstants
    {
        #region Service Constants

        /// <summary>
        /// The name of the service
        /// </summary>
        /// <remarks>
        /// This is the name that would be used in a NET START command
        /// </remarks>
        public const string SERVICE_NAME = "VSFWinServ";
        /// <summary>
        /// The display name of the service
        /// </summary>
        public const string SERVICE_DISPLAY_NAME = "Virtual Services Framework Windows Service";
        /// <summary>
        /// The description of the service.
        /// </summary>
        public const string SERVICE_DESCRIPTION = "Provides simple hosting framework for virtual services.";
        /// <summary>
        /// The executable file name of the service.
        /// </summary>
        public const string SERVICE_EXECUTABLE = "VSFWinServ.exe";        
        /// <summary>
        /// This is the amount of time to wait before polling the service again.
        /// </summary>
        /// <remarks>Time is expressed in milliseconds</remarks>
        public const int POLLDELAY = 1000;
        /// <summary>
        /// This is the amount of time to wait before timing out.
        /// </summary>
        /// <remarks>Time is expressed in milliseconds</remarks>
        public const int TIMEOUT = 10000;

        #endregion
    }
}
