﻿using System.Collections.ObjectModel;
using DiagramLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace DiagramDesigner.ViewModels
{

    internal class DiagramNodeBigViewModel : NodeBaseViewModel
    {
        public DiagramNodeBigViewModel(string name)
        {
            this.Name = name;
        }
    }

}