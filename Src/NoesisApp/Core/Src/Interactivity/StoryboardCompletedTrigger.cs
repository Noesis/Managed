using Noesis;

namespace NoesisApp
{
    /// <summary>
    /// A trigger that listens for the completion of a Storyboard.
    /// </summary>
    public class StoryboardCompletedTrigger : StoryboardTrigger
    {
        public new StoryboardCompletedTrigger Clone()
        {
            return (StoryboardCompletedTrigger)base.Clone();
        }

        public new StoryboardCompletedTrigger CloneCurrentValue()
        {
            return (StoryboardCompletedTrigger)base.CloneCurrentValue();
        }

        protected override void CloneCommonCore(Freezable source)
        {
            base.CloneCommonCore(source);

            StoryboardCompletedTrigger action = (StoryboardCompletedTrigger)source;
            Storyboard storyboard = action.Storyboard;
            if (storyboard != null &&
                FrameworkElement.FindTreeParent(storyboard) is ResourceDictionary)
            {
                // use original Storyboard resource, not a clone
                Storyboard = storyboard;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            Storyboard storyboard = Storyboard;
            DependencyObject associatedObject = AssociatedObject;
            FrameworkElement target = StoryboardAction.FindStoryboardTarget(storyboard, associatedObject);
            _target = GetPtr(target);
        }

        protected override void OnDetaching()
        {
            Storyboard storyboard = Storyboard;
            if (storyboard != null)
            {
                storyboard.Completed -= OnStoryboardCompleted;

                _target = System.IntPtr.Zero;
            }

            base.OnDetaching();
        }

        protected override void OnStoryboardChanged(DependencyPropertyChangedEventArgs e)
        {
            Storyboard oldStoryboard = (Storyboard)e.OldValue;
            Storyboard newStoryboard = (Storyboard)e.NewValue;

            if (oldStoryboard != null)
            {
                oldStoryboard.Completed -= OnStoryboardCompleted;

                _target = System.IntPtr.Zero;
            }
            if (newStoryboard != null)
            {
                newStoryboard.Completed += OnStoryboardCompleted;

                DependencyObject associatedObject = AssociatedObject;
                FrameworkElement target = StoryboardAction.FindStoryboardTarget(newStoryboard, associatedObject);
                _target = GetPtr(target);
            }
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            TimelineEventArgs args = (TimelineEventArgs)e;
            FrameworkElement argsTarget = args.Target as FrameworkElement;
            FrameworkElement target = (FrameworkElement)GetProxy(_target);
            if (argsTarget == null || target == null || target.TemplatedParent == null ||
                argsTarget.TemplatedParent == target.TemplatedParent)
            {
                InvokeActions(e);
            }
        }

        private System.IntPtr _target = System.IntPtr.Zero;
    }
}
