using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSF.Services.VirtualServices
{
    public class MetadataService : VirtualServiceBase
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
            ServiceName = SERVICE_NAME;
        }

        #endregion
    }
}
