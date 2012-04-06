using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Configuration;

namespace VSF.Services.Windows
{
	static class Program
	{
		#region Properties

		/// <summary>
		/// The name of the Windows Service
		/// </summary>
		public static string ServiceName { get; set; }
		/// <summary>
		/// The display name of the Windows Service
		/// </summary>
		public static string DisplayName { get; set; }
		/// <summary>
		/// The description of the Windows Service.
		/// </summary>
		public static string ServiceDescription { get; set; }
		/// <summary>
		/// This is the file path to the configuration file.
		/// </summary>
		public static string ConfigurationFilePath { get; set; }

		#endregion

		#region Constants

		/// <summary>
		/// This message is to be used when the end user has not entered a valid
		/// command line argument.
		/// </summary>
		private const String CMDLINE_ARG_MSG = "An invalid argument was passed into the service.\n\n" +
												"Valid Arguments (case insensitive):\n" +
												"-Install		Installs the service into the system.\n" +
												"-Uninstall		Uninstalls the service from the system.\n" +
												"-Start			Starts the service.\n" +
												"-Stop			Stops the service.\n" +
												"-Pause			Pauses the service.\n" +
												"-Resume			Resumes the service.\n" +
												"-Status			Returns the current status of the service.\n" +
												"-ConsoleHost		Runs the ServicesManager as a Console App not as a service.";

		private const int S_OK = 0;

		#endregion

		#region Static Methods

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// <returns>S_OK (0) if no error has occurred, otherwise the error number.</returns>
		static int Main(string[] args)
		{
			var nResult = S_OK;

			//  Setup your default properties
			ConfigurationFilePath = VirtualServiceStore.VIRTUAL_SERVICE_CONFIG_FOLDER;

			try
			{
				//	Declare variables
				ServiceController scController = null;
				CustomServiceInstaller cusInstaller = new CustomServiceInstaller();
				var bSuccess = false;
				

				//  Look in the application settings of the configuration file.
				if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["ServiceName"]) == false)
					ServiceName = ConfigurationManager.AppSettings["ServiceName"];
				else
					ServiceName = WindowsServiceConstants.SERVICE_NAME;

				//  Parse out the command line parameters
				ParseCommandLineArguments(args);

				//  Parse out the registry items.
				ParseRegistry();

				//  Create your service controller
				scController = new ServiceController(ServiceName);

