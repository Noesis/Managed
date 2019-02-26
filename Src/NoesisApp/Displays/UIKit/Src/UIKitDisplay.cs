using System;
using UIKit;
using Foundation;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;

namespace NoesisApp
{
    public class View : UIView, IUIKeyInput
    {
        public UIKitDisplay Display { get; set; }

        public View(CGRect rect)
            : base(rect)
        {
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            nfloat s = UIScreen.MainScreen.Scale;
            foreach (UITouch touch in touches)
            {
                CGPoint p = touch.LocationInView(null);
                Display.OnTouchDown((int)(p.X * s), (int)(p.Y * s), (ulong)touch.Handle);
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            nfloat s = UIScreen.MainScreen.Scale;
            foreach (UITouch touch in touches)
            {
                CGPoint p = touch.LocationInView(null);
                Display.OnTouchMove((int)(p.X * s), (int)(p.Y * s), (ulong)touch.Handle);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            nfloat s = UIScreen.MainScreen.Scale;
            foreach (UITouch touch in touches)
            {
                CGPoint p = touch.LocationInView(null);
                Display.OnTouchUp((int)(p.X * s), (int)(p.Y * s), (ulong)touch.Handle);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            TouchesEnded(touches, evt);
        }

        public void InsertText(string text)
        {
            foreach (char c in text)
            {
                Display.OnChar(c);
            }
        }

        public void DeleteBackward()
        {
            Display.OnDeleteBackward();
        }

        public bool HasText { get { return true; } }

        public UITextAutocapitalizationType AutocapitalizationType { get; set; }

        public UITextAutocorrectionType AutocorrectionType { get; set; }

        public UIKeyboardType KeyboardType { get; set; }

        public UIKeyboardAppearance KeyboardAppearance { get; set; }

        public UIReturnKeyType ReturnKeyType { get; set; }

        public bool EnablesReturnKeyAutomatically { get; set; }

        public bool SecureTextEntry { get; set; }

        public UITextSpellCheckingType SpellCheckingType { get; set; }
    }

    public class ViewController : UIViewController
    {
        UIKitDisplay Display { get; set; }

        public ViewController(UIKitDisplay display)
        {
            Display = display;
        }

        public override void LoadView()
        {
            View view = new View(UIScreen.MainScreen.Bounds);
            view.Display = Display;
            view.MultipleTouchEnabled = true;
            View = view;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.All;
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            Display.OnSizeChanged(Display.ClientWidth, Display.ClientHeight);
        }
    }

    public class UISubclassedWindow : UIWindow
    {
        public UIKitDisplay Display { get; set; }

        public UISubclassedWindow(CGRect rect)
            : base(rect)
        {
        }

        [Export("Activate:")]
        public void Activate(NSNotification notification)
        {
            if (Display.DisplayLink != null)
            {
                NSRunLoop loop = NSRunLoop.Current;
                Display.DisplayLink.AddToRunLoop(loop, NSRunLoopMode.Default);
            }

            Display.OnActivated();
        }

        [Export("Deactivate:")]
        public void Deactivate(NSNotification notification)
        {
            if (Display.DisplayLink != null)
            {
                NSRunLoop loop = NSRunLoop.Current;
                Display.DisplayLink.RemoveFromRunLoop(loop, NSRunLoopMode.Default);
            }

            Display.OnDeactivated();
        }

        [Export("Render")]
        public void Render()
        {
            Display.OnRender();
        }
    }

    public class UIKitDisplay : Display
    {
        public UISubclassedWindow Window { get; set; }
        public CADisplayLink DisplayLink { get; set; }

        public UIKitDisplay()
        {
            Window = new UISubclassedWindow(UIScreen.MainScreen.Bounds);
            Window.Display = this;
            Window.AutosizesSubviews = true;
            Window.RootViewController = new ViewController(this);
            Window.BackgroundColor = UIColor.Black;
            NSNotificationCenter.DefaultCenter.AddObserver(Window, new Selector("Activate:"), UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(Window, new Selector("Deactivate:"), UIApplication.WillResignActiveNotification, null);
        }

        ~UIKitDisplay()
        {
            DisplayLink.Invalidate();

            NSNotificationCenter.DefaultCenter.RemoveObserver(Window, UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(Window, UIApplication.WillResignActiveNotification, null);
        }

        public override IntPtr NativeHandle
        {
            get { return IntPtr.Zero; }
        }

        public override IntPtr NativeWindow
        {
            get { return Window.RootViewController.View.Handle; }
        }

        public override int ClientWidth
        {
            get { return (int)(Window.Bounds.Size.Width * UIScreen.MainScreen.Scale); }
        }

        public override int ClientHeight
        {
            get { return (int)(Window.Bounds.Size.Height * UIScreen.MainScreen.Scale); }
        }

        public override void Show()
        {
            SizeChanged?.Invoke(this, ClientWidth, ClientHeight);
            LocationChanged?.Invoke(this, 0, 0);
            Window.MakeKeyAndVisible();
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            DisplayLink = CADisplayLink.Create(Window, new Selector("Render"));
            DisplayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
            NSRunLoop.Current.Run();
        }

        public override void OpenSoftwareKeyboard(Noesis.UIElement focused)
        {
            UIView view = Window.RootViewController.View;
            view.BecomeFirstResponder();
        }

        public override void CloseSoftwareKeyboard()
        {
            UIView view = Window.RootViewController.View;
            view.ResignFirstResponder();
        }

        public void OnActivated()
        {
            Activated?.Invoke(this);
        }

        public void OnDeactivated()
        {
            Deactivated?.Invoke(this);
        }

        public void OnRender()
        {
            Render?.Invoke(this);
        }

        public void OnSizeChanged(int Width, int Height)
        {
            SizeChanged(this, ClientWidth, ClientHeight);
        }

        public void OnTouchDown(int x, int y, ulong id)
        {
            TouchDown?.Invoke(this, x, y, id);
        }

        public void OnTouchMove(int x, int y, ulong id)
        {
            TouchMove?.Invoke(this, x, y, id);
        }

        public void OnTouchUp(int x, int y, ulong id)
        {
            TouchUp?.Invoke(this, x, y, id);
        }

        public void OnChar(char c)
        {
            Char?.Invoke(this, c);
        }

        public void OnDeleteBackward()
        {
            KeyDown?.Invoke(this, Noesis.Key.Back);
            Char?.Invoke(this, '\b');
            KeyUp?.Invoke(this, Noesis.Key.Back);
        }
    }
}
