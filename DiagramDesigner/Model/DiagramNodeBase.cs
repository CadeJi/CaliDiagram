﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiagramDesigner.Model
{
    [DataContract]
    public class DiagramNodeBase
    {

        [DataMember]
        public Point Location { get; set; }
    }
}
