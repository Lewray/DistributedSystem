using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ProcessContracts
{
    [ServiceContract(Name = "Process", Namespace = "http://ProcessContracts")]
    public interface IProcess
    {
        /// <summary>
        /// Starts up all components of the IProcess implementation
        /// </summary>
        void Start();

        /// <summary>
        /// Cleanly stops all running processes within the IProcess implementation
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts an HttpListener to provide a status report of the system when naviagted to in a web browser.
        /// </summary>
        void StartStatusServer();
    }
}
