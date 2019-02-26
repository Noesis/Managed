using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Represents the base class for specifying behaviors for DependencyObjects.
    /// </summary>
    public abstract class Behavior : AttachableObject
    {
        protected Behavior(Type associatedType) : base(associatedType)
        {
        }

        public new Behavior Clone()
        {
            return (Behavior)base.Clone();
        }

        public new Behavior CloneCurrentValue()
        {
            return (Behavior)base.CloneCurrentValue();
        }
    }

    public abstract class Behavior<T> : Behavior where T : DependencyObject
    {
        protected Behavior() : base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }
}
