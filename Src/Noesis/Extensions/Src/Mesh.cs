////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Defines a shape described with a mesh of vertices and indices.
    ///
    /// Geometry is directly sent to the GPU without extra preprocessing, no tessellation, making
    /// this class ideal when rendering dynamic geometry that changes per frame.
    /// </summary>
    public class MeshData : Animatable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new MeshData();
        }
    }

    /// <summary>
    /// The Mesh element allows drawing custom geometric shapes defined by a mesh of vertices.
    /// </summary>
    public class Mesh : FrameworkElement
    {
        public MeshData Data
        {
            get { return (MeshData)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(MeshData), typeof(Mesh), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender));

        public System.Windows.Media.Brush Brush
        {
            get { return (System.Windows.Media.Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(System.Windows.Media.Brush), typeof(Mesh), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Brush != null)
            {
                drawingContext.DrawRectangle(Brush, null, new Rect(RenderSize));
            }
        }
    }
}
