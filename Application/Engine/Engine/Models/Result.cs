using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Engine.Models
{
    [DataContract]
    public class Result
    {
        public Result()
        {

        }

        [DataMember]
        public Datapoint Focus { get; set; }

        [DataMember]
        public IEnumerable<Datapoint> Related { get; set; }

        [DataMember]
        public double ExpiresIn { get; set; }
    }
}