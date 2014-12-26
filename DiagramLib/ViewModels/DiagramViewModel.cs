﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Windows.Input;
using System.Windows;

namespace DiagramLib.ViewModels
{
    public class DiagramViewModel: PropertyChangedBase
    {
        public DiagramViewModel()
        {
            DiagramItems = new ObservableCollection<DiagramBaseViewModel>();
            Edges = new ObservableCollection<ConnectionViewModel>();
            AttachDescriptors = new ObservableCollection<DiagramBaseViewModel>();
        }
        public ObservableCollection<DiagramBaseViewModel> AttachDescriptors { get; set; } 
        public ObservableCollection<DiagramBaseViewModel> DiagramItems { get; set; }
        public ObservableCollection<ConnectionViewModel> Edges { get; set; }
        public event EventHandler<Point> OnDiagramClick;
        public event EventHandler<DiagramBaseViewModel> NodeSelected;

        public void DiagramClick(MouseButtonEventArgs args, FrameworkElement el)
        {
           var pos =  args.GetPosition(el);
            if (OnDiagramClick != null)
                OnDiagramClick(this, pos);
        }


        private DiagramBaseViewModel _SelectedNode;
        public DiagramBaseViewModel SelectedNode
        {
            get { return _SelectedNode; }
            set
            {
                if (_SelectedNode != value)
                {
                    _SelectedNode = value;
                    NotifyOfPropertyChange(() => SelectedNode);
                }
            }
        }

        

        public void SelectNode(DiagramBaseViewModel vm)
        {
            SelectedNode = vm;
            if (NodeSelected != null)
                NodeSelected(this, vm);
        }
    }
}
