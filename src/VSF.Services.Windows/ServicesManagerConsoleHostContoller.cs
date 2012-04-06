using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VSF.Services.Windows
{
    class ServicesManagerConsoleHostContoller
    {
        #region Private Members

        private VirtualServicesManager _virServMgr = null;

        #endregion

        #region Constants

        private const string CONSOLE_HEADER = "═════════════════════════════════════════[ Virtual Services Console Host ]════";
        private const string CONSOLE_FOOTER = "══════════════════════════════════════════════════════════════════════════════";
        private const string USER_COMMANDS = "  Start\t - Starts all of the virtual services.\n" +
                                             "  Stop\t - Stops all of the virtual services.\n" +
                                             "  Report - Causes the virtual services to report their status.\n" + 
                                             "  Exit\t - Exit the program.";

        #endregion

        #region Properties

        public string Version { get; set; }
        public string ConfigurationFolder { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the Service Manager Console Host Controller object
        /// </summary>
        public ServicesManagerConsoleHostContoller() : this ("Unknown") {}

        /// <summary>
        /// Creates a new instance of the Service Manager Console Host Controller object
        /// </summary>
        /// <param name="winServiceVersion">The version of the Windows Service hosting</param>        
        public ServicesManagerConsoleHostContoller(string winServiceVersion)
        {
            ConfigurationFolder = VirtualServiceStore.VIRTUAL_SERVICE_CONFIG_FOLDER;
            Version = winServiceVersion;
        }

        /// <summary>
        /// This method will start the services
        /// </summary>
        public void Start()
        {
            try
            {
                //  Declares
                var sUserCmd = String.Empty;
                String[] sCmdArgs = null;


                //  Output the list of commands
                Console.Clear();
                Console.WriteLine(CONSOLE_HEADER);
                Console.WriteLine(USER_COMMANDS);
                Console.WriteLine(CONSOLE_FOOTER);
                
                //  Start the virtual services
                StartServices();
                Console.WriteLine(CONSOLE_FOOTER);
                                
                //  Loop around until it is time to exit
                while ((sUserCmd.ToUpper() != "EXIT") && (sUserCmd.ToUpper() != "E"))
                {                    
                    //  Pass along the user command to the handler
                    HandleUserCommand(sUserCmd, sCmdArgs);

                    //  Create your own command prompt
                    Console.Write("Cmd> ");

                    //  Get the user command
                    var sConsoleInput = Console.ReadLine();

                    //  Split on spaces into the command and then arguments
                    var sArgs = sConsoleInput.Split(' ');
                    if (sArgs.Length > 0)
                    {
                        sCmdArgs = new String[(sArgs.Length - 1)];
                        sUserCmd = sArgs[0];

                        //  Copy the command arguments
                        if (sArgs.Length > 1)
                            Array.Copy(sArgs, 1, sCmdArgs, 0, (sArgs.Length - 1));
                    }
                }

                //  Notify the user you are exiting
                Console.WriteLine(CONSOLE_FOOTER);
                if (Debugger.IsAttached)
                    Console.WriteLine("While you sleep, they'll be waiting...");
                Console.WriteLine("Exiting...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while attempting to process the command." + Environment.NewLine +
                                    "Source: " + ex.Source + Environment.NewLine +
                                    "Description: " + ex.Message);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method will start the services
        /// </summary>        
        protected void StartServices()
        {
            Console.WriteLine("Creating ServicesManager...");
            _virServMgr = new VirtualServicesManager(ConfigurationFolder);
            Console.WriteLine("\t...ServicesManager Created");

            Console.WriteLine("Initializing ServicesManager...");
            _virServMgr.Initialize();
            Console.WriteLine("\t...ServicesManager Initialized");

            Console.WriteLine("Starting Virtual Services...");
            _virServMgr.StartServices();
            Console.WriteLine("\t...Virtual Services Started");
        }

        /// <summary>
        /// When a user enters a command this method will process that command. 
        /// </summary>
        /// <param name="strCommand">The command that the user has entered</param>
        /// <param name="strArgs">The command arguments</param>
        protected void HandleUserCommand(string strCommand, params string[] strArgs)
        {
            try
            {
                //  Determine which command the user is executing.
                switch (strCommand.ToUpper())
                {
                    //  Start all of the services
                    case "START":
                        if (strArgs.Length > 0)
                            _virServMgr.StartService(strArgs[0]);
                        else
                            _virServMgr.StartServices();
                            break;
                    //  Stop all of the services
                    case "STOP":
                        if (strArgs.Length > 0)
                            _virServMgr.StopService(strArgs[0]);
                        else
                            _virServMgr.StopServices();
                            break;
                    //  This will loop through each of the services and ask them to provide a report
                    case "STATUS":
                    case "REPORT":
                            _virServMgr.ReportServices();
                            break;
                    case "":
                        //  Do nothing
                        break;
                    default:
                            Console.WriteLine(" WARNING: Unknown command" + Environment.NewLine +
                                                " Valid Commands:" + Environment.NewLine +
                                                USER_COMMANDS + Environment.NewLine);
                            break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw ex;
            }
        }

        #endregion
    }
}
