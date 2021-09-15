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
                storyboard.Completed -= OnStoryboardCompleted;
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
            }
            if (newStoryboard != null)
            {
                newStoryboard.Completed += OnStoryboardCompleted;
            }
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            InvokeActions(e);
        }
    }
}
