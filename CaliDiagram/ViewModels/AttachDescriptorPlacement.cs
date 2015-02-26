﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace CaliDiagram.ViewModels
{
    public class AttachDescriptorPlacement
    {
        private NodeBaseViewModel parent;
        public AttachDescriptorPlacement(NodeBaseViewModel parentControl)
        {
            parent = parentControl;
            AttachPoints = new Dictionary<AttachDirection, List<AttachPoint>>();
            foreach (AttachDirection type in Enum.GetValues(typeof(AttachDirection)))
                AttachPoints.Add(type, new List<AttachPoint>());

        }

        Point GetPoint(AttachDirection dir, Rect rec)
        {
            if (dir == AttachDirection.Top)
                return new Point(rec.X + rec.Width / 2, rec.Y);
            if (dir == AttachDirection.Right)
                return new Point(rec.X + rec.Width, rec.Y + rec.Height / 2);
            if (dir == AttachDirection.Bottom)
                return new Point(rec.X + rec.Width / 2, rec.Y + rec.Height);
            if (dir == AttachDirection.Left)
                return new Point(rec.X, rec.Y + rec.Height / 2);
            throw new ArgumentException();
        }

        public Dictionary<AttachDirection, List<AttachPoint>> AttachPoints;

        void UpdateAttachPointOrder()
        {
            foreach (var direction in AttachPoints)
            {


                List<Tuple<AttachPoint, NodeBaseViewModel>> apByPositionOfConnectedTo =
                    new List<Tuple<AttachPoint, NodeBaseViewModel>>();
                foreach (var ap in direction.Value)
                {
                    ConnectionViewModel cvm = ap.Connection;
                    if (cvm != null)
                    {
                        NodeBaseViewModel connTo = (cvm.AttachPointFrom == ap) ? cvm.To : cvm.From;
                        apByPositionOfConnectedTo.Add(Tuple.Create(ap, connTo));
                    }

                }
                int n = 0;

                if (direction.Key == AttachDirection.Left || direction.Key == AttachDirection.Right)
                {
                    foreach (var tuple in apByPositionOfConnectedTo.OrderBy(a => a.Item2.Location.Y))
                        tuple.Item1.Order = n++;
                }
                else if (direction.Key == AttachDirection.Top || direction.Key == AttachDirection.Bottom)
                {
                    foreach (var tuple in apByPositionOfConnectedTo.OrderBy(a => a.Item2.Location.X))
                        tuple.Item1.Order = n++;
                }


            }
        }

        /// <summary>
        /// Updates attach points belongig to this descriptor(which belongs to entityviewmodel)
        /// When call it?
        /// -Parent control size or location changed
        /// -Any attachment view size or location changed
        /// -On create(When parent view has size, when all attachnment have size
        /// </summary>        
        public void UpdateAttachPoints()
        {
            if (parent.ParentDiagram == null)
                return;
            // update attach point ic called from external code in case of batch mode usage
            if (parent.ParentDiagram.IsInBatchMode)
                return;

          //  Console.WriteLine(parent.Name + "::UpdateAttachPoints");
            if (AttachPoints.All((kv) => kv.Value.Count == 0))
            {
                return;
            }

            
            //if (AttachPoints.Any(kv => kv.Value.Count(a => a.Control.Width == 0) > 0))
            //{
            //    return;
            //}

            UpdateAttachPointOrder();

            foreach (var directionPoints in AttachPoints)
            {
                var direction = directionPoints.Key;
                var attachPoints = directionPoints.Value;


                if (attachPoints.Any(ap => ap.Width == 0))
                {

                }

                if (attachPoints.Count == 0)
                    continue;

                int attachSpacing = 5;
                double groupWidth = attachPoints.Sum(p => p.Width) + attachSpacing * (attachPoints.Count - 1);
                double groupHeight = attachPoints.Sum(p => p.Height) + attachSpacing * (attachPoints.Count - 1);

                var middlePoint = GetPoint(direction, parent.Rect);

                double offsetX = middlePoint.X - (groupWidth / 2.0);
                double offsetY = middlePoint.Y - (groupHeight / 2.0);

                foreach (var attachPoint in attachPoints.OrderBy(a=>a.Order))
                {
                    if(attachPoint.Width == 0)
                    {
                        if(attachPoint.Control != null)
                            Trace.TraceWarning("W: width of attach point is 0: {0}", attachPoint.Control.Name);
                        else
                            Trace.TraceWarning("W: no control associated with attach point");
                    }
                    if (direction == AttachDirection.Top || direction == AttachDirection.Bottom)
                    {
                        attachPoint.Location = new Point(offsetX + (attachPoint.Width / 2), middlePoint.Y);
                        offsetX += attachPoint.Width + attachSpacing;
                    }
                    else if (direction == AttachDirection.Left || direction == AttachDirection.Right)
                    {
                        attachPoint.Location = new Point(middlePoint.X, offsetY + (attachPoint.Height / 2));
                        offsetY += attachPoint.Height + attachSpacing;
                    }

                    if (attachPoint.Control != null)
                    {
                        attachPoint.ControlLocation = DiagramHelpers.GetAttachmentLocation(attachPoint.Control,
                            attachPoint.Location, attachPoint.Side);


                        attachPoint.Control.Location = attachPoint.ControlLocation;
                    }
                }
            }
        }

        public AttachPoint Attach(AttachDirection direction, ConnectionViewModel connection, NodeBaseViewModel associatedControl)
        {
            AttachPoint attachPoint = new AttachPoint(direction, connection, associatedControl );
            attachPoint.DirectionChanging += attachPoint_DirectionChanging;
            AttachPoints[direction].Add(attachPoint);            
            UpdateAttachPoints();
            return attachPoint;
        }

        void attachPoint_DirectionChanging(AttachPoint ap, AttachDirection direction)
        {
            AttachPoints[ap.Side].Remove(ap);
            AttachPoints[direction].Add(ap);
            //UpdatePoints();
        }

        public void Detach(AttachPoint point)
        {
            AttachPoints[point.Side].Remove(point);
            point.DirectionChanging -= attachPoint_DirectionChanging;
            UpdateAttachPoints();
        }

    }
}
