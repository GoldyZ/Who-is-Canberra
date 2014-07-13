using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Engine.Models
{
    [DataContract]
    public class Endplate
    {
        public Endplate()
        {

        }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Uri { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public string Credit { get; set; }
    }
}