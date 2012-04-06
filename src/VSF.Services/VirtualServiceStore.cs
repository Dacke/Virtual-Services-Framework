using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using VSF.Services.VirtualServices;

namespace VSF.Services
{
    public class VirtualServiceStore
    {
        #region Constants

        /// <summary>
        /// This is the folder location that the common configuration files live in.
        /// </summary>
        public static string VIRTUAL_SERVICE_CONFIG_FOLDER = (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VirtualServicesFramework");

        /// <summary>
        /// This is the XML file that holds the virtual service configuration.  
        /// It may be located in the Windows System 32 folder.
        /// </summary>
        public const string VIRTUAL_SERVICE_CONFIGFILE = "Services.xml";
        
        #endregion

        #region Properties

        /// <summary>
        /// The data source for the service information
        /// </summary>
        public XElement DataSource { get; set; }

        /// <summary>
        /// This is the folder that holds the configuration file information.
        /// </summary>
        public string ConfigurationFolder { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        /// Creates a new instance of the Virtual Service Storage object
        /// </summary>
        public VirtualServiceStore() : this(VIRTUAL_SERVICE_CONFIG_FOLDER) { }

        /// <summary>
        /// Creates a new instance of the Virtual Service Storage object
        /// </summary>
        /// <param name="VirtualServiceConfigurationFolder">The virtual services configuration folder</param>
        public VirtualServiceStore(string VirtualServiceConfigurationFolder)
        {
            try
            {
                //  Set your property
                ConfigurationFolder = VirtualServiceConfigurationFolder;
                Trace.WriteLine("Configuration Folder: " + ConfigurationFolder);
                //  Initialize your configuration file
                InitializeStore();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw ex;
            }            
        }

        /// <summary>
        /// This method will cause the data store to reload from the XML file.
        /// </summary>
        public void RefreshDataStore()
        {
            //  Load the Services
            DataSource = XElement.Load(ConfigurationFolder + 
                                        "\\" + 
                                        VIRTUAL_SERVICE_CONFIGFILE);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method will actually initialize the data store (if it does not exist)
        /// </summary>
        private void InitializeStore()
        {
            //  Declares
            var sFullFilePath = (ConfigurationFolder + "\\" + VIRTUAL_SERVICE_CONFIGFILE);


            //  If the directory does not exist, then create it.
            if (Directory.Exists(ConfigurationFolder) == false)
            {
                //  Create the folder
                Directory.CreateDirectory(ConfigurationFolder);

                //  Create the blank file
                GenerateServicesXMLFile(sFullFilePath);
            }
            else
            {
                //  If the file does not exist, then create one.
                if (File.Exists(sFullFilePath) == false)
                    GenerateServicesXMLFile(sFullFilePath);
            }
        }

        /// <summary>
        /// This method will generate an empty services xml file (so you can add services)
        /// </summary>
        private void GenerateServicesXMLFile(string sFullFilePath)
        {
            //  Create the default (included in every service) virtual service
            var metaServ = new MetadataService();

            //  Create the blank issue
            XElement xEle = new XElement(ServicesXmlConstants.TAG_SERVICES,
                                new XElement(ServicesXmlConstants.TAG_SERVICE,
                                    new XAttribute(ServicesXmlConstants.ATTRIB_SERVICE_NAME, metaServ.ServiceName),
                                    new XElement(ServicesXmlConstants.TAG_TYPE, metaServ.TypeName),
                                    new XElement(ServicesXmlConstants.TAG_ASSEMBLY, metaServ.AssemblyName)));

            //  Save the file
            xEle.Save(sFullFilePath);
        }

        #endregion
    }
}
