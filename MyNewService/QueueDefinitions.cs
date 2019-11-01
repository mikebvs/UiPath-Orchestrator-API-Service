using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNewService
{
    class QueueDefinitions
    {
        public string odataContext { get; set; }
        public int odataCount { get; set; }
        public List<Value> value { get; set; }
        public class Value
        {
            public QueueResponse queueResponse { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int MaxNumberOfRetries { get; set; }
            public bool AcceptAutomaticallyRetry { get; set; }
            public bool EnforceUniqueReference { get; set; }
            public DateTime CreationTime { get; set; }
            public int Id { get; set; }
        }
    }
}
