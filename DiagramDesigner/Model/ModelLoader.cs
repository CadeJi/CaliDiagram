﻿using DiagramDesigner.Properties;
using DiagramLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiagramLib;
using DiagramDesigner.ViewModels;
using System.Windows;
using System.Windows.Threading;


namespace DiagramDesigner.Model
{
    class ModelLoader<TDiagramModel> where TDiagramModel: new()
    {
        DiagramViewModel diagramViewModel;
        XmlSettings<DiagramModel> xmlSettings;
        public ModelLoader(DiagramViewModel diagramViewModel)
        {
            this.diagramViewModel = diagramViewModel;
            xmlSettings = new XmlSettings<DiagramModel>(new Type[]
                {
                    typeof(DiagramNodeBig),
                    typeof(DiagramNodeSmall),
                    typeof(DiagramNodeBroker)
                });
        }

        NodeBaseViewModel ViewModelFromModel(DiagramNodeBase model)
        {
            if (model is DiagramNodeBig)
                return new DiagramNodeBigViewModel(model.Name) { Location = model.Location };
            if (model is DiagramNodeSmall)
                return new DiagramNodeSmallViewModel(model.Name) { Location = model.Location };
            if (model is DiagramNodeBroker)
                return new DiagramNodeBrokerViewModel(model.Name) { Location = model.Location };
            return null;
        }
        DiagramNodeBase ModelFromViewModel(NodeBaseViewModel viewModel)
        {
            if (viewModel is DiagramNodeSmallViewModel)
                return new DiagramNodeSmall() { Location = viewModel.Location, Name = viewModel.Name };
            if (viewModel is DiagramNodeBigViewModel)
                return new DiagramNodeBig() { Location = viewModel.Location, Name = viewModel.Name };
            if (viewModel is DiagramNodeBrokerViewModel)
                return new DiagramNodeBroker() { Location = viewModel.Location, Name = viewModel.Name };
            return null;
        }
        public void SaveDiagram(string filename)
        {
            DiagramModel diagramModel = new DiagramModel();
            Dictionary<NodeBaseViewModel, DiagramNodeBase> nodeDictionary = new Dictionary<NodeBaseViewModel, DiagramNodeBase>();
            foreach (var node in diagramViewModel.Nodes)
            {
                DiagramNodeBase diagramNode = ModelFromViewModel(node);               
                if (diagramNode != null)
                {
                    diagramModel.Nodes.Add(diagramNode);
                    nodeDictionary.Add(node, diagramNode);
                }
            }

            foreach (var edge in diagramViewModel.Edges)
            {
                diagramModel.Edges.Add(new DiagramConnection()
                {
                    From = nodeDictionary[edge.From],
                    To = nodeDictionary[edge.To]
                });
            }
            xmlSettings.SaveModel(diagramModel, filename);
        }

        public void LoadDiagram(string filename)
        {
            DiagramModel model = xmlSettings.ModelFromSettings(filename);

            diagramViewModel.ClearDiagram();

            Dictionary<DiagramNodeBase, NodeBaseViewModel> nodeDictionary = new Dictionary<DiagramNodeBase, NodeBaseViewModel>();
            foreach (var node in model.Nodes)
            {
                NodeBaseViewModel nodeViewModel = ViewModelFromModel(node);
               
                if (nodeViewModel != null)
                {
                    diagramViewModel.Nodes.Add(nodeViewModel);
                    nodeDictionary.Add(node, nodeViewModel);
                }
            }
            // Force rendering so we can have sizes of all nodes
            diagramViewModel.ForceRedraw();

            foreach (var edge in model.Edges)
            {
                diagramViewModel.AddConnection(nodeDictionary[edge.From], nodeDictionary[edge.To]);                
            }
            // Render again so we can have sizes of attach descriptors
            diagramViewModel.ForceRedraw();

            Console.WriteLine("Model loaded");

            foreach (var conn in diagramViewModel.Edges)
                conn.UpdateConnection();

            foreach (var node in diagramViewModel.AttachDescriptors)
                node.RaiseInitialize();

            foreach (var node in diagramViewModel.Nodes)
                node.RaiseInitialize();

        }
    }
}
