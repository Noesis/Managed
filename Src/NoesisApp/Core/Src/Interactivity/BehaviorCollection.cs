using Noesis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NoesisApp
{
    /// <summary>
    /// Represents a collection of behaviors with a shared AssociatedObject and provides change
    /// notifications to its contents when that AssociatedObject changes.
    /// </summary>
    public class BehaviorCollection : AttachableCollection<Behavior>
    {
        public BehaviorCollection()
        {
        }

        public new BehaviorCollection Clone()
        {
            return (BehaviorCollection)base.Clone();
        }

        public new BehaviorCollection CloneCurrentValue()
        {
            return (BehaviorCollection)base.CloneCurrentValue();
        }

        protected override void OnAttached()
        {
            foreach (Behavior behavior in this)
            {
                behavior.Attach(base.AssociatedObject);
            }
        }

        protected override void OnDetaching()
        {
            foreach (Behavior behavior in this)
            {
                behavior.Detach();
            }
        }

        protected override void ItemAdded(Behavior item)
        {
            if (base.AssociatedObject != null)
            {
                item.Attach(base.AssociatedObject);
            }
        }

        protected override void ItemRemoved(Behavior item)
        {
            if (base.AssociatedObject != null)
            {
                item.Detach();
            }
        }
    }
}
