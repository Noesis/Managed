using Noesis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NoesisApp
{
    /// <summary>
    /// Represents a collection of triggers with a shared AssociatedObject and provides change
    /// notifications to its contents when that AssociatedObject changes.
    /// </summary>
    public class TriggerCollection : AttachableCollection<TriggerBase>
    {
        public TriggerCollection()
        {
        }

        public new TriggerCollection Clone()
        {
            return (TriggerCollection)base.Clone();
        }

        public new TriggerCollection CloneCurrentValue()
        {
            return (TriggerCollection)base.CloneCurrentValue();
        }

        protected override void OnAttached()
        {
            foreach (TriggerBase trigger in this)
            {
                trigger.Attach(base.AssociatedObject);
            }
        }

        protected override void OnDetaching()
        {
            foreach (TriggerBase trigger in this)
            {
                trigger.Detach();
            }
        }

        protected override void ItemAdded(TriggerBase item)
        {
            if (base.AssociatedObject != null)
            {
                item.Attach(base.AssociatedObject);
            }
        }

        protected override void ItemRemoved(TriggerBase item)
        {
            if (base.AssociatedObject != null)
            {
                item.Detach();
            }
        }
    }
}
