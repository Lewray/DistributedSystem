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
        int ConstituentCount { get; }

        // this is used to manually add an election endpoint to a processes internal list
        [OperationContract]
        void Introduce(string electionServiceAddress);

        void FindConstituents();

        IEnumerable<EndpointAddress> GetConstituents();
    }
}
