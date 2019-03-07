////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Collection of Behaviors that will be added to the Interaction.Behaviors attached property
    /// </summary>
    public class StyleBehaviorCollection : List<System.Windows.Interactivity.Behavior> { }

    /// <summary>
    /// Collection of Triggers that will be added to the Interaction.Triggers attached property
    /// </summary>
    public class StyleTriggerCollection : List<System.Windows.Interactivity.TriggerBase> { }

    /// <summary>
    /// Provides two attached properties that will allow to specify behaviors and triggers within a Style
    /// </summary>
    public class StyleInteraction
    {
        #region Behaviors property
        public static StyleBehaviorCollection GetBehaviors(DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException("d");
            return (StyleBehaviorCollection)d.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject d, StyleBehaviorCollection behaviors)
        {
            if (d == null) throw new ArgumentNullException("d");
            d.SetValue(BehaviorsProperty, behaviors);
        }

        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            "Behaviors", typeof(StyleBehaviorCollection), typeof(StyleInteraction),
            new PropertyMetadata(null, OnBehaviorsChanged));

        private static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StyleBehaviorCollection styleBehaviors = e.NewValue as StyleBehaviorCollection;
            if (styleBehaviors != null)
            {
                System.Windows.Interactivity.BehaviorCollection behaviors =
                    System.Windows.Interactivity.Interaction.GetBehaviors(d);

                foreach (System.Windows.Interactivity.Behavior behavior in styleBehaviors)
                {
                    behaviors.Add((System.Windows.Interactivity.Behavior)behavior.Clone());
                }
            }
        }
        #endregion

        #region Triggers property
        public static StyleTriggerCollection GetTriggers(DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException("d");
            return (StyleTriggerCollection)d.GetValue(TriggersProperty);
        }

        public static void SetTriggers(DependencyObject d, StyleTriggerCollection triggers)
        {
            if (d == null) throw new ArgumentNullException("d");
            d.SetValue(TriggersProperty, triggers);
        }

        public static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached(
            "Triggers", typeof(StyleTriggerCollection), typeof(StyleInteraction),
            new PropertyMetadata(null, OnTriggersChanged));

        private static void OnTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StyleTriggerCollection styleTriggers = e.NewValue as StyleTriggerCollection;
            if (styleTriggers != null)
            {
                System.Windows.Interactivity.TriggerCollection triggers =
                    System.Windows.Interactivity.Interaction.GetTriggers(d);

                foreach (System.Windows.Interactivity.TriggerBase trigger in styleTriggers)
                {
                    System.Windows.Interactivity.TriggerBase triggerClone =
                        (System.Windows.Interactivity.TriggerBase)trigger.Clone();

                    foreach (System.Windows.Interactivity.TriggerAction action in trigger.Actions)
                    {
                        triggerClone.Actions.Add((System.Windows.Interactivity.TriggerAction)action.Clone());
                    }

                    triggers.Add(triggerClone);
                }
            }
        }
        #endregion
    }
}
