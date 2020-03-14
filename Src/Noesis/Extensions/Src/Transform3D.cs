////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Provides a base class for all three-dimensional transformations, including translation,
    /// rotation, and scale transformations.
    /// </summary>
    public abstract class Transform3D : Animatable
    {
    }

    /// <summary>
    /// Represents 3-D scale, rotation, and translate transforms to be applied to an element.
    /// </summary>
    public class CompositeTransform3D : Transform3D
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the center of rotation of the object you rotate
        /// </summary>
        #region CenterX dependency property

        public double CenterX
        {
            get { return (double)GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the y-coordinate of the center of rotation of the object you rotate
        /// </summary>
        #region CenterY dependency property

        public double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the z-coordinate of the center of rotation of the object you rotate
        /// </summary>
        #region CenterZ dependency property

        public double CenterZ
        {
            get { return (double)GetValue(CenterZProperty); }
            set { SetValue(CenterZProperty, value); }
        }

        public static readonly DependencyProperty CenterZProperty =
            DependencyProperty.Register("CenterZ", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the number of degrees to rotate the object around the x-axis of rotation
        /// </summary>
        #region RotationX dependency property

        public double RotationX
        {
            get { return (double)GetValue(RotationXProperty); }
            set { SetValue(RotationXProperty, value); }
        }

        public static readonly DependencyProperty RotationXProperty =
            DependencyProperty.Register("RotationX", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the number of degrees to rotate the object around the y-axis of rotation
        /// </summary>
        #region RotationY dependency property

        public double RotationY
        {
            get { return (double)GetValue(RotationYProperty); }
            set { SetValue(RotationYProperty, value); }
        }

        public static readonly DependencyProperty RotationYProperty =
            DependencyProperty.Register("RotationY", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the number of degrees to rotate the object around the z-axis of rotation
        /// </summary>
        #region RotationZ dependency property

        public double RotationZ
        {
            get { return (double)GetValue(RotationZProperty); }
            set { SetValue(RotationZProperty, value); }
        }

        public static readonly DependencyProperty RotationZProperty =
            DependencyProperty.Register("RotationZ", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the x-axis scale factor
        /// </summary>
        #region ScaleX dependency property

        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register("ScaleX", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the y-axis scale factor
        /// </summary>
        #region ScaleY dependency property

        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }

        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the z-axis scale factor
        /// </summary>
        #region ScaleZ dependency property

        public double ScaleZ
        {
            get { return (double)GetValue(ScaleZProperty); }
            set { SetValue(ScaleZProperty, value); }
        }

        public static readonly DependencyProperty ScaleZProperty =
            DependencyProperty.Register("ScaleZ", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the distance to translate along the x-axis in pixels
        /// </summary>
        #region TranslateX dependency property

        public double TranslateX
        {
            get { return (double)GetValue(TranslateXProperty); }
            set { SetValue(TranslateXProperty, value); }
        }

        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register("TranslateX", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the distance to translate along the y-axis in pixels
        /// </summary>
        #region TranslateY dependency property

        public double TranslateY
        {
            get { return (double)GetValue(TranslateYProperty); }
            set { SetValue(TranslateYProperty, value); }
        }

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Gets or sets the distance to translate along the z-axis in pixels
        /// </summary>
        #region TranslateZ dependency property

        public double TranslateZ
        {
            get { return (double)GetValue(TranslateZProperty); }
            set { SetValue(TranslateZProperty, value); }
        }

        public static readonly DependencyProperty TranslateZProperty =
            DependencyProperty.Register("TranslateZ", typeof(double), typeof(CompositeTransform3D), new PropertyMetadata(0.0));

        #endregion

        #region CreateInstanceCore implementation

        protected override Freezable CreateInstanceCore()
        {
            return new CompositeTransform3D();
        }

        #endregion
    }

    /// <summary>
    /// Applies a 3D transformation matrix to an object.
    /// </summary>
    public class MatrixTransform3D : Transform3D
    {
        /// <summary>
        /// Gets or sets the 3D matrix that is used to transform the object
        /// </summary>
        #region Matrix dependency property

        public Matrix3D Matrix
        {
            get { return (Matrix3D)GetValue(MatrixProperty); }
            set { SetValue(MatrixProperty, value); }
        }

        public static readonly DependencyProperty MatrixProperty =
            DependencyProperty.Register("Matrix", typeof(Matrix3D), typeof(CompositeTransform3D), new PropertyMetadata(Matrix3D.Identity));

        #endregion

        #region CreateInstanceCore implementation

        protected override Freezable CreateInstanceCore()
        {
            return new MatrixTransform3D();
        }

        #endregion
    }
    }
