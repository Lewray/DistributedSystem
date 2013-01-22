using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using DiscoveryUtils;

using ElectionContracts;

namespace ElectionServices
{
    public class Election : IElection
    {
        private static List<EndpointAddress> _electionServices = new List<EndpointAddress>();

        #region IElection Members

        public int ConstituentCount
        {
            get 
            {
                return _electionServices.Count();
            }
        }

        public void Introduce(string electionServiceAddress)
        {
            Console.WriteLine("adding new election service address: {0}", electionServiceAddress);

            EndpointAddress a = new EndpointAddress(electionServiceAddress);

            if (!_electionServices.Contains(a))
            {
                _electionServices.Add(a);
            }            
        }

        public void FindConstituents()
        {
            var addresses = DiscoveryHelper.DiscoverAddresses<IElection>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);   
            }

            _electionServices.AddRange(addresses);
        }

        public IEnumerable<EndpointAddress> GetConstituents()
        {
            return _electionServices;
        }

        #endregion
    }
}
