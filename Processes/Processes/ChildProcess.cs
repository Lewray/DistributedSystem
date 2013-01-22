using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    class ChildProcess : IChildProcess
    {
        EndpointAddress _coordinatorAddress;

        ServiceHost _processHost;

        #region IProcess Members

        public void Start()
        {
            Console.WriteLine("Starting local child process Service...");

            Uri baseAddress = DiscoveryUtils.DiscoveryHelper.AvailableTcpBaseAddress;

            this.FindCoordinator();

            _processHost = new ServiceHost(typeof(ChildProcess), baseAddress);

            _processHost.AddDefaultEndpoints();

            _processHost.Open();

            IntroduceToCoordinator();
        }

        public void Stop()
        {
            if (_processHost != null)
            {
                try
                {
                    _processHost.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region IChildProcess Members

        public void Introduce(string coordinatorServiceAddress)
        {
            _coordinatorAddress = new EndpointAddress(coordinatorServiceAddress);
        }

        public void DoTask(Task task)
        {
            throw new NotImplementedException();
        }

        public void FindCoordinator()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<ICoordinator>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);
            }

            if (addresses.Count() <= 0)
            {
                throw new Exception("No Coordinator found");
            }

            _coordinatorAddress = addresses[0];
        }

        #endregion

        private void IntroduceToCoordinator()
        {
            Console.WriteLine("Introducing to: {0}", this._coordinatorAddress);

            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, this._coordinatorAddress);

            proxy.Introduce(this._processHost.Description.Endpoints[0].Address.ToString());            
        }

    }
}
