using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace MyNewService
{
    public partial class MyNewService : ServiceBase
    {
        private int eventId1 = 1;
        private int eventId2 = 1;
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        public MyNewService(string[] args)
        {
            InitializeComponent();
            
            string eventSourceName = "MySource";
            string logName = "MyNewLog";
            
            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }
            
            if (args.Length > 1)
            {
                logName = args[1];
            }
            
            eventLog1 = new EventLog();
            
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;

            eventLog2 = new EventLog();

            if (!EventLog.SourceExists("API Handler"))
            {
                EventLog.CreateEventSource("API Handler", "Queue Definitions");
            }

            eventLog2.Source = "API Handler";
            eventLog2.Log = "Queue Definitions";
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Update Event Log
            eventLog1.WriteEntry("Service Starting.");


            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);


            string url = Environment.GetEnvironmentVariable("Orchestrator_URL");
            string tenant = "default";
            APIHandler core = new APIHandler(url, tenant, eventLog1, eventLog2, eventId1, eventId2);
            eventLog1.WriteEntry("API Handler Object Created", EventLogEntryType.Information, eventId1++);

            //foreach(KeyValuePair<int,string> item in core.queues)
            //{
            //    eventLog1.WriteEntry("[" + item.Key.ToString() + "]Queue Name: " + item.Value.ToString());
            //}

            // Set up a timer that triggers every minute.
            //Timer timer = new Timer();
            //timer.Interval = 60000; // 60 seconds
            //timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            //timer.Start();

        }
        //public void OnTimer(object sender, ElapsedEventArgs args)
        //{
        //    // TODO: Insert monitoring activities here.
        //    eventLog1.WriteEntry("Pinging Orchestrator API", EventLogEntryType.Information, eventId++);
        //}

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Update Event Log
            eventLog1.WriteEntry("Service Stopping.", EventLogEntryType.Information, eventId1++);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("Service Continuing.", EventLogEntryType.Information, eventId1++);
        }
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

    }
}
