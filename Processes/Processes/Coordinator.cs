using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

using ProcessContracts;

namespace Processes
{
    class Coordinator : ICoordinator
    {
        Dictionary<int, Task> _tasks = new Dictionary<int, Task>();

        List<EndpointAddress> _processes = new List<EndpointAddress>();

        ServiceHost _coordinatorHost;

        private static int _nextTaskId = 0;     

        private static int NextTaskId
        {
            get
            {
                int temp = _nextTaskId++;

                return temp;
            }
        }

        #region IProcess Members

        public void Start()
        {
            Console.WriteLine("Starting local Coordinator Service...");

            Uri baseAddress = DiscoveryUtils.DiscoveryHelper.AvailableTcpBaseAddress;
                        
            this.FindProcesses();

            _coordinatorHost = new ServiceHost(typeof(Coordinator), baseAddress);

            _coordinatorHost.AddDefaultEndpoints();

            _coordinatorHost.Open();

            IntroduceToProcesses();
        }

        public void Stop()
        {
            if (_coordinatorHost != null)
            {
                try
                {
                    _coordinatorHost.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
            }
        }

        #endregion

        #region ICoordinatorService Members

        public int StartNewTask(string taskData)
        {
            Task t = new Task() { TaskId = NextTaskId, TaskData = taskData, Status = new TaskStatus() };

            // get next available process

            // set Task assigned process address

            // assign task to process

            return -1;
        }

        public TaskStatus CheckTaskProgress(int taskId)
        {
            throw new NotImplementedException();
        }

        public SystemStatus GetCurrentSystemStatus()
        {
            throw new NotImplementedException();
        }

        public void Introduce(string processServiceAddress)
        {
            Console.WriteLine("adding new process service address: {0}", processServiceAddress);

            EndpointAddress a = new EndpointAddress(processServiceAddress);

            if (!_processes.Contains(a))
            {
                _processes.Add(a);
            }   
        }

        public int ProcessCount
        {
            get { throw new NotImplementedException(); }
        }

        public void FindProcesses()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<IProcess>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);
            }

            _processes.AddRange(addresses);
        }

        public IEnumerable<System.ServiceModel.EndpointAddress> GetProcesses()
        {
            return _processes;
        }

        #endregion

        private void IntroduceToProcesses()
        {
            var processes = this.GetProcesses();

            foreach (EndpointAddress address in _processes)
            {
                Console.WriteLine("Introducing to: {0}", address);

                Binding binding = new NetTcpBinding();

                IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, address);

                proxy.Introduce(this._coordinatorHost.Description.Endpoints[0].Address.ToString());
            }
        }
    }
}