				//  Determine how many command line arguments accompanied the executable
				if (args.Length > 0)
				{
					switch (args[0].ToUpper())
					{
						case "-INSTALL":
							{
								//  Install the service
								Install(cusInstaller);

								//	Write a success message out
								Console.WriteLine("The service was installed successfully.");

								break;
							}
						case "-UNINSTALL":
						case "-REMOVE":
							{
								//	Make sure that the service is not already uninstalled (never installed)
								if (cusInstaller.DoesServiceExists(ServiceName) == true)
								{
									//	Attempt to uninstall the service.
									bSuccess = cusInstaller.UnInstallService(ServiceName,
																				WindowsServiceConstants.TIMEOUT,
																				WindowsServiceConstants.POLLDELAY);
									if (bSuccess == false)
										throw new Exception("Unable to uninstall the service successfully.");

									//	Write out a success message
									Console.WriteLine("The service was uninstalled successfully.");
								}
								else
									throw new Exception("The service does not exist as an installed service.");

								break;
							}
						case "-STATUS":
							{
								//	Write the status of the service to the console                                
								Console.WriteLine("Service Status:" + scController.Status.ToString() + Environment.NewLine);
								break;
							}
						case "-START":
							{
								//  Make sure that the service is stopped
								if (scController.Status == ServiceControllerStatus.Stopped)
								{
									//  Attempt to start the service
									scController.Start();
									Console.WriteLine("The service was started successfully.");
								}
								else
									Console.WriteLine("Current Service State:" + scController.Status.ToString() +
														Environment.NewLine +
														"The service is not in a state which can be started.");
								break;
							}
						case "-STOP":
							{
								//  Make sure that the service is stopped
								if (scController.Status == ServiceControllerStatus.Running)
								{
									//  Attempt to start the service
									scController.Stop();
									Console.WriteLine("The service was stopped successfully.");
								}
								else
									Console.WriteLine("Current Service State:" + scController.Status.ToString() +
														Environment.NewLine +
														"The service is not in a state which can be stopped.");
								break;
							}
						case "-PAUSE":
							{
								//  Make sure that the service is stopped
								if (scController.Status == ServiceControllerStatus.Running)
								{
									//  Attempt to start the service
									scController.Pause();
									Console.WriteLine("The service was paused successfully.");
								}
								else
									Console.WriteLine("Current Service State:" + scController.Status.ToString() +
														Environment.NewLine +
														"The service is not in a state which can be paused.");
								break;
							}
						case "-CONTINUE":
						case "-RESUME":
							{
								//  Make sure that the service is stopped
								if (scController.Status == ServiceControllerStatus.Paused)
								{
									//  Attempt to start the service
									scController.Continue();
									Console.WriteLine("The service was resumed successfully.");
								}
								else
									Console.WriteLine("Current Service State:" + scController.Status.ToString() +
														Environment.NewLine +
														"The service is not in a state which can be resumed.");
								break;
							}
						case "-CONSOLEHOST":
							{
								//  Create the console host service controller
								var svcConsoleHostCtrl = new ServicesManagerConsoleHostContoller();

								if (String.IsNullOrEmpty(ConfigurationFilePath) == false)
									svcConsoleHostCtrl.ConfigurationFolder = ConfigurationFilePath;

								svcConsoleHostCtrl.Start();
								break;
							}
						default:
							{
								//  Output the message telling them about the valid arguments
								Console.WriteLine(CMDLINE_ARG_MSG);
								throw new InvalidOperationException("Command line arguments not recognized.");
							}
					}
				}
				else
				{
					//  Since no arguments were passed, just attempt to start the service as a normal 
					//  windows service (The windows service control manager calls this methods)
					ServiceBase[] ServicesToRun;
					ServicesToRun = new ServiceBase[] 
					{ 
						new VSFWinServ(ConfigurationFilePath)
					};
					ServiceBase.Run(ServicesToRun);
				}
			}
			catch (Exception ex)
			{
				if (Debugger.IsAttached)
					Debugger.Break();
				Trace.WriteLine(ex);
				Console.WriteLine("\nUnable to perform the selected action\n\nSource: " + ex.Source + "\nDescription: " + ex.Message);
				nResult = 69;
			}

