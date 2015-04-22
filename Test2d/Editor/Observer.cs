﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//#define VERBOSE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2d
{
    public class Observer
    {
        private readonly Editor _editor;
        private readonly Action _invalidateStyles;
        private readonly Action _invalidateLayers;
        private readonly Action _invalidateShapes;

        public Observer(Editor editor)
        {
            _editor = editor;

            _invalidateStyles = () =>
            {
                _editor.Renderer.ClearCache();
                _editor.Container.Invalidate();
            };

            _invalidateLayers = () =>
            {
                _editor.Container.Invalidate();
            };

            _invalidateShapes = () =>
            {
                _editor.Container.Invalidate();
            };

            InitializeStyles(_editor.Container);
            InitializeLayers(_editor.Container);
            //Add(_editor.Container.TemplateLayer);
            //Add(_editor.Container.WorkingLayer);
        }

        #region Debug

        [System.Diagnostics.Conditional("VERBOSE")]
        private void Debug(string text)
        {
            System.Diagnostics.Debug.Print(text);
        } 

        #endregion

        #region Handlers

        private void StylesCollectionObserver(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems.Cast<ShapeStyle>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug("Styles Replace");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems.Cast<ShapeStyle>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug("Styles Replace");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug("Styles Reset");
                    break;
            }

            _invalidateStyles();
        }

        private void LayersCollectionObserver(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems.Cast<Layer>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug("Layers Replace");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems.Cast<Layer>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug("Layers Replace");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug("Layers Reset");
                    break;
            }

            _invalidateLayers();
        }

        private void ShapesCollectionObserver(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems.Cast<BaseShape>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug("Shapes Replace");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems.Cast<BaseShape>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug("Shapes Replace");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug("Shapes Reset");
                    break;
            }

            _invalidateShapes();
        }

        private void StyleObserver(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug(
                "Style: " + 
                (sender is ShapeStyle ? (sender as ShapeStyle).Name : sender.GetType().ToString()) + 
                ", Property: " + 
                e.PropertyName);
            _invalidateStyles();
        }

        private void LayerObserver(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug(
                "Layer: " + 
                (sender is Layer ? (sender as Layer).Name : sender.GetType().ToString()) + 
                ", Property: " + 
                e.PropertyName);
            _invalidateLayers();
        }

        private void ShapeObserver(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug(
                "Shape: " + 
                sender.GetType() + 
                ", Property: " + 
                e.PropertyName);
            _invalidateShapes();
        }

        #endregion

        #region Styles

        private void InitializeStyles(Container container)
        {
            Add(container.Styles);

            (container.Styles as ObservableCollection<ShapeStyle>)
                .CollectionChanged += StylesCollectionObserver;
        }

        private void Add(ShapeStyle style)
        {
            style.PropertyChanged += StyleObserver;
            style.Stroke.PropertyChanged += StyleObserver;
            style.Fill.PropertyChanged += StyleObserver;
            style.LineStyle.PropertyChanged += StyleObserver;
            style.LineStyle.StartArrowStyle.PropertyChanged += StyleObserver;
            style.LineStyle.EndArrowStyle.PropertyChanged += StyleObserver;
            style.TextStyle.PropertyChanged += StyleObserver;
            Debug("Add Style: " + style.Name);
        }

        private void Remove(ShapeStyle style)
        {
            style.PropertyChanged -= StyleObserver;
            style.Stroke.PropertyChanged -= StyleObserver;
            style.Fill.PropertyChanged -= StyleObserver;
            style.LineStyle.PropertyChanged -= StyleObserver;
            style.LineStyle.StartArrowStyle.PropertyChanged -= StyleObserver;
            style.LineStyle.EndArrowStyle.PropertyChanged -= StyleObserver;
            style.TextStyle.PropertyChanged -= StyleObserver;
            Debug("Remove Style: " + style.Name);
        }

        private void Add(IEnumerable<ShapeStyle> styles)
        {
            foreach (var style in styles)
            {
                Add(style);
            }
        }

        private void Remove(IEnumerable<ShapeStyle> styles)
        {
            foreach (var style in styles)
            {
                Remove(style);
            }
        }

        #endregion

        #region Layers

        private void InitializeLayers(Container container)
        {
            Add(container.Layers);

            (container.Layers as ObservableCollection<Layer>)
                .CollectionChanged += LayersCollectionObserver;
        }

        private void Add(Layer layer)
        {
            layer.PropertyChanged += LayerObserver;
            Debug("Add Layer: " + layer.Name);
            InitializeShapes(layer);
        }

        private void Remove(Layer layer)
        {
            layer.PropertyChanged -= LayerObserver;
            Debug("Remove Layer: " + layer.Name);
            Remove(layer.Shapes);
        }

        private void Add(IEnumerable<Layer> layers)
        {
            foreach (var layer in layers)
            {
                Add(layer);
            }
        }

        private void Remove(IEnumerable<Layer> layers)
        {
            foreach (var layer in layers)
            {
                Remove(layer);
            }
        }

        #endregion

        #region Shapes

        private void InitializeShapes(Layer layer)
        {
            Add(layer.Shapes);

            (layer.Shapes as ObservableCollection<BaseShape>)
                .CollectionChanged += ShapesCollectionObserver;
        }

        private void Add(BaseShape shape)
        {
            shape.PropertyChanged += ShapeObserver;

            if (shape is XPoint)
            {
                var point = shape as XPoint;
                point.Shape.PropertyChanged += ShapeObserver;
            }
            else if (shape is XLine)
            {
                var line = shape as XLine;
                line.Start.PropertyChanged += ShapeObserver;
                line.End.PropertyChanged += ShapeObserver;
            }
            else if (shape is XRectangle)
            {
                var rectangle = shape as XRectangle;
                rectangle.TopLeft.PropertyChanged += ShapeObserver;
                rectangle.BottomRight.PropertyChanged += ShapeObserver;
            }
            else if (shape is XEllipse)
            {
                var ellipse = shape as XEllipse;
                ellipse.TopLeft.PropertyChanged += ShapeObserver;
                ellipse.BottomRight.PropertyChanged += ShapeObserver;
            }
            else if (shape is XArc)
            {
                var arc = shape as XArc;
                arc.Point1.PropertyChanged += ShapeObserver;
                arc.Point2.PropertyChanged += ShapeObserver;
            }
            else if (shape is XBezier)
            {
                var bezier = shape as XBezier;
                bezier.Point1.PropertyChanged += ShapeObserver;
                bezier.Point2.PropertyChanged += ShapeObserver;
                bezier.Point3.PropertyChanged += ShapeObserver;
                bezier.Point4.PropertyChanged += ShapeObserver;
            }
            else if (shape is XQBezier)
            {
                var qbezier = shape as XQBezier;
                qbezier.Point1.PropertyChanged += ShapeObserver;
                qbezier.Point2.PropertyChanged += ShapeObserver;
                qbezier.Point3.PropertyChanged += ShapeObserver;
            }
            else if (shape is XText)
            {
                var text = shape as XText;
                text.TopLeft.PropertyChanged += ShapeObserver;
                text.BottomRight.PropertyChanged += ShapeObserver;
            }
            else if (shape is XGroup)
            {
                var group = shape as XGroup;
                Add(group.Shapes);
                (group.Shapes as ObservableCollection<BaseShape>)
                    .CollectionChanged += ShapesCollectionObserver;
            }

            Debug("Add Shape: " + shape.GetType());
        }

        private void Remove(BaseShape shape)
        {
            shape.PropertyChanged -= ShapeObserver;

            if (shape is XPoint)
            {
                var point = shape as XPoint;
                point.Shape.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XLine)
            {
                var line = shape as XLine;
                line.Start.PropertyChanged -= ShapeObserver;
                line.End.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XRectangle)
            {
                var rectangle = shape as XRectangle;
                rectangle.TopLeft.PropertyChanged -= ShapeObserver;
                rectangle.BottomRight.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XEllipse)
            {
                var ellipse = shape as XEllipse;
                ellipse.TopLeft.PropertyChanged -= ShapeObserver;
                ellipse.BottomRight.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XArc)
            {
                var arc = shape as XArc;
                arc.Point1.PropertyChanged -= ShapeObserver;
                arc.Point2.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XBezier)
            {
                var bezier = shape as XBezier;
                bezier.Point1.PropertyChanged -= ShapeObserver;
                bezier.Point2.PropertyChanged -= ShapeObserver;
                bezier.Point3.PropertyChanged -= ShapeObserver;
                bezier.Point4.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XQBezier)
            {
                var qbezier = shape as XQBezier;
                qbezier.Point1.PropertyChanged -= ShapeObserver;
                qbezier.Point2.PropertyChanged -= ShapeObserver;
                qbezier.Point3.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XText)
            {
                var text = shape as XText;
                text.TopLeft.PropertyChanged -= ShapeObserver;
                text.BottomRight.PropertyChanged -= ShapeObserver;
            }
            else if (shape is XGroup)
            {
                var group = shape as XGroup;
                Remove(group.Shapes);
                (group.Shapes as ObservableCollection<BaseShape>)
                    .CollectionChanged -= ShapesCollectionObserver;
            }

            Debug("Remove Shape: " + shape.GetType());
        }

        private void Add(IEnumerable<BaseShape> shapes)
        {
            foreach (var shape in shapes)
            {
                Add(shape);
            }
        }

        private void Remove(IEnumerable<BaseShape> shapes)
        {
            foreach (var shape in shapes)
            {
                Remove(shape);
            }
        }

        #endregion
    }
}
