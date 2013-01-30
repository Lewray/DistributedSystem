using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

using ProcessContracts;

namespace Processes
{
    class ChildProcess : IChildProcess, ITask
    {
        private readonly object _statusLock = new object();
        
        private static EndpointAddress _coordinatorAddress;

        private ServiceHost _processHost;

        private static Queue<Task> _tasks = new Queue<Task>();

        private static Dictionary<int, TaskStatus> _taskStatuses = new Dictionary<int, TaskStatus>();
                
        private Thread _worker;

        private bool _shutDown = false;

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

            _worker = new Thread(new ThreadStart(DoWork));

            _worker.Start();
        }

        public void Stop()
        {
            if (_processHost != null)
            {
                try
                {
                    _shutDown = true;
                    _processHost.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region ITask Members

        public TaskStatus AcceptTask(int num1, int num2, string op)
        {
            string taskData = num1.ToString() + "," + op + "," + num2.ToString();

            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, _coordinatorAddress);

            int taskId = proxy.StartNewTask(taskData);

            TaskStatus ts = new TaskStatus();

            ts.TaskId = taskId;

            ts.Complete = false;
            ts.Successful = false;
            ts.ResultMessage = "Task started...";

            return ts;
        }

        public TaskStatus CheckProgress(int taskId)
        {
            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, _coordinatorAddress);

            return proxy.CheckTaskProgress(taskId);
        }

        #endregion

        #region IChildProcess Members

        public void Introduce(string coordinatorServiceAddress)
        {
            Console.WriteLine("New Coordinator address received: {0}", coordinatorServiceAddress);
            Console.WriteLine();
            _coordinatorAddress = new EndpointAddress(coordinatorServiceAddress);
        }

        public void DoTask(Task task)
        {
            _tasks.Enqueue(task);

            lock (_statusLock)
            {
                _taskStatuses.Add(task.TaskId, new TaskStatus() { TaskId = task.TaskId, Complete = false, Successful = false, ResultMessage = "Task Queued..." });
            }
        }

        public void FindCoordinator()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<ICoordinator>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine("Coordinator found at: {0}", address);
            }

            if (addresses.Count() <= 0)
            {
                throw new Exception("No Coordinator found");
            }

            _coordinatorAddress = addresses[0];
        }

        public TaskStatus CheckTaskProgress(int taskId)
        {
            TaskStatus status;

            lock (_statusLock)
            {
                status = _taskStatuses[taskId];
            }

            return status;
        }

        #endregion

        private void IntroduceToCoordinator()
        {
            Console.WriteLine("Introducing to: {0}", _coordinatorAddress);
            Console.WriteLine();

            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, _coordinatorAddress);

            proxy.Introduce(this._processHost.Description.Endpoints[1].Address.ToString());            
        }

        private void DoWork()
        {
            while (!_shutDown)
            {
                Thread.Sleep(1000);

                if (_tasks.Count > 0)
                {
                    DoNextTask(_tasks.Dequeue());
                }
            }
        }

        private void DoNextTask(Task task)
        {
            Console.WriteLine("Starting Task ID: {0}...", task.TaskId);

            int operand1, operand2, result;
            string op;
            string[] chunks = task.TaskData.Split(new[] { ',' });

            operand1 = int.Parse(chunks[0]);
            op = chunks[1];
            operand2 = int.Parse(chunks[2]);
            
            switch (op)
            {
                case "+":

                    result = operand1 + operand2;

                    break;
                case "-":

                    result = operand1 - operand2;

                    break;
                case "*":

                    result = operand1 * operand2;

                    break;
                case "/":

                    result = operand1 / operand2;

                    break;
                default:
                    throw new Exception("Operator not recognised");
            }

            TaskStatus status = new TaskStatus() { Successful = true, Complete = true, ResultMessage = string.Format("{0}: The result of {1} is {2}", _processHost.Description.Endpoints[1].Address.ToString(), (operand1.ToString() + " " + op + " " + operand2.ToString()), result) };

            lock (_statusLock)
            {
                _taskStatuses[task.TaskId] = status;
            }

            Thread.Sleep(1000);

            Console.WriteLine("Task completed");
            Console.WriteLine();
        }        
    }
}