			return nResult;
		}

		/// <summary>
		/// This method is used to install the service without a dependency.  This would need 
		/// to be the case if the dependency doesn't actually exist.
		/// </summary>
		/// <param name="customInstaller">The custom installer.</param>
		static void Install(CustomServiceInstaller customInstaller)
		{
			Install(customInstaller, null);
		}

		/// <summary>
		/// This method is used to actually install the service.  The service is installed with the 
		/// specified dependency included.  If the dependency parameter is null, no dependency is
		/// created.
		/// </summary>
		/// <param name="customInstaller">The installer object.</param>
		/// <param name="dependencies">The specified dependency.</param>
		static void Install(CustomServiceInstaller customInstaller, string dependencies)
		{
			//	Make sure that the service does not already exist as an installed service
			//if (customInstaller.DoesServiceExists(WindowsServiceConstants.SERVICE_NAME) == false)
			if (customInstaller.DoesServiceExists(ServiceName) == false)
			{
				//	Build the path to the executable
				var sServiceExe = Environment.CurrentDirectory + "\\" + WindowsServiceConstants.SERVICE_EXECUTABLE;

				//	Attempts to install the service.
				var bSuccess = customInstaller.InstallService(sServiceExe, ServiceName, DisplayName, ServiceDescription,
					CustomServiceInstaller.SERVICE_START_TYPE.SERVICE_AUTO_START,
					dependencies);

				if (bSuccess == false)
					throw new Exception("Unable to install the service successfully.");
			}
			else
				throw new Exception("The service already exists as an installed service.");
		}

		/// <summary>
		/// This method will parse the registry entries (if any) that have been made for the service.
		/// </summary>
		private static void ParseRegistry()
		{
			try
			{
				Trace.WriteLine("Start of Parsing the Registry");
				//  Open a read only view into the registry
				var serviceParamRegKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" +
																			ServiceName + "\\Parameters",
																			RegistryKeyPermissionCheck.ReadSubTree,
																			(RegistryRights.ReadKey | RegistryRights.EnumerateSubKeys | RegistryRights.QueryValues));
				if (serviceParamRegKey != null)
				{
					foreach (var valueNameItem in serviceParamRegKey.GetValueNames())
					{
						switch (valueNameItem)
						{
							case "ServiceName":
								ServiceName = serviceParamRegKey.GetValue(valueNameItem, null).ToString();
								break;
							case "ConfigurationFolder":
								ConfigurationFilePath = serviceParamRegKey.GetValue(valueNameItem, null).ToString();
								break;
							default:
								Trace.WriteLine(String.Format("Sub Key Name: {0}, Value: {1} not implemented", valueNameItem,
													serviceParamRegKey.GetValue(valueNameItem, null)));
								break;
						}
					}
				}
				else
					Trace.WriteLine(String.Format("Unable to find the registry key {0}\\Parameters", ServiceName));
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Unable to load any registry parameter items due to the following error.");
				Trace.WriteLine(ex);
			}
		}

		/// <summary>
		/// This method will look for the various command line arguments to see if they exist.
		/// </summary>
		/// <param name="args">The argument list.</param>
		private static void ParseCommandLineArguments(string[] args)
		{
			var value = String.Empty;

			//  Get the Service Name from the command line, persist if needed.
			var serviceNameQuery = (from arguments in args
									where arguments.ToUpper().Contains("-SERVICENAME=")
									select arguments).FirstOrDefault<String>();
			if (String.IsNullOrEmpty(serviceNameQuery) == false)
			{
				var nIndex = (serviceNameQuery.IndexOf('=') + 1);
				if (nIndex > 0)
					ServiceName = serviceNameQuery.Substring(nIndex).Trim('"');
			}

			//  Get the Display Name
			var displayNameQuery = (from arguments in args
									where arguments.ToUpper().Contains("-DISPLAYNAME=")
									select arguments).FirstOrDefault<String>();
			if (String.IsNullOrEmpty(displayNameQuery) == false)
			{
				var nIndex = (displayNameQuery.IndexOf('=') + 1);
				if (nIndex > 0)
				{
					DisplayName = displayNameQuery.Substring(nIndex).Trim('"'); ;
					if (String.IsNullOrEmpty(DisplayName))
						DisplayName = WindowsServiceConstants.SERVICE_DISPLAY_NAME;
				}
			}
			else
				DisplayName = WindowsServiceConstants.SERVICE_DISPLAY_NAME;

			//  Get the Service Description
			var serviceDescriptionQuery = (from arguments in args
										   where arguments.ToUpper().Contains("-DESCRIPTION=")
										   select arguments).FirstOrDefault<String>();
			if (String.IsNullOrEmpty(serviceDescriptionQuery) == false)
			{
				var nIndex = (serviceDescriptionQuery.IndexOf('=') + 1);
				if (nIndex > 0)
				{
					ServiceDescription = serviceDescriptionQuery.Substring(nIndex).Trim('"'); ;
					if (String.IsNullOrEmpty(ServiceDescription))
						ServiceDescription = WindowsServiceConstants.SERVICE_DESCRIPTION;
				}
			}
			else
				ServiceDescription = WindowsServiceConstants.SERVICE_DESCRIPTION;
		}

		#endregion
	}
}
