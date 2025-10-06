using NoesisApp;
using System;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Allows the playback of an animated GIF to an Image control.
    ///
    /// Usage:
    ///
    ///    <Grid 
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///      xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=NoesisGUI.GUI.Extensions">
    ///      <Image Stretch="Uniform">
    ///        <b:Interaction.Behaviors>
    ///          <noesis:AnimatedGifBehavior Source="Images/Animated.gif"/>
    ///        </b:Interaction.Behaviors>
    ///      </Image>
    ///    </Grid>
    /// </summary>
    public class AnimatedGifBehavior : Behavior<Image>
    {
        #region Source
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly Noesis.DependencyProperty SourceProperty = Noesis.DependencyProperty.Register(
            "Source", typeof(Uri), typeof(AnimatedGifBehavior),
            new Noesis.FrameworkPropertyMetadata(null,
                Noesis.FrameworkPropertyMetadataOptions.AffectsMeasure | Noesis.FrameworkPropertyMetadataOptions.AffectsRender,
                OnSourceChanged));

        private static void OnSourceChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            var ag = d as AnimatedGifBehavior;
            ag.RemoveStoryboard();
            ag.AddStoryboard();
        }
        #endregion

        #region Behavior methods
        protected override void OnAttached()
        {
            Image target = AssociatedObject;
            if (target != null)
            {
                target.Loaded += OnTargetLoaded;
                target.Unloaded += OnTargetUnloaded;
            }
        }

        protected override void OnDetaching()
        {
            Image target = AssociatedObject;
            if (target != null)
            {
                target.Loaded -= OnTargetLoaded;
                target.Unloaded -= OnTargetUnloaded;
            }
        }

        private void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            AddStoryboard();
        }

        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            RemoveStoryboard();
        }
        #endregion

        #region Private members
        private void AddStoryboard()
        {
            Image target = AssociatedObject;
            string sourceStr = Source.ToString();
            if (target != null && sourceStr.Length != 0)
            {
                IntPtr imageSources = IntPtr.Zero;
                IntPtr timesPtr = IntPtr.Zero;
                int count = Noesis_AnimatedGif_LoadImageSources(sourceStr, ref imageSources, ref timesPtr);

                if (count == 0)
                    return;

                float[] times = new float[count + 1];
                times[0] = 0.0f;
                System.Runtime.InteropServices.Marshal.Copy(timesPtr, times, 1, count);

                var animation = new ObjectAnimationUsingKeyFrames();

                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));

                animation.Duration = new Duration(new TimeSpan((long)(times[count] * TimeSpan.TicksPerSecond)));
                animation.RepeatBehavior = RepeatBehavior.Forever;

                var keyFrames = animation.KeyFrames;

                for (int i = 0; i < count; ++i)
                {
                    var nativeImageSource = System.Runtime.InteropServices.Marshal.ReadIntPtr(imageSources, i * System.Runtime.InteropServices.Marshal.SizeOf<IntPtr>());
                    var imageSource = BaseComponent.GetProxy(nativeImageSource) as ImageSource;
                    var kf = new DiscreteObjectKeyFrame();
                    kf.KeyTime = KeyTime.FromTimeSpan(new TimeSpan((long)(times[i] * TimeSpan.TicksPerSecond)));
                    kf.Value = imageSource;
                    keyFrames.Add(kf);
                }

                storyboard_ = new Storyboard();
                storyboard_.Children.Add(animation);
                storyboard_.Begin(FrameworkElement.FindTreeElement(target), true);

                Noesis_AnimatedGif_FreeArrays(imageSources, timesPtr, count);
            }
        }

        private void RemoveStoryboard()
        {
            Image target = AssociatedObject;
            if (target != null && storyboard_ != null)
            {
                storyboard_.Stop(FrameworkElement.FindTreeElement(target));
                storyboard_.Children.Clear();
                storyboard_.Dispose();
                storyboard_ = null;
            }
        }

        Storyboard storyboard_ = null;
        #endregion

        #region Imports
        [DllImport("Noesis")]
        public static extern int Noesis_AnimatedGif_LoadImageSources([MarshalAs(UnmanagedType.LPWStr)] string uriStr, ref IntPtr imageSources, ref IntPtr times);

        [DllImport("Noesis")]
        public static extern void Noesis_AnimatedGif_FreeArrays(IntPtr imageSources, IntPtr times, int count);

        #endregion
    }
}
