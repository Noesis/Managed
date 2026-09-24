////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Media;

namespace NoesisGUIExtensions
{
    public class SetFocusAction : TargetedTriggerAction<UIElement>
    {
        public bool Engage
        {
            get { return (bool)GetValue(EngageProperty); }
            set { SetValue(EngageProperty, value); }
        }

        public static readonly DependencyProperty EngageProperty = DependencyProperty.Register(
            "Engage", typeof(bool), typeof(SetFocusAction), new PropertyMetadata(true));

        protected override void Invoke(object parameter)
        {
            UIElement element = Target;
            if (element != null)
            {
                element.Focus();
            }
        }
    }

    public enum FocusDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public class MoveFocusAction : TriggerAction<UIElement>
    {
        public FocusDirection Direction
        {
            get { return (FocusDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction", typeof(FocusDirection), typeof(MoveFocusAction), new PropertyMetadata(FocusDirection.Left));

        public bool Engage
        {
            get { return (bool)GetValue(EngageProperty); }
            set { SetValue(EngageProperty, value); }
        }

        public static readonly DependencyProperty EngageProperty = DependencyProperty.Register(
            "Engage", typeof(bool), typeof(MoveFocusAction), new PropertyMetadata(true));

        protected override void Invoke(object parameter)
        {
            UIElement element = AssociatedObject;
            if (element != null)
            {
                UIElement source = (UIElement)Keyboard.FocusedElement ?? element;

                int direction = (int)FocusNavigationDirection.Left + (int)Direction;
                UIElement target = (UIElement)source.PredictFocus((FocusNavigationDirection)direction);

                if (target != null)
                {
                    target.Focus();
                }
            }
        }
    }

    public class SelectAction : TriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            Control control = AssociatedObject;
            if (control != null)
            {
                Selector.SetIsSelected(control, true);
            }
        }
    }

    public class SelectAllAction : TriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            Control control = AssociatedObject;

            TextBox textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
                return;
            }

            PasswordBox passwordBox = control as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.SelectAll();
            }
        }
    }

    public class SeekStoryboardAction : StoryboardAction
    {
        public TimeSpan Offset
        {
            get { return (TimeSpan)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(TimeSpan), typeof(SeekStoryboardAction),
            new PropertyMetadata(TimeSpan.Zero));

        public TimeSeekOrigin Origin
        {
            get { return (TimeSeekOrigin)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        public static readonly DependencyProperty OriginProperty = DependencyProperty.Register(
            "Origin", typeof(TimeSeekOrigin), typeof(SeekStoryboardAction),
            new PropertyMetadata(TimeSeekOrigin.BeginTime));

        protected override void Invoke(object o)
        {
            if (this.AssociatedObject != null && this.Storyboard != null)
            {
                this.Storyboard.Seek(Offset, Origin);
            }
        }
    }

    public class PlayMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            if (this.Target != null)
            {
                this.Target.Play();
            }
        }
    }

    public class PauseMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            if (this.Target != null)
            {
                this.Target.Pause();
            }
        }
    }

    public class RewindMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            if (this.Target != null)
            {
                this.Target.Position = TimeSpan.Zero;
            }
        }
    }

    public class StopMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            if (this.Target != null)
            {
                this.Target.Stop();
            }
        }
    }
}
