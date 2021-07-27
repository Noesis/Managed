using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Blurs an object along a specific angle, to create a blurred streaking effect.
    /// </summary>
    public class DirectionalBlurEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public DirectionalBlurEffect() { PixelShader = _pixelShader; }

        #region Angle
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            "Angle", typeof(double), typeof(DirectionalBlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        #endregion

        #region Radius
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(DirectionalBlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));
        #endregion
    }

    /// <summary>
    /// Effect to warp or pinch a specific area in a visual.
    /// </summary>
    public class PinchEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public PinchEffect() { PixelShader = _pixelShader; }

        #region Amount
        public double Amount
        {
            get { return (double)GetValue(AmountProperty); }
            set { SetValue(AmountProperty, value); }
        }

        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
            "Amount", typeof(double), typeof(PinchEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        #endregion

        #region Center
        public Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Point), typeof(PinchEffect),
            new PropertyMetadata(new Point(0, 0), PixelShaderConstantCallback(1)));
        #endregion

        #region Radius
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(PinchEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));
        #endregion
    }

    /// <summary>
    /// Effect to warp or pinch a specific area in a visual.
    /// </summary>
    public class PixelateEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public PixelateEffect() { PixelShader = _pixelShader; }

        #region Size
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(PixelateEffect),
            new PropertyMetadata(5.0, PixelShaderConstantCallback(0)));
        #endregion
    }

    /// <summary>
    /// Use this effect to alter the saturation of an image.
    /// </summary>
    public class SaturationEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public SaturationEffect() { PixelShader = _pixelShader; }

        #region Saturation
        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            "Saturation", typeof(double), typeof(SaturationEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        #endregion
    }

    /// <summary>
    /// This effect tints the source image by multiplying the source image by the specified color.
    /// </summary>
    public class TintEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public TintEffect() { PixelShader = _pixelShader; }

        #region Color
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(TintEffect),
            new PropertyMetadata(Colors.Black, PixelShaderConstantCallback(0)));
        #endregion
    }

    /// <summary>
    /// Fades the input image at the edges to a user-set color.
    /// </summary>
    public class VignetteEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader();
        public VignetteEffect() { PixelShader = _pixelShader; }

        #region Color
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(VignetteEffect),
            new PropertyMetadata(Colors.Black, PixelShaderConstantCallback(0)));
        #endregion

        #region Size
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(VignetteEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));
        #endregion

        #region Strength
        public double Strength
        {
            get { return (double)GetValue(StrengthProperty); }
            set { SetValue(StrengthProperty, value); }
        }

        public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(
            "Strength", typeof(double), typeof(VignetteEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));
        #endregion
    }
}