using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using System.Timers;
using System.Diagnostics;

namespace MyNewService
{
    class APIHandler
    {
        public APIHandler(string url, string tenant, EventLog eventLog1, EventLog eventLog2, int eventId1, int eventId2)
        {
            this.orchestratorURL = url;
            this.tenant = tenant;
            this.eventLog1 = eventLog1;
            this.eventId1 = eventId1;
            this.eventId2 = eventId2;
            this.eventLog2 = eventLog2;
            eventLog1.WriteEntry("Building API Handler Object", EventLogEntryType.Information, eventId1++);
            Authenticate();
            RetrieveQueues();
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            eventLog1.WriteEntry("Timer Elapsed.", EventLogEntryType.Information, eventId1++);
            RetrieveQueues();
        }
        public void Refresh()
        {
            eventLog1.WriteEntry("Refresh required to update Authentication Bearer token.", EventLogEntryType.Warning, eventId1++);
            Authenticate();
            RetrieveQueues();
        }
        private void Authenticate()
        {
            string endpoint = this.orchestratorURL + "/api/Account/Authenticate";
            string jsonPayload = "{\"tenancyName\":\"" + this.tenant + "\"," +
                    "\"usernameOrEmailAddress\":\"" + Environment.GetEnvironmentVariable("Orchestrator_User") + "\"," +
                    "\"password\":\"" + Environment.GetEnvironmentVariable("Orchestrator_Password") + "\"}";

            string response = POSTRequest(jsonPayload, endpoint);

            this.authResponse = JsonConvert.DeserializeObject<Auth>(response);
            this.bearer = authResponse.result.ToString();
            eventLog1.WriteEntry("Authorization Bearer received, Authenticated successfully.", EventLogEntryType.Information, eventId1++);
        }
        private void RetrieveQueues()
        {
            eventLog2.WriteEntry("Establishing Queue Definitions.", EventLogEntryType.Information, eventId2++);
            try
            {
                string endpoint = this.orchestratorURL + "/odata/QueueDefinitions";
                string response = GETRequest(endpoint);
                if(response.Contains("You are not authenticated!"))
                {
                    throw (new System.Exception("Not authenticated, calling refresh method to authenticate the user account."));
                }
                this.QID = JsonConvert.DeserializeObject<QueueDefinitions>(response);
                Dictionary<int, string> queueDict = new Dictionary<int, string>();
                foreach (QueueDefinitions.Value item in QID.value)
                {
                    queueDict.Add(item.Id, item.Name);
                }
                this.queues = queueDict;
                string toLog = "-- Queue items received via Orchestrator API --";
                foreach (KeyValuePair<int, string> item in this.queues)
                {
                    toLog += "\n" + "[" + item.Key.ToString() + "] Queue Name: " + item.Value.ToString();
                }
                eventLog2.WriteEntry(toLog, EventLogEntryType.Information, eventId2++);
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("HTTP Request failed, attempting to Refresh Authentication.\n" + e.Message.ToString(), EventLogEntryType.Warning, eventId1++);
                Refresh();
            }
        }
        private string GETRequest(string endpoint)
        {
            string response = String.Empty;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers["Authorization"] = "Bearer " + authResponse.result.ToString();
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            return response;
        }
        private string POSTRequest(string jsonPayload, string endpoint)
        {
            string response = String.Empty;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonPayload);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            return response;
        }
        public EventLog eventLog1 { get; set; }

        private int eventId1;
        private int eventId2;

        public EventLog eventLog2 { get; set; }
        public Dictionary<int,string> queues { get; set; }
        public string bearer { get; set; }
        public string tenant { get; set; }
        public string orchestratorURL { get; set; }
        public Auth authResponse { get; set; }
        public QueueDefinitions QID { get; set; }
    }
}
