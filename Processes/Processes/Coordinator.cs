using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using ProcessContracts;

namespace Processes
{
    class Coordinator : ICoordinator, ITask
    {
        private static Dictionary<int, Task> _tasks = new Dictionary<int, Task>();

        private static Queue<EndpointAddress> _processes = new Queue<EndpointAddress>();

        private static object _processLock = new object();

        private static Dictionary<EndpointAddress, int> _taskCount = new Dictionary<EndpointAddress, int>();

        ServiceHost _coordinatorHost;

        private static DateTime _startTime;

        private static int _nextTaskId = 0;

        private static bool _shutDown = false;

        private static CancellationTokenSource _cancelSource = new CancellationTokenSource();

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
            this.StartCoordinator();

            _startTime = DateTime.Now;

            StartStatusServer();
        }

        public void Stop()
        {
            _shutDown = true;

            _cancelSource.Cancel();

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

        public void StartStatusServer()
        {
            Thread worker = new Thread(new ThreadStart(StartServer));

            worker.Start();
        }

        #endregion

        #region ICoordinatorService Members

        public int StartNewTask(string taskData)
        {
            Task t = new Task() { TaskId = NextTaskId, TaskData = taskData, Status = new TaskStatus() };

            // get next available process
            EndpointAddress address;
            lock (_processLock)
            {
                address = _processes.Dequeue();
            }            

            // set Task assigned process address
            t.AssignedProcessAddress = address.ToString();

            Console.WriteLine("Task ID {0} Created", t.TaskId);
            Console.WriteLine("Task assigned to {0}", t.AssignedProcessAddress);
            Console.WriteLine();

            // store in dictionary
            _tasks.Add(t.TaskId, t);

            // assign task to process
            Binding binding = new NetTcpBinding();

            IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, address);

            /*
             * Returning process to back of queue - this creates a 'round robin'
             * task allocation
             */
            lock (_processLock)
            {
                _processes.Enqueue(address);
            }

            proxy.DoTask(t);

            if (_taskCount.ContainsKey(address))
            {
                _taskCount[address] = _taskCount[address] + 1;
            }
            else
            {
                _taskCount.Add(address, 1);
            }

            return t.TaskId;
        }

        public TaskStatus CheckTaskProgress(int taskId)
        {
            Task t = _tasks[taskId];

            Binding binding = new NetTcpBinding();

            IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, new EndpointAddress(t.AssignedProcessAddress));
            
            TaskStatus status = proxy.CheckTaskProgress(taskId);

            if (status.Complete)
            {
                _tasks.Remove(taskId);
            }

