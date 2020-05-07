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

        protected override void OnDetaching()
        {
            Storyboard storyboard = Storyboard;
            if (storyboard != null)
            {
                storyboard.Completed -= _wrapper.Completed;
            }

            base.OnDetaching();
        }

        protected override void OnStoryboardChanged(DependencyPropertyChangedEventArgs e)
        {
            Storyboard oldStoryboard = (Storyboard)e.OldValue;
            Storyboard newStoryboard = (Storyboard)e.NewValue;

            if (oldStoryboard != null)
            {
                oldStoryboard.Completed -= _wrapper.Completed;
            }
            if (newStoryboard != null)
            {
                newStoryboard.Completed += _wrapper.Completed;
            }
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            InvokeActions(e);
        }

        public StoryboardCompletedTrigger()
        {
            _wrapper = new EventWrapper { wr = new System.WeakReference(this) };
        }

        private struct EventWrapper
        {
            public System.WeakReference wr;

            public void Completed(object sender, EventArgs e)
            {
                ((StoryboardCompletedTrigger)wr.Target).OnStoryboardCompleted(sender, e);
            }
        }

        private EventWrapper _wrapper;
    }
}
