using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Noesis
{
    public delegate HitTestFilterBehavior HitTestFilterCallback(Visual target);
    public delegate HitTestResultBehavior HitTestResultCallback(HitTestResult hit);
    public delegate HitTestResultBehavior HitTest3DResultCallback(HitTest3DResult hit);

    /// <summary>
    /// This is the base class for packing together parameters for a hit test pass.
    /// </summary>
    public abstract class HitTestParameters
    {
        // Prevent 3rd parties from extending this abstract base class.
        internal HitTestParameters() { }
    }

    /// <summary>
    /// This is the class for specifying parameters hit testing with a point.
    /// </summary>
    public class PointHitTestParameters : HitTestParameters
    {
        /// <summary>
        /// The constructor takes the point to hit test with.
        /// </summary>
        public PointHitTestParameters(Point point) : base()
        {
            _hitPoint = point;
        }

        /// <summary>
        /// The point to hit test against.
        /// </summary>
        public Point HitPoint
        {
            get
            {
                return _hitPoint;
            }
        }

        private Point _hitPoint;
    }
}

