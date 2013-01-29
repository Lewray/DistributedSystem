using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using DiscoveryUtils;

using ElectionContracts;
using ElectionServices;

using ProcessContracts;

namespace Processes
{
    public class Process
    {
        IElection _election;

        ServiceHost _electionHost;

        IChildProcess _childProcess;

        ICoordinator _coordinator;

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

        #region Election Methods

        private void StartElectionService()
        {
            Console.WriteLine("Starting local election service...");

            Uri baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;

            _election = new Election();

            _election.Elected += ElectionElected;

            _election.FindConstituents();

            _electionHost = new ServiceHost(typeof(Election), baseAddress);

            _electionHost.AddDefaultEndpoints();

            _electionHost.Open();

            _election.ElectionIdentity = _electionHost.Description.Endpoints[1].Address;

            Console.WriteLine("Election Service Started at: {0}", _electionHost.Description.Endpoints[1].Address.ToString());
            Console.WriteLine();

            IntroduceElection();
        }

        void ElectionElected(object sender, EventArgs e)
        {
            StopProcess(_childProcess);

            StartCoordinator();            
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

            Console.WriteLine();
        }

        private void StopElectionService()
        {
            _election.Stop();

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

            _election.SetProcess();
        }

        private void StartCoordinator()
        {
            _coordinator = new Coordinator();

            _coordinator.Start();

            Console.WriteLine("I am the new Coordinator!");

            _election.SetCoordinator();
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
