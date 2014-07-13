using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Engine.Models
{
    [DataContract]
    public class Datapoint
    {
        public Datapoint()
        {

        }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Country { get; set; }

        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public int Ratio { get; set; }

        [DataMember]
        public IEnumerable<Question> Questions { get; set; }

        [DataMember]
        public IEnumerable<Endplate> Endplates { get; set; }
    }
}