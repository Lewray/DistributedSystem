using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ElectionContracts
{
    [ServiceContract(Name = "Election", Namespace = "http://ElectionContracts")]
    public interface IElection
    {
        /// <summary>
        /// Raised when the process is successfully elected as the new Coordinator
        /// </summary>
        event EventHandler Elected; 

        /// <summary>
        /// Gets a value representing the number of IElection implementations found through WCF Discovery
        /// </summary>
        int ConstituentCount { get; }

        /// <summary>
        /// Gets or sets a value representing the endpoint address of the current election service.
        /// </summary>
        EndpointAddress ElectionIdentity { get; set; }

        /// <summary>
        /// This is called when a new process starts to manually add an election endpoint to a list of 
        /// service addresses stored for when an election is required.
        /// </summary>
        /// <param name="electionServiceAddress">The endpoint address in String format</param>
        [OperationContract]
        void Introduce(string electionServiceAddress);

        /// <summary>
        /// Is called every x seconds by the coordinator to confirm to all processes that it is still active
        /// </summary>
        [OperationContract]
        void HeartbeatFromCoordinator(string endpointAddress);

        /// <summary>
        /// Is called when a process volunteers itself as the new coordinator
        /// </summary>
        /// <returns>True if the process agrees with the nomination</returns>
        [OperationContract]
        bool Nomination(string address);

        /// <summary>
        /// This is called by a process which has received a nomination declaration from a process
        /// which it believes not to be the correct process to become the new coordinator.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Elect();

        /// <summary>
        /// Populates a local list of all IElection implementations found using Service Discovery
        /// </summary>
        void FindConstituents();

        /// <summary>
        /// Gets the local list of IElection implementations
        /// </summary>
        /// <returns></returns>
        IEnumerable<EndpointAddress> GetConstituents();

        /// <summary>
        /// Indicates that this should start the appropriate Coordinator behaviour
        /// </summary>
        void SetCoordinator();

        /// <summary>
        /// Indicates that this should start the appropriate Process behaviour
        /// </summary>
        void SetProcess();

        /// <summary>
        /// Stops all threads running within the Election service
        /// </summary>
        void Stop();
    }
}
