using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Manages a collection of behaviors and triggers that expand the object functionality from XAML.
    /// </summary>
    public static class Interaction
    {
        /// <summary>
        /// Gets the value of the Behaviors attached property
        /// </summary>
        public static BehaviorCollection GetBehaviors(DependencyObject d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            BehaviorCollection behaviors = (BehaviorCollection)d.GetValue(BehaviorsProperty);
            if (behaviors == null)
            {
                behaviors = new BehaviorCollection();
                d.SetValue(BehaviorsProperty, behaviors);
            }

            return behaviors;
        }

        private static readonly DependencyProperty BehaviorsProperty = DependencyProperty.Register(
            "Behaviors", typeof(BehaviorCollection), typeof(Interaction),
            new PropertyMetadata(null, OnBehaviorsChanged));

        private static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BehaviorCollection oldBehaviors = (BehaviorCollection)e.OldValue;
            if (oldBehaviors != null && ((IAttachedObject)oldBehaviors).AssociatedObject == d)
            {
                oldBehaviors.Detach();
            }

            BehaviorCollection newBehaviors = (BehaviorCollection)e.NewValue;
            if (newBehaviors != null)
            {
                FrameworkElement element = FrameworkElement.FindTreeElement(newBehaviors);
                if (element != null && !element.IsInitialized)
                {
                    // assigned from a template, collection must be cloned
                    d.SetValue(BehaviorsProperty, newBehaviors.Clone());
                }
                else
                {
                    newBehaviors.Attach(d);
                }
            }
        }

        /// <summary>
        /// Gets the value of the Triggers attached property
        /// </summary>
        public static TriggerCollection GetTriggers(DependencyObject d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            TriggerCollection triggers = (TriggerCollection)d.GetValue(TriggersProperty);
            if (triggers == null)
            {
                triggers = new TriggerCollection();
                d.SetValue(TriggersProperty, triggers);
            }

            return triggers;
        }

        private static readonly DependencyProperty TriggersProperty = DependencyProperty.Register(
            "Triggers", typeof(TriggerCollection), typeof(Interaction),
            new PropertyMetadata(null, OnTriggersChanged));

        private static void OnTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TriggerCollection oldTriggers = (TriggerCollection)e.OldValue;
            if (oldTriggers != null && ((IAttachedObject)oldTriggers).AssociatedObject == d)
            {
                oldTriggers.Detach();
            }

            TriggerCollection newTriggers = (TriggerCollection)e.NewValue;
            if (newTriggers != null)
            {
                FrameworkElement element = FrameworkElement.FindTreeElement(newTriggers);
                if (element != null && !element.IsInitialized)
                {
                    // assigned from a template, collection must be cloned
                    d.SetValue(TriggersProperty, newTriggers.Clone());
                }
                else
                {
                    newTriggers.Attach(d);
                }
            }
        }
    }
}
