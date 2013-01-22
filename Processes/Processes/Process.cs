using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using DiscoveryUtils;

using ElectionContracts;
using ElectionServices;

using ProcessContracts;

namespace Processes
{
    public class Process : IProcess
    {
        IElection _election;

        ServiceHost _electionHost;

        IChildProcess _childProcess;

        ICoordinator _coordinator;

        bool _isCoordinator = false;

        #region IProcess Members

        public void Start()
        {
            StartElectionService();

            this.InitialStartProcess();
        }

        public void Stop()
        {
            StopElectionService();

            StopProcess(_coordinator);

            StopProcess(_childProcess);
        }

        #endregion

        #region Election Methods

        private void StartElectionService()
        {
            Console.WriteLine("Starting local election service...");

            Uri baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;

            _election = new Election();

            _election.FindConstituents();

            _electionHost = new ServiceHost(typeof(Election), baseAddress);

            _electionHost.AddDefaultEndpoints();

            _electionHost.Open();

            IntroduceElection();
        }
        
        private void IntroduceElection()
        {
            var constituents = _election.GetConstituents();

            foreach (EndpointAddress address in constituents)
            {
                Console.WriteLine("Introducing to: {0}", address);

                Binding binding = new NetTcpBinding();

                IElection proxy = ChannelFactory<IElection>.CreateChannel(binding, address);

                proxy.Introduce(_electionHost.Description.Endpoints[1].Address.ToString());
            }
        }

        private void StopElectionService()
        {
            _electionHost.Close();
        }

        #endregion

        #region Process Methods

        private void InitialStartProcess()
        {
            if (_election.ConstituentCount == 0)
            {
                StartCoordinator();
            }
            else
            {
                StartProcess();
            }
        }

        private void StartProcess()
        {
            _childProcess = new ChildProcess();

            _childProcess.Start();

            Console.WriteLine("I am a new Process!");
        }

        private void StartCoordinator()
        {
            _coordinator = new Coordinator();

            _coordinator.Start();

            _isCoordinator = true;

            Console.WriteLine("I am the new Coordinator!");
        }

        private void StopProcess(IProcess process)
        {
            if (process != null)
            {
                process.Stop();
            }
        }

        #endregion
    }
}