            return status;
        }

        public string GetCurrentSystemStatus()
        {
            return GetStatusPage();
        }

        public void Introduce(string processServiceAddress)
        {
            Console.WriteLine("adding new process service address: {0}", processServiceAddress);

            EndpointAddress a = new EndpointAddress(processServiceAddress);

            lock (_processLock)
            {
                if (!_processes.Contains(a))
                {
                    _processes.Enqueue(a);
                }
            }

            if (!_taskCount.ContainsKey(a))
            {
                _taskCount.Add(a, 0);
            }
        }

        public void FindProcesses()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<IChildProcess>();

            _taskCount.Clear();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);

                _taskCount.Add(address, 0);
            }

            lock (_processLock)
            {
                _processes = new Queue<EndpointAddress>(addresses);
            }
        }

        public IEnumerable<System.ServiceModel.EndpointAddress> GetProcesses()
        {
            EndpointAddress[] processes;

            lock (_processLock)
            {
                processes = new EndpointAddress[_processes.Count];

                _processes.CopyTo(processes, 0);
            }            

            return processes;
        }

        #endregion

        #region ITask Members

        public TaskStatus AcceptTask(int num1, int num2, string op)
        {
            string taskData = num1.ToString() + "," + op + "," + num2.ToString();

            int taskId = this.StartNewTask(taskData);

            TaskStatus ts = new TaskStatus();

            ts.TaskId = taskId;

            ts.Complete = false;
            ts.Successful = false;
            ts.ResultMessage = "Task started...";

            return ts;
        }

        public TaskStatus CheckProgress(int taskId)
        {
            return this.CheckTaskProgress(taskId);
        }

        #endregion

        private void StartCoordinator()
        {
            Console.WriteLine("Starting local Coordinator Service...");

            Uri baseAddress = DiscoveryUtils.DiscoveryHelper.AvailableTcpBaseAddress;

            this.FindProcesses();

            _coordinatorHost = new ServiceHost(typeof(Coordinator), baseAddress);

            _coordinatorHost.AddDefaultEndpoints();

            _coordinatorHost.Open();

            IntroduceToProcesses();
        }

        private void IntroduceToProcesses()
        {
            var processes = this.GetProcesses();

            foreach (EndpointAddress address in processes)
            {
                Console.WriteLine("Introducing to: {0}", address);

                Binding binding = new NetTcpBinding();

                IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, address);

                proxy.Introduce(this._coordinatorHost.Description.Endpoints[1].Address.ToString());
            }
        }

        private void StartServer()
        {
            HttpListener listener = new HttpListener();

            int port = DiscoveryUtils.DiscoveryHelper.FindAvailablePort();

            string serverString = "http://localhost:" + port + "/";

            listener.Prefixes.Add(serverString);

            Console.WriteLine("Starting status server at: {0}", serverString);

            listener.Start();

            while (!_shutDown)
            {
                try
                {
                    System.Threading.Tasks.Task<HttpListenerContext> t = listener.GetContextAsync();

                    CancellationToken token = _cancelSource.Token;

                    t.Wait(token);

                    if (!t.IsCanceled)
                    {
                        HttpListenerContext context = t.Result;

                        HttpListenerResponse response = context.Response;

                        string responseString = GetStatusPage();

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                        response.ContentLength64 = buffer.Length;

                        using (System.IO.Stream output = response.OutputStream)
                        {
                            output.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            listener.Stop();
        }

        private string GetStatusPage()
        {
            string statusPage = System.IO.File.ReadAllText("../../../status.html");

            statusPage = Regex.Replace(statusPage, "@uptime", GetUptime());

            statusPage = Regex.Replace(statusPage, "@taskCount", GetTotalTasks());

            statusPage = Regex.Replace(statusPage, "@activetasks", GetCurrentTasksTable());

            statusPage = Regex.Replace(statusPage, "@processtable", GetActiveProcessTable());

            return statusPage;
        }

        private string GetActiveProcessTable()
        {
            StringBuilder b = new StringBuilder();

            b.Append("<table border=\"1\">");
            b.Append("<tr>");
            b.Append("<th>");
            b.Append("Process Address");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("Tasks Processed");
            b.Append("</th>");
            b.Append("</tr>");

            foreach (KeyValuePair<EndpointAddress, int> pair in _taskCount)
            {
                b.Append("<tr>");

                b.Append("<td>");

                b.Append(pair.Key.ToString());

                b.Append("</td>");

                b.Append("<td>");

                b.Append(pair.Value.ToString());

                b.Append("</td>");

                b.Append("</tr>");
            }

            b.Append("</table>");

            return b.ToString();
        }

        private string GetCurrentTasksTable()
        {
            Task[] tasks = new Task[_tasks.Count];

            _tasks.Values.CopyTo(tasks, 0);

            StringBuilder b = new StringBuilder();

            b.Append("<table border=\"1\">");
            b.Append("<tr>");
            b.Append("<th>");
            b.Append("Task ID");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("Start Time");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("End Time");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("Process Address");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("Data");
            b.Append("</th>");
            b.Append("<th>");
            b.Append("Result");
            b.Append("</th>");
            b.Append("</tr>");
            
            foreach (Task t in tasks)
            {
                b.Append("<tr>");

                b.Append("<td>");

                b.Append(t.TaskId.ToString());

                b.Append("</td>");

                b.Append("<td>");

                b.Append(t.StartTime.ToString());

                b.Append("</td>");

                b.Append("<td>");

                b.Append(t.EndTime.ToString());

                b.Append("</td>");

                b.Append("<td>");

                b.Append(t.AssignedProcessAddress);

                b.Append("</td>");

                b.Append("<td>");

                b.Append(t.TaskData);

                b.Append("</td>");

                b.Append("<td>");

                b.Append(t.Status.ResultMessage);

                b.Append("</td>");

                b.Append("</tr>");
            }

            b.Append("</table>");

            return b.ToString();
        }

        private string GetUptime()
        {
            TimeSpan uptime = DateTime.Now.Subtract(_startTime);

            return uptime.ToString();
        }

        private string GetTotalTasks()
        {
            string totalTasks = _nextTaskId.ToString();

            return totalTasks;
        }
    }
}
