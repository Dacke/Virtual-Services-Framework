using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;
using System.ServiceModel;
using System.Xml.Linq;

namespace VSF.Services
{
    /// <summary>
    /// This object is in charge of managing the virtual services.
    /// </summary>
    public class VirtualServicesManager
    {
        #region Private Members

        private VirtualServiceStore _virServStore = null;
        private Hashtable _virServices = null;

        #endregion

        #region Properties

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        public string ConfigurationFile { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the object
        /// </summary>
        public VirtualServicesManager() : this(VirtualServiceStore.VIRTUAL_SERVICE_CONFIG_FOLDER) { }

        /// <summary>
        /// Creates an new instance of the object.
        /// </summary>
        /// <param name="ConfigurationFolder">The folder that holds the virtual services configuration file.</param>
        public VirtualServicesManager(string ConfigurationFolder)
        {
            //  Create your objects
            _virServStore = new VirtualServiceStore(ConfigurationFolder);
            _virServices = new Hashtable();
        }

        /// <summary>
        /// This method will read the configuration information from the XML storage so that it knows 
        /// which services it is supposed to host.
        /// </summary>
        public void Initialize() {}

        /// <summary>
        /// This method will the virtual services listening for their connections
        /// </summary>
        public void StartServices()
        {
            try
            {
                //  Refresh the data store
                _virServStore.RefreshDataStore();

                //  Proceed if you have data
                if (_virServStore.DataSource != null)
                {
                    //  Create the LINQ query to get the elements
                    var servQuery = from serv in _virServStore.DataSource.Elements(ServicesXmlConstants.TAG_SERVICE)
                                    select serv;

                    foreach (XElement srvItem in servQuery)
                    {
                        //  Create the object
                        var virServObj = CreateVirtualServiceObject(srvItem);

                        //  Start the service
                        StartVirtualService(virServObj);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                Trace.WriteLine(ex);
                throw ex;
            }
        }

        /// <summary>
        /// This method will stop all of the virtual services.
        /// </summary>
        public void StopServices()
        {
            try
            {
                //  Enumerate through the hash table to start the services
                foreach (DictionaryEntry dicEntry in _virServices)
                {
                    try
                    {
                        if (dicEntry.Value is ServiceHost)
                        {
                            var svcHost = (ServiceHost)dicEntry.Value;
                            var hostedService = svcHost.SingletonInstance;

                            if (hostedService is VirtualServiceBase)
                            {
                                Console.WriteLine("\tStopping Service: " + ((VirtualServiceBase)hostedService).ServiceName);

                                ((VirtualServiceBase)hostedService).Stop();

                                Console.WriteLine("\tService Successfully Stopped");
                            }

                            //  Close the host connection
                            svcHost.Close(TimeSpan.FromSeconds(5));
                        }
                        else
                            throw new InvalidCastException("Unable to stop the service because the value does not resolve to type ServiceHost");

                    }
                    catch (Exception servEx)
                    {
                        Console.WriteLine("\tService Stop Failed");
                        Console.WriteLine("\tERROR:" + servEx.Message);
                    }
                }
        
                //  Clean up the hash table
                _virServices.Clear();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw ex;
            }
        }
        
        /// <summary>
        /// This method will get the running status of the virtual services.
        /// </summary>
        public void ReportServices()
        {
            try
            {
                //  Enumerate through the hash table to start the services
                foreach (DictionaryEntry dicEntry in _virServices)
                {
                    Console.WriteLine("\tReporting Status of Service: " + dicEntry.Key.ToString());
                    //  Embedded try so that if one service fails
                    //  they don't all fail
                    try
                    {
                        if (dicEntry.Value is ServiceHost)
                        {
                            var hostedService = ((ServiceHost)dicEntry.Value).SingletonInstance;
                            if (hostedService is VirtualServiceBase)
                                Console.WriteLine("\t" + ((VirtualServiceBase)hostedService).Report());
                            else
                                Console.WriteLine("\tThe hosted service does not have a valid instance.");
                        }
                        else
                            throw new InvalidCastException("Unable to report the status of the service because the value does not resolve to type ServiceHost");
                        
                    }
                    catch (Exception servEx)
                    {
                        Console.WriteLine("\tService Report Failed");
                        Console.WriteLine("\tERROR:" + servEx.Message);
                    }
                }

                Console.WriteLine("\tReport Finished");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw ex;
            }
        }

        /// <summary>
        /// This method is used to start the individual service given the service name
        /// </summary>
        /// <param name="ServiceName">The service name that you want to start</param>
        public void StartService(string ServiceName)
        {
            try
            {
                //  Refresh the data store
                _virServStore.RefreshDataStore();

                //  Proceed if you have data
                if (_virServStore.DataSource != null)
                {
                    //  Create the LINQ query to get the elements
                    var servQuery = from serv in _virServStore.DataSource.Elements(ServicesXmlConstants.TAG_SERVICE)
                                    where (serv.Attribute(ServicesXmlConstants.ATTRIB_SERVICE_NAME).Value.ToUpper() == ServiceName.ToUpper())
                                    select serv;

                    if (servQuery.Count() > 0)
                    {
                        foreach (var srvItem in servQuery)
                        {
                            //  Create the object
                            var virServObj = CreateVirtualServiceObject(srvItem);

                            //  Only start it if it has not already been started
                            if (_virServices.Contains(virServObj.ServiceName) == false)
                                StartVirtualService(virServObj);
                            else
                                Console.WriteLine("\tThe service is already started.");
                        }
                    }
                    else
                        Console.WriteLine("\tThe specified service was not found in the configuration file.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw ex;
            }
        }

        /// <summary>
        /// This method is used to stop the individual service given the service name
        /// </summary>
        /// <param name="ServiceName">The service name that you want to stop</param>
        public void StopService(string ServiceName)
        {
            try
            {
                var htVirServClone = (Hashtable)_virServices.Clone();

                //  Enumerate through the hash table to start the services
                foreach (DictionaryEntry dicEntry in htVirServClone)
                {
                    //  Only stop the service that matches
                    if (dicEntry.Key.ToString().ToUpper() == ServiceName.ToUpper())
                    {
                        if (dicEntry.Value is ServiceHost)
                        {                            
                            var svcHost = (ServiceHost)dicEntry.Value;
                            var hostedService = svcHost.SingletonInstance;

                            //  Make sure you have an object
                            if (hostedService is VirtualServiceBase)
                            {
                                Console.WriteLine("\tStopping Service: " + ((VirtualServiceBase)hostedService).ServiceName);

                                //  Stop the virtual service
                                ((VirtualServiceBase)hostedService).Stop();

                                Console.WriteLine("\tService Successfully Stopped");
                            }

                            //  Close the host connection
                            svcHost.Close(TimeSpan.FromSeconds(5));

                            //  Remove from the virtual services
                            _virServices.Remove(dicEntry.Key);
                        }
                        else
                            throw new InvalidCastException("Unable to stop the service because the value does not resolve to type ServiceHost");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\tService Stop Failed");
                Console.WriteLine("\tERROR:" + ex.Message);
                Trace.WriteLine(ex);
                throw ex;                
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method will create an instance of the virtual service object
        /// </summary>
        /// <param name="srvItem">A reference to the XElement containing the service object</param>
        /// <returns></returns>
        private VirtualServiceBase CreateVirtualServiceObject(XElement srvItem)
        {
            //  Create the Item
            var virServ = (VirtualServiceBase)Activator.CreateInstance(srvItem.Element(ServicesXmlConstants.TAG_ASSEMBLY).Value,
                                                                       srvItem.Element(ServicesXmlConstants.TAG_TYPE).Value).Unwrap();

            //  Assign any properties here
            AssignServiceProperties(virServ, srvItem);

            return virServ;
        }

        /// <summary>
        /// This method actually performs the work of starting the virtual service
        /// </summary>
        /// <param name="srvItem">The XElement that contains the service information.</param>
        private void StartVirtualService(VirtualServiceBase virService)
        {
            Console.WriteLine("\tStarting Service: " + virService.ServiceName);

            //  Call the service start
            virService.Start();

            //  Create a new service host
            var svcHost = new ServiceHost(virService);
            svcHost.Open(TimeSpan.FromSeconds(5));

            //  Add the service to the hash table
            _virServices.Add(virService.ServiceName, svcHost);

            Console.WriteLine("\tService Started Successfully");
        }

        /// <summary>
        /// This will assign any properties that found in the Services XML file.
        /// </summary>
        /// <param name="virServ">The virtual service object that you are assigning properties</param>
        /// <param name="srvItem">The assembly element that is the root element.</param>
        private void AssignServiceProperties(VirtualServiceBase virServ, XElement srvItem)
        {
            var propQuery = (from props in srvItem.Elements(ServicesXmlConstants.PROPERTIES)
                             select props).Descendants();

            foreach (var propItem in propQuery)
            {
                Trace.WriteLine(String.Format("Property {0}, Value {1}", propItem.Name, propItem.Value));
                
                //  Get the property item, if the property is found and 
                //  it can be written to (not read only) then it will 
                //  set the value to the property.
                var propInfo = virServ.GetType().GetProperty(propItem.Name.LocalName);
                if (propInfo != null)
                {
                    if (propInfo.CanWrite)
                        propInfo.SetValue(virServ, Convert.ChangeType(propItem.Value, propInfo.PropertyType) , null);
                }
            }
        }

        #endregion
    }
}
