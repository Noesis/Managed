////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Base class for custom brushes.
    /// </summary>
    public abstract class BrushShader : Animatable
    {
        protected override Freezable CreateInstanceCore()
        {
            return (Freezable)Activator.CreateInstance(GetType());
        }
    }

    /// <summary>
    /// Creates a gradient with color transitions rotated around a center point.
    /// </summary>
    [ContentProperty("GradientStops")]
    public class ConicGradientBrushShader : BrushShader
    {
        public ConicGradientBrushShader()
        {
            GradientStops = new GradientStopCollection();
        }

        #region GradientStops
        public GradientStopCollection GradientStops
        {
            get { return (GradientStopCollection)GetValue(GradientStopsProperty); }
            set { SetValue(GradientStopsProperty, value); }
        }

        public static readonly DependencyProperty GradientStopsProperty = DependencyProperty.Register(
            "GradientStops", typeof(GradientStopCollection), typeof(ConicGradientBrushShader),
            new PropertyMetadata(null));
        #endregion
    }
    public class ConicGradientBrush : ConicGradientBrushShader { }

    /// <summary>
    /// Turns an image into a monochrome color.
    /// </summary>
    public class MonochromeBrushShader : BrushShader
    {
        #region Color
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(MonochromeBrushShader),
            new PropertyMetadata(Colors.White));
        #endregion
    }
    public class MonochromeBrush : MonochromeBrushShader { }

    /// <summary>
    /// Interpolates between two images.
    /// </summary>
    public class CrossFadeBrush : BrushShader
    {
        #region Weight
        public float Weight
        {
            get { return (float)GetValue(WeightProperty); }
            set { SetValue(WeightProperty, value); }
        }

        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register(
            "Weight", typeof(float), typeof(CrossFadeBrush),
            new PropertyMetadata(0.0f));
        #endregion

        #region Source
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(CrossFadeBrush),
            new PropertyMetadata(null));
        #endregion
    }

    /// <summary>
    /// Old school plasma effect.
    /// </summary>
    public class PlasmaBrushShader : BrushShader
    {
        #region Time
        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time", typeof(double), typeof(PlasmaBrushShader),
            new PropertyMetadata(0.0));
        #endregion

        #region Size
        public Point Size
        {
            get { return (Point)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(Point), typeof(PlasmaBrushShader),
            new PropertyMetadata(new Point(15, 15)));
        #endregion

        #region Scale
        public Point3D Scale
        {
            get { return (Point3D)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(Point3D), typeof(PlasmaBrushShader),
            new PropertyMetadata(new Point3D(0.5, 0.5, 0.5)));
        #endregion

        #region Bias
        public Point3D Bias
        {
            get { return (Point3D)GetValue(BiasProperty); }
            set { SetValue(BiasProperty, value); }
        }

        public static readonly DependencyProperty BiasProperty = DependencyProperty.Register(
            "Bias", typeof(Point3D), typeof(PlasmaBrushShader),
            new PropertyMetadata(new Point3D(0.5, 0.5, 0.5)));
        #endregion

        #region Frequency
        public Point3D Frequency
        {
            get { return (Point3D)GetValue(FrequencyProperty); }
            set { SetValue(FrequencyProperty, value); }
        }

        public static readonly DependencyProperty FrequencyProperty = DependencyProperty.Register(
            "Frequency", typeof(Point3D), typeof(PlasmaBrushShader),
            new PropertyMetadata(new Point3D(1, 1, 1)));
        #endregion

        #region Phase
        public Point3D Phase
        {
            get { return (Point3D)GetValue(PhaseProperty); }
            set { SetValue(PhaseProperty, value); }
        }

        public static readonly DependencyProperty PhaseProperty = DependencyProperty.Register(
            "Phase", typeof(Point3D), typeof(PlasmaBrushShader),
            new PropertyMetadata(new Point3D(0, 0, 0)));
        #endregion
    }
    public class PlasmaBrush : PlasmaBrushShader { }

    /// <summary>
    /// Paints a PSP wavy background effect.
    /// </summary>
    public class WavesBrushShader : BrushShader
    {
        #region Time
        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time", typeof(double), typeof(WavesBrushShader),
            new PropertyMetadata(0.0));
        #endregion
    }
    public class WavesBrush : WavesBrushShader { }

    /// <summary>
    /// Allows creating custom brushes based on a pixel shader by extending an ImageBrush.
    ///
    /// Usage:
    ///
    ///    <Grid
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions"
    ///      xmlns:local="clr-namespace:CustomBrushes">
    ///
    ///      <Rectangle Width="300" Height="300">
    ///        <Rectangle.Fill>
    ///          <ImageBrush ImageSource="Images/image.jpg">
    ///            <noesis:Brush.Shader>
    ///              <local:WaveBrushShader Frequency="0.7" />
    ///            </noesis:Brush.Shader>
    ///          </ImageBrush>
    ///        </Rectangle.Fill>
    ///      </Rectangle>
    ///
    ///    </Grid>
    ///
    /// </summary>
    public static class Brush
    {
        /// <summary>
        /// Gets or sets brush shader.
        /// </summary>
        #region Shader attached property

        public static readonly DependencyProperty ShaderProperty = DependencyProperty.RegisterAttached(
            "Shader", typeof(BrushShader), typeof(Brush),
            new FrameworkPropertyMetadata(null));

        public static void SetShader(ImageBrush brush, BrushShader value)
        {
            brush.SetValue(ShaderProperty, value);
        }

        public static BrushShader GetShader(ImageBrush brush)
        {
            return (BrushShader)brush.GetValue(ShaderProperty);
        }

        #endregion
    }
}
