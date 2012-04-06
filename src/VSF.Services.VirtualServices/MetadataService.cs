using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace VSF.Services.VirtualServices
{
    /// <summary>
    /// This is the Interface file for the MetaDataService, which is a virtual service 
    /// that provides data about the health of the Windows Service as well as the other 
    /// virtual services on the machine.
    /// </summary>
    /// <remarks>
    /// NOTE: If you change the class name "MetadataService" here, you must also update the reference to "MetadataService" in App.config.
    /// </remarks>    
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class MetadataService : VirtualServiceBase, IMetadataService
    {
        #region Constants

        private const string SERVICE_NAME = "MetadataService";

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the MetadataService
        /// </summary>
        public MetadataService()
        {
            this.ServiceName = SERVICE_NAME;
        }


        #region Interface Methods
                
        #endregion

        #endregion
    }
}
