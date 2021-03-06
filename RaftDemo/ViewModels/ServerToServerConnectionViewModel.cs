﻿using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;
using CaliDiagram.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaftDemo.Model;


namespace RaftDemo.ViewModels
{
    class ServerToServerConnectionViewModel: ConnectionViewModel
    {
        
        public ServerToServerConnectionViewModel(NodeBaseViewModel from, NodeBaseViewModel to, SimulationSettings worldSettings) :
            base(from, to)
        {
            this.worldSettings = worldSettings;
            StrokeThickness = 5;
            Stroke = Brushes.OrangeRed;
         //   DispatcherTimer timer = new DispatcherTimer();
          //  timer.Interval = new TimeSpan(0,0,1);
          //  timer.Tick += timer_Tick;
         //   timer.Start();
            Type = EdgeLineType.Bezier;
        }
        public override double Latency
        {
            get
            {
                return worldSettings.ServerToServerLatency;
            }
        }
        SimulationSettings worldSettings;
        private int n = 0;
        void timer_Tick(object sender, EventArgs e)
        {
            n++;
            if (n%2 == 0)
            {
                Stroke = Brushes.Red;
            }
            else
            {

                Stroke = Brushes.Orange;
            }
        }
    }
}
