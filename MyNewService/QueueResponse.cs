using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNewService
{
    class QueueResponse
    {
        public class SpecificContent
        {
        }

        public class Value
        {
            public int QueueDefinitionId { get; set; }
            public object OutputData { get; set; }
            public string Status { get; set; }
            public string ReviewStatus { get; set; }
            public object ReviewerUserId { get; set; }
            public string Key { get; set; }
            public string Reference { get; set; }
            public object ProcessingExceptionType { get; set; }
            public DateTime? DueDate { get; set; }
            public string Priority { get; set; }
            public object DeferDate { get; set; }
            public DateTime? StartProcessing { get; set; }
            public DateTime? EndProcessing { get; set; }
            public int SecondsInPreviousAttempts { get; set; }
            public object AncestorId { get; set; }
            public int RetryNumber { get; set; }
            public string SpecificData { get; set; }
            public DateTime CreationTime { get; set; }
            public object Progress { get; set; }
            public string RowVersion { get; set; }
            public int Id { get; set; }
            public object ProcessingException { get; set; }
            public SpecificContent SpecificContent { get; set; }
            public object Output { get; set; }
        }

        public class RootObject
        {
            public string odataContext { get; set; }
            public int odataCount { get; set; }
            public List<Value> value { get; set; }
        }
    }
}
