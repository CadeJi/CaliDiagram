﻿using CaliDiagram.ViewModels;
using RaftDemo.NodeSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo.Model
{
    public interface INetworkModel
    {
        INodeChannel CreateChannel(ConnectionViewModel connection, NodeBaseViewModel from);
    }
}
