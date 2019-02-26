using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An interface for an object that can be attached to another object.
    /// </summary>
    public interface IAttachedObject
    {
        /// <summary>
        /// Gets the associated object
        /// </summary>
        DependencyObject AssociatedObject { get; }

        /// <summary>
        /// Attaches to the specified object
        /// </summary>
        void Attach(DependencyObject associatedObject);

        /// <summary>
        /// Detaches this instance from its associated object
        /// </summary>
        void Detach();
    }
}
