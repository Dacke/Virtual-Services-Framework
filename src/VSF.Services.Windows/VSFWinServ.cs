using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace VSF.Services.Windows
{
    /// <summary>
    /// This is the class that IS the windows service
    /// </summary>
    public partial class VSFWinServ : ServiceBase
    {
        #region Private Members

        private VirtualServicesManager _virServMgr = null;
      
        #endregion        

        #region Public Methods

        /// <summary>
        /// Creates an instance of the Virtual Services Framework Windows Service (VSFWinServ) object
        /// </summary>
        public VSFWinServ() : this(VirtualServiceStore.VIRTUAL_SERVICE_CONFIG_FOLDER) { }

        /// <summary>
        /// Creates an instance of the Virtual Services Framework Windows Service (VSFWinServ) object
        /// </summary>
        /// <param name="ConfigurationFolder">The folder where the services configuration file is located.</param>
        public VSFWinServ(string ConfigurationFolder)
        {
            //  Initialize your object
            InitializeComponent();

            //  Setup the Windows Service
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.CanHandlePowerEvent = true;
            this.AutoLog = true;

            //  Setup the virtual services manager
            _virServMgr = new VirtualServicesManager(ConfigurationFolder);
        }

        #endregion

        #region Private Methods
        
        #region Event Handlers

        /// <summary>
        /// This handles the OnStart service event
        /// </summary>
        protected override void OnStart(string[] args)
        {
            try
            {
                Trace.WriteLine("On Start");
                foreach (var arg in args)
                {
                    Trace.WriteLine("OnStart Arg: " + arg);
                }                
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// This handles the OnStop service event
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                Trace.WriteLine("On Stop");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// This handles the OnPause service event
        /// </summary>
        protected override void OnPause()
        {
            try
            {
                Trace.WriteLine("On Pause");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// This handles the OnContinue service event
        /// </summary>
        protected override void OnContinue()
        {
            try
            {
                Trace.WriteLine("On Continue");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// This occurs when a power event was triggered.
        /// </summary>
        /// <param name="powerStatus">Indicates the system's power status</param>
        /// <returns>True if we are going to accept the query, otherwise false</returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            try
            {
                Trace.WriteLine("On Power Event");
                Trace.WriteLine(powerStatus.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            //When implemented in a derived class, the needs of your application determine what value to return. 
            //For example, if a QuerySuspend broadcast status is passed, you could cause your application to 
            //reject the query by returning false.
            return base.OnPowerEvent(powerStatus);
        }

        #endregion

        #endregion
    }
}
