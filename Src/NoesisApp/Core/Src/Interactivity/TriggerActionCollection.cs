using Noesis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NoesisApp
{
    /// <summary>
    /// Represents a collection of actions with a shared AssociatedObject and provides change
    /// notifications to its contents when that AssociatedObject changes.
    /// </summary>
    public class TriggerActionCollection : AttachableCollection<TriggerAction>
    {
        public TriggerActionCollection()
        {
        }

        public new TriggerActionCollection Clone()
        {
            return (TriggerActionCollection)base.Clone();
        }

        public new TriggerActionCollection CloneCurrentValue()
        {
            return (TriggerActionCollection)base.CloneCurrentValue();
        }

        protected override void OnAttached()
        {
            foreach (TriggerAction triggerAction in this)
            {
                triggerAction.Attach(base.AssociatedObject);
            }
        }

        protected override void OnDetaching()
        {
            foreach (TriggerAction triggerAction in this)
            {
                triggerAction.Detach();
            }
        }

        protected override void ItemAdded(TriggerAction item)
        {
            if (base.AssociatedObject != null)
            {
                item.Attach(base.AssociatedObject);
            }
        }

        protected override void ItemRemoved(TriggerAction item)
        {
            if (base.AssociatedObject != null)
            {
                item.Detach();
            }
        }
    }
}
