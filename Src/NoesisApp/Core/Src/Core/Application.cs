using System;
using Noesis;
using NoesisApp;

namespace NoesisApp
{
    public class StartupEventArgs : System.EventArgs
    {
    }

    public delegate void StartupEventHandler(object sender, StartupEventArgs e);

    public class ExitEventArgs : System.EventArgs
    {
        public int ApplicationExitCode { get; internal set; }
    }

    public delegate void ExitEventHandler(object sender, ExitEventArgs e);


    /// <summary>
    /// Application base class
    /// </summary>
    public abstract class Application
    {
        public static Application Current { get; private set; }

        public Dispatcher Dispatcher { get; private set; }

        public string Uri { get; set; }

        public string StartupUri { get; set; }

        public Display Display { get; set; }

        public Window MainWindow { get; set; }

        public ResourceDictionary Resources { get; set; }

        public event StartupEventHandler Startup;

        public event ExitEventHandler Exit;

        public Application()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("An Application was already created");
            }

            Current = this;
            Dispatcher = Dispatcher.CurrentDispatcher;

            _exitCode = 0;

            Log.SetLogCallback(LogCallback);
            Error.SetUnhandledCallback(UnhandledCallback);
            GUI.Init();
        }

        ///<summary>
        /// Run is called to start an application with a render loop
        /// Once run has been called - an application's OnStartup override and Startup event is
        /// called immediately afterwards.
        ///</summary>
        /// <returns>ExitCode of the application</returns>
        public int Run()
        {
            Start();

            Display.Show();

            DateTime startTime = DateTime.Now;
            Display.Render += (d) =>
            {
                MainWindow.Render((DateTime.Now - startTime).TotalSeconds);
            };

            bool runInBackground = false; // TODO: decide how to get this option
            Display.EnterMessageLoop(runInBackground);

            Finish();

            return _exitCode;
        }

        private void Start()
        {
            // Create application display
            Display = CreateDisplay();

            // Redirect integration callbacks to display
            GUI.SetSoftwareKeyboardCallback((UIElement focused, bool open) =>
            {
                if (open)
                {
                    Display.OpenSoftwareKeyboard(focused);
                }
                else
                {
                    Display.CloseSoftwareKeyboard();
                }
            });

            GUI.SetCursorCallback((View view, Cursor cursor) =>
            {
                Display.SetCursor(cursor);
            });

            GUI.SetOpenUrlCallback((string url) =>
            {
                Display.OpenUrl(url);
            });

            GUI.SetPlaySoundCallback((string filename, float volume) =>
            {
                Display.PlaySound(filename, volume);
            });

            // Set resource providers
            Type type = this.GetType();
            GUI.SetXamlProvider(new EmbeddedXamlProvider(type.Assembly, type.Namespace, CreateXamlProvider()));
            GUI.SetTextureProvider(new EmbeddedTextureProvider(type.Assembly, type.Namespace, CreateTextureProvider()));
            GUI.SetFontProvider(new EmbeddedFontProvider(type.Assembly, type.Namespace, CreateFontProvider()));

            // Load App.xaml
            if (string.IsNullOrEmpty(Uri))
            {
                throw new InvalidOperationException("App xaml uri not defined");
            }

            GUI.LoadComponent(this, Uri);
            GUI.SetApplicationResources(Resources);

            // Load StartupUri xaml as MainWindow
            if (string.IsNullOrEmpty(StartupUri))
            {
                throw new InvalidOperationException("Startup window not defined");
            }

            object root = GUI.LoadXaml(StartupUri);
            MainWindow = root as Window;

            if (MainWindow == null)
            {
                // non window roots are allowed
                MainWindow = new Window();
                MainWindow.Content = root;
                MainWindow.Title = Title;
            }

            uint samples = Samples;
            bool vsync = VSync;
            bool sRGB = StandardRGB;
            bool ppaa = PPAA;

            RenderContext renderContext = CreateRenderContext();
            renderContext.Init(Display.NativeHandle, Display.NativeWindow, samples, vsync, sRGB);
            renderContext.Device.OffscreenSampleCount = samples;

            Display.NativeHandleChanged += (Display display, IntPtr window) =>
            {
                renderContext.SetWindow(window);
            };

            MainWindow.Init(Display, renderContext, samples, ppaa);

            StartupEventArgs e = new StartupEventArgs();
            OnStartup(e);
        }

        private void Finish()
        {
            ExitEventArgs e = new ExitEventArgs { ApplicationExitCode = _exitCode };
            try
            {
                OnExit(e);
            }
            finally
            {
                _exitCode = e.ApplicationExitCode;

                // Make sure the View is destroyed after the Window element tree
                View view = MainWindow.View;

                MainWindow.Shutdown();
                MainWindow = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                view = null;
                Resources = null;
                Current = null;

                // Shut down Noesis
                GUI.Shutdown();
            }
        }

        /// <summary>
        ///  Shuts down the application. You can handle the Exit event to detect when an application is
        ///  about to stop running, to perform any appropriate processing.
        /// </summary>
        public void Shutdown()
        {
            Shutdown(0);
        }

        /// <summary>
        /// Shuts down the application that returns the specified exit code to the operating system.
        /// </summary>
        /// <param name="exitCode">An integer exit code. The default is 0.</param>
        public void Shutdown(int exitCode)
        {
            _exitCode = exitCode;
            MainWindow.Close();
        }

        private void LogCallback(LogLevel level, string channel, string message)
        {
            OnLog(level, channel, message);
        }

        /// <summary>
        /// Called when Noesis wants to print a message
        /// </summary>
        /// <param name="level">Trace, Debug, Info, Warning or Error</param>
        /// <param name="channel">General channel is the empty string</param>
        /// <param name="message">Message text</param>
        protected virtual void OnLog(LogLevel level, string channel, string message)
        {
            if (string.IsNullOrEmpty(channel) || channel == "Binding")
            {
                string[] prefixes = new string[] { "T", "D", "I", "W", "E" };
                string prefix = (int)level < prefixes.Length ? prefixes[(int)level] : " ";
                System.Console.WriteLine("[NOESIS/" + prefix + "] " + message);
            }
        }

        private void UnhandledCallback(Exception exception)
        {
            OnUnhandledException(exception);
        }

        /// <summary>
        /// Called when an unhandled exception occurs in C# during a Native to Managed transition.
        /// No throws should be raised inside this function to avoid stack unwinding problems.
        /// </summary>
        /// <param name="exception">Unhanlded exception generated in managed code</param>
        protected virtual void OnUnhandledException(Exception exception)
        {
            // Exception stack trace stops on managed-native transitions, so we create a StackTrace
            // object that contains information of the full call stack
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(3, true);
            System.Console.WriteLine(exception.GetType() + ": " + exception.Message + "\n" +
                exception.StackTrace + "\n" + stack);
        }

        /// <summary>
        /// Creates an operating system Display where the application will be rendered
        /// </summary>
        protected abstract Display CreateDisplay();

        /// <summary>
        /// Creates the RenderContext used to render the application
        /// </summary>
        protected abstract RenderContext CreateRenderContext();

        /// <summary>
        /// Creates the provider used for loading XAML files and resources
        /// </summary>
        protected virtual XamlProvider CreateXamlProvider()
        {
            Type type = this.GetType();
            return new EmbeddedXamlProvider(type.Assembly, type.Namespace);
        }

        /// <summary>
        /// Creates the provider used for loading textures
        /// </summary>
        protected virtual FileTextureProvider CreateTextureProvider()
        {
            Type type = this.GetType();
            return new EmbeddedTextureProvider(type.Assembly, type.Namespace);
        }

        /// <summary>
        ///  Creates the provider used for loading fonts
        /// </summary>
        protected virtual FontProvider CreateFontProvider()
        {
            Type type = this.GetType();
            return new EmbeddedFontProvider(type.Assembly, type.Namespace);
        }

        protected virtual string Title
        {
            get { return ""; }
        }

        protected virtual uint Samples
        {
            get { return 1; }
        }

        protected virtual bool VSync
        {
            get { return false; }
        }

        protected virtual bool StandardRGB
        {
            get { return false; }
        }

        protected virtual bool PPAA
        {
            get { return Samples == 1; }
        }

        protected virtual void OnStartup(StartupEventArgs e)
        {
            Startup?.Invoke(this, e);
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            Exit?.Invoke(this, e);
        }

        #region Private members
        int _exitCode;
        #endregion
    }
}

