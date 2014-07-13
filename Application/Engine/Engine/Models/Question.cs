using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Engine.Models
{
    [DataContract]
    public class Question
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Area { get; set; }

        [DataMember]
        public int Type { get; set; }
    }
}