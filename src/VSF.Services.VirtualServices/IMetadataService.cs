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
    [ServiceContract]
    public interface IMetadataService
    {
        [OperationContract]
        string Report();
    }
}
