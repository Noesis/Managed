using Noesis;
using System;

namespace NoesisGUIExtensions
{
    public class StyleBehaviorCollection : FreezableCollection<NoesisApp.Behavior> { }

    public class StyleTriggerCollection : FreezableCollection<NoesisApp.TriggerBase> { }

    /// <summary>
    /// Allows setting a collection of Interactivity behaviors and triggers in a Style.
    /// </summary>
    public static class StyleInteraction
    {
        /// <summary>
        /// Gets the value of the Behaviors attached property
        /// </summary>
        public static StyleBehaviorCollection GetBehaviors(DependencyObject d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            return (StyleBehaviorCollection)d.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Sets the value of the Behaviors attached property
        /// </summary>
        public static void SetBehaviors(DependencyObject d, StyleBehaviorCollection value)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            d.SetValue(BehaviorsProperty, value);
        }

        private static readonly DependencyProperty BehaviorsProperty = DependencyProperty.Register(
            "Behaviors", typeof(StyleBehaviorCollection), typeof(StyleInteraction),
            new PropertyMetadata(null, OnBehaviorsChanged));

        private static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NoesisApp.BehaviorCollection behaviors = NoesisApp.Interaction.GetBehaviors(d);
            StyleBehaviorCollection styleBehaviors = (StyleBehaviorCollection)e.NewValue;
            int numBehaviors = styleBehaviors != null ? styleBehaviors.Count : 0;
            for (int i = 0; i < numBehaviors; ++i)
            {
                // we clone the original behavior to attach a new instance to the styled element
                behaviors.Add(styleBehaviors[i].Clone());
            }
        }

        /// <summary>
        /// Gets the value of the Triggers attached property
        /// </summary>
        public static StyleTriggerCollection GetTriggers(DependencyObject d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            return (StyleTriggerCollection)d.GetValue(TriggersProperty);
        }

        /// <summary>
        /// Sets the value of the Triggers attached property
        /// </summary>
        public static void SetTriggers(DependencyObject d, StyleTriggerCollection value)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            d.SetValue(TriggersProperty, value);
        }

        private static readonly DependencyProperty TriggersProperty = DependencyProperty.Register(
            "Triggers", typeof(StyleTriggerCollection), typeof(StyleInteraction),
            new PropertyMetadata(null, OnTriggersChanged));

        private static void OnTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NoesisApp.TriggerCollection triggers = NoesisApp.Interaction.GetTriggers(d);
            StyleTriggerCollection styleTriggers = (StyleTriggerCollection)e.NewValue;
            int numTriggers = styleTriggers != null ? styleTriggers.Count : 0;
            for (int i = 0; i < numTriggers; ++i)
            {
                // we clone the original trigger to attach a new instance to the styled element
                triggers.Add(styleTriggers[i].Clone());
            }
        }
    }
}

