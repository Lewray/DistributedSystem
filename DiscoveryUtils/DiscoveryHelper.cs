using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoveryUtils
{
    /// <summary>
    /// Helper methods for initialising new services and for service discovery
    /// </summary>
    public class DiscoveryHelper
    {
        public static Uri AvailableTcpBaseAddress
        {
            get
            {
                return new Uri("net.tcp://localhost:" + FindAvailablePort() + "/");
            }
        }

        public static int FindAvailablePort()
        {
            Mutex mutex = new Mutex(false, "ServiceModelEx.DiscoveryHelper.FindAvailablePort");
            try
            {
                mutex.WaitOne();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(endPoint);
                    IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
                    return local.Port;
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public static EndpointAddress[] DiscoverAddresses<T>(Uri scope = null)
        {
            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());

            FindCriteria criteria = new FindCriteria(typeof(T));
            criteria.Duration = new TimeSpan(0, 0, 5);

            Console.WriteLine();
            Console.WriteLine("Searching for: {0}", criteria.ContractTypeNames[0]);

            if (scope != null)
            {
                criteria.Scopes.Add(scope);
            }

            FindResponse discovered = discoveryClient.Find(criteria);
            discoveryClient.Close();

            var addresses = discovered.Endpoints.Select((endpoint) => endpoint.Address).ToArray();

            Console.WriteLine("Finished Searching: {0} services found", addresses.Count());
            Console.WriteLine();

            return addresses;
        }
    }
}
