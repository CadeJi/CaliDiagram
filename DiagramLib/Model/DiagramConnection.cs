﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DiagramLib.Model
{
    /// <summary>
    /// Diagram connection
    /// </summary>
    [DataContract]
    public class DiagramConnection
    {
        public DiagramConnection()
        {
            
        }
        [DataMember]
        public DiagramNodeBase From { get; set; }
        [DataMember]
        public DiagramNodeBase To { get; set; }
    }
}
