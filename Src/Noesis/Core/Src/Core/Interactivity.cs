using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace Noesis.Interactivity
{
    /// <summary>
    /// Behavior is the base class for providing attachable state and logic to an object.
    /// The types the Behavior can be attached to can be controlled by the generic parameter.
    /// Override OnAttached() and OnDetaching() methods to hook and unhook any necessary handlers
    /// from the AssociatedObject.
    /// </summary>
    public abstract class Behavior<T> : Behavior where T : DependencyObject
    {
        internal Behavior(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        protected Behavior(): base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }

    /// <summary>
    /// TriggerBase is the base class for controlling actions. Override OnAttached() and
    /// OnDetaching() to hook and unhook handlers on the AssociatedObject. You may constrain
    /// the types that a derived TriggerBase may be attached to by specifying the generic
    /// parameter. Call InvokeActions() to fire all Actions associated with this trigger.
    /// </summary>
    public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject
    {
        internal TriggerBase(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        protected TriggerBase(): base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }

    /// <summary>
    /// Represents an attachable object that encapsulates a unit of functionality.
    /// </summary>
    public abstract class TriggerAction<T> : TriggerAction where T : DependencyObject
    {
        internal TriggerAction(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        protected TriggerAction(): base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }

    /// <summary>
    /// Represents an action that can be targeted to affect an object other than its AssociatedObject.
    /// </summary>
    public abstract class TargetedTriggerAction<T> : TargetedTriggerAction where T : class
    {
        internal TargetedTriggerAction(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        protected TargetedTriggerAction(): base(typeof(T)) { }

        protected new T Target { get { return (T)base.Target; } }

        protected sealed override void OnTargetChangedImpl(object oldTarget, object newTarget)
        {
            base.OnTargetChangedImpl(oldTarget, newTarget);
            OnTargetChanged((T)oldTarget, (T)newTarget);
        }

        protected virtual void OnTargetChanged(T oldTarget, T newTarget)
        {
        }
    }

    /// <summary>
    /// Event handler for registering to TriggerBase's PreviewInvoke event.
    /// </summary>
    public delegate void PreviewInvokeHandler(object sender, PreviewInvokeEventArgs e);

    /// <summary>
    /// Represents a collection of behaviors with a shared AssociatedObject.
    /// </summary>
    public sealed class BehaviorCollection : FreezableCollection<Behavior>
    {
        internal BehaviorCollection(IntPtr cPtr, bool cMemoryOwn): base(cPtr, cMemoryOwn) { }

        internal new static BehaviorCollection CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new BehaviorCollection(cPtr, cMemoryOwn);
        }
    }

    ///<summary>
    /// Represents a collection of triggers with a shared AssociatedObject.
    /// </summary>
    public sealed class TriggerCollection : FreezableCollection<TriggerBase>
    {
        internal TriggerCollection(IntPtr cPtr, bool cMemoryOwn): base(cPtr, cMemoryOwn) { }

        internal new static TriggerCollection CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new TriggerCollection(cPtr, cMemoryOwn);
        }
    }

    /// <summary>
    /// Represents a collection of actions with a shared AssociatedObject.
    /// </summary>
    public class TriggerActionCollection : FreezableCollection<TriggerAction>
    {
        internal TriggerActionCollection(IntPtr cPtr, bool cMemoryOwn): base(cPtr, cMemoryOwn) { }

        internal new static TriggerActionCollection CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new TriggerActionCollection(cPtr, cMemoryOwn);
        }
    }


    /// <summary>
    /// Represents a collection of conditions for the ConditionBehavior.
    /// </summary>
    public class ConditionCollection : FreezableCollection<ComparisonCondition>
    {
        internal ConditionCollection(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        internal new static ConditionCollection CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new ConditionCollection(cPtr, cMemoryOwn);
        }
    }
}
