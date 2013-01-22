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

            Console.WriteLine("Searching for: {0}", criteria.ContractTypeNames[0]);

            if (scope != null)
            {
                criteria.Scopes.Add(scope);
            }

            FindResponse discovered = discoveryClient.Find(criteria);
            discoveryClient.Close();

            Console.WriteLine("finished Searching");

            return discovered.Endpoints.Select((endpoint) => endpoint.Address).ToArray();
        }
    }
}
