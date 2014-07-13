using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Engine.Models
{
    [DataContract]
    public class Response
    {
        public Response()
        {

        }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public Guid Game { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string PostCode { get; set; }

        [DataMember]
        public Datapoint Expected { get; set; }

        [DataMember]
        public Datapoint Actual { get; set; }
    }
}