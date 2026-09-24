using Noesis;

namespace Noesis.Interactivity
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
            if (storyboard != null)
            {
                storyboard.Completed += OnStoryboardCompleted;
                _target = GetPtr(StoryboardAction.FindStoryboardTarget(storyboard, AssociatedObject));
            }
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
            DependencyObject associatedObject = AssociatedObject;
            if (associatedObject != null)
            {
                Storyboard oldStoryboard = (Storyboard)e.OldValue;
                if (oldStoryboard != null)
                {
                    oldStoryboard.Completed -= OnStoryboardCompleted;
                    _target = System.IntPtr.Zero;
                }

                Storyboard newStoryboard = (Storyboard)e.NewValue;
                if (newStoryboard != null)
                {
                    newStoryboard.Completed += OnStoryboardCompleted;
                    _target = GetPtr(StoryboardAction.FindStoryboardTarget(newStoryboard, associatedObject));
                }
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
