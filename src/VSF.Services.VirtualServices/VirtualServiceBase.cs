using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Reflection;


namespace VSF.Services
{
    /// <summary>
    /// All virtual services must inherit from this base class to ensure proper information.
    /// </summary>
    public class VirtualServiceBase
    {
        #region Properties

        public String ServiceName { get; set; }
        public String AssemblyName { get; set; }
        public String TypeName { get; set; }
        public Hashtable ServiceProperties { get; set; }
        public Exception LastException { get; set; }
                
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the virtual service
        /// </summary>
        /// <param name="sServiceName">The name of the service that you want to find.</param>
        public VirtualServiceBase() 
        {
            ServiceProperties = null;
            LastException = null;
            TypeName = this.GetType().FullName;
            AssemblyName = this.GetType().Assembly.FullName;
        }

        /// <summary>
        /// This method resets the last exception in the service
        /// </summary>
        public void ResetExceptions() 
        {
            LastException = null;
        }
        
        #region Methods you should to override

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <remarks>It is recommended that you override this method.</remarks>
        public virtual void Start() { }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <remarks>It is recommended that you override this method.</remarks>
        public virtual void Stop() { }

        /// <summary>
        /// This method allows the virtual service to describe themselves
        /// </summary>
        /// <remarks>It is recommended that you override this method.</remarks>
        /// <returns>This is the descriptive string</returns>
        public virtual string Report() 
        {
            var sReport = (ServiceName + ": ");

            if (LastException != null)
                sReport += LastException.Message;
            else
                sReport += "Functioning Normally.";

            return sReport;
        }

        /// <summary>
        /// This method will take a parameter list of objects that you may use to start your service.
        /// </summary>
        /// <remarks>It is recommended that you override this method.</remarks>
        /// <param name="startupParameters"></param>
        public virtual void InitializeValues(params object[] startupParameters) {}

        #endregion

        #endregion
    }
}
