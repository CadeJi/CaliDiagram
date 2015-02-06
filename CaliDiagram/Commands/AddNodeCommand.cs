﻿using CaliDiagram.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CaliDiagram.Commands
{
    class AddNodeCommand : DiagramCommand
    {
        NodeTypeBehaviour nodeBehaviour;
        public AddNodeCommand(DiagramViewModel diagram, string description, NodeTypeBehaviour nodeTypeBehaviour):base(diagram)
        {
            this.Description = description;
            this.nodeBehaviour = nodeTypeBehaviour;
        }
        public override string Description
        {
            get;
            set;
        }

        public override void HandleDiagramClick(Point location)
        {
            Diagram.AddNode(nodeBehaviour.CreateNode(location), location);
        }
    }
}
