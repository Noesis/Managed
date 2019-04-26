using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Represents the base class for specifying attachable objects.
    /// </summary>
    public abstract class AttachableObject : Animatable, IAttachedObject
    {
        protected AttachableObject(Type associatedType)
        {
            _associatedType = associatedType;
            _associatedObject = null;
            _view = null;
        }

        protected Type AssociatedType { get { return _associatedType; } }

        protected DependencyObject AssociatedObject { get { return _associatedObject; } }

        DependencyObject IAttachedObject.AssociatedObject { get { return this.AssociatedObject; } }

        public void Attach(DependencyObject associatedObject)
        {
            if (AssociatedObject != associatedObject)
            {
                if (AssociatedObject != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "'{0}' already attached to another object '{1}'",
                        GetType(), AssociatedObject.GetType()));
                }

                if (associatedObject == null)
                {
                    throw new InvalidOperationException("Cannot attach to a null object");
                }

                if (!_associatedType.IsAssignableFrom(associatedObject.GetType()))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid associated element type '{0}' for '{1}'",
                        associatedObject.GetType(), GetType()));
                }

                _associatedObject = associatedObject;
                _view = View.Find(this);

                OnAttached();
            }
        }

        public void Detach()
        {
            if (AssociatedObject != null)
            {
                OnDetaching();

                _associatedObject = null;
                _view = null;
            }
        }

        /// <summary>
        /// Called after the object is attached to an AssociatedObject
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called just before the object is detached from its AssociatedObject
        /// </summary>
        protected virtual void OnDetaching()
        {
        }

        /// <summary>
        /// Gets the View where this object is connected to
        /// </summary>
        public View View
        {
            get { return _view; }
        }

        private Type _associatedType;
        DependencyObject _associatedObject;
        View _view;
    }
}
