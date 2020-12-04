using System;
using Noesis;

namespace NoesisApp
{
    /// <summary>
    /// Specifies which message box button a user clicked.
    /// </summary>
    public enum MessageBoxResult
    {
        /// The message box returns no result.
        None = 0,
        /// The result value of the message box is OK.
        OK = 1,
        /// The result value of the message box is Cancel.
        Cancel = 2,
        /// The result value of the message box is Yes.
        Yes = 6,
        /// The result value of the message box is No.
        No = 7
    }

    /// <summary>
    /// Specifies the buttons that are displayed on a message box.
    /// </summary>
    public enum MessageBoxButton
    {
        /// The message box displays an OK button.
        OK = 0,
        /// The message box displays OK and Cancel buttons.
        OKCancel = 1,
        /// The message box displays Yes, No, and Cancel buttons.
        YesNoCancel = 3,
        /// The message box displays Yes and No buttons.
        YesNo = 4
    }

    /// <summary>
    /// Specifies the icon that is displayed by a message box.
    /// </summary>
    public enum MessageBoxImage
    {
        /// No icon is displayed.
        None = 0,
        /// The message box contains a symbol consisting of a white X inside a red circle.
        Hand = 0x10,
        /// The message box contains a symbol consisting of a question mark in a blue circle.
        Question = 0x20,
        /// The message box contains a symbol consisting of an exclamation inside a yellow triangle.
        Exclamation = 0x30,
        /// The message box contains a symbol consisting of a lowercase letter i in a blue circle.
        Asterisk = 0x40,

        Stop = Hand,
        Error = Hand,
        Warning = Exclamation,
        Information = Asterisk
    }

    /// <summary>
    /// This delegate gets called when MessageBox is closed, notifying about the result
    /// </summary>
    public delegate void MessageBoxResultCallback(MessageBoxResult result);

    /// <summary>
    /// Displays a message box by calling the static *Show* method.  The callback gets called when the
    /// dialog is closed indicating which button was pressed as result.
    /// </summary>
    public class MessageBox : UserControl
    {
        /// <summary>
        /// Displays a message box that has a message and that uses the callback to notify the result.
        /// </summary>
        public static void Show(string text, MessageBoxResultCallback callback)
        {
            Show(text, "", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, callback);
        }

         /// <summary>
        /// Displays a message box that has a message and title bar caption; and that uses the callback
        /// to notify the result.
        /// </summary>
        public static void Show(string text, string caption, MessageBoxResultCallback callback)
        {
            Show(text, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, callback);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and button; and uses the
        /// callback to notify the result.
        /// </summary>
        public static void Show(string text, string caption, MessageBoxButton button,
            MessageBoxResultCallback callback)
        {
            Show(text, caption, button, MessageBoxImage.None, MessageBoxResult.None, callback);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that
        /// uses the callback to notify the result.
        /// </summary>
        public static void Show(string text, string caption, MessageBoxButton button,
            MessageBoxImage icon, MessageBoxResultCallback callback)
        {
            Show(text, caption, button, icon, MessageBoxResult.None, callback);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that
        /// accepts a default message box result and that uses the callback to notify the result.
        /// </summary>
        public static void Show(string text, string caption, MessageBoxButton button,
            MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxResultCallback callback)
        {
            MessageBox messageBox = new MessageBox(text, caption, button, icon, defaultResult, callback);
            Application.Current.MainWindow.AddLayer(messageBox);
        }

        private MessageBox(string text, string caption, MessageBoxButton button, MessageBoxImage icon,
            MessageBoxResult defaultResult, MessageBoxResultCallback callback)
        {
            _callback = callback;

            MessageBoxResult defaultButton = GetDefaultButton(button, defaultResult);

            DelegateCommand close = new DelegateCommand((parameter) =>
            {
                Application.Current.MainWindow.RemoveLayer(this);
                _callback((MessageBoxResult)parameter);
            });

            SetValue(MessageBoxTextProperty, text);
            SetValue(MessageBoxCaptionProperty, caption);
            SetValue(MessageBoxButtonProperty, button);
            SetValue(MessageBoxDefaultButtonProperty, defaultButton);
            SetValue(MessageBoxIconProperty, icon);
            SetValue(MessageBoxCloseProperty, close);

            GUI.LoadComponent(this, "Src/Core/MessageBox.xaml");
        }

        #region Private members

        private static MessageBoxResult GetDefaultButton(MessageBoxButton button, MessageBoxResult defaultResult)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                {
                    return MessageBoxResult.OK;
                }
                case MessageBoxButton.OKCancel:
                {
                    if (defaultResult == MessageBoxResult.Cancel)
                    {
                        return MessageBoxResult.Cancel;
                    }
                    else
                    {
                        return MessageBoxResult.OK;
                    }
                }
                case MessageBoxButton.YesNoCancel:
                {
                    if (defaultResult == MessageBoxResult.Cancel)
                    {
                        return MessageBoxResult.Cancel;
                    }
                    else if (defaultResult == MessageBoxResult.No)
                    {
                        return MessageBoxResult.No;
                    }
                    else
                    {
                        return MessageBoxResult.Yes;
                    }
                }
                case MessageBoxButton.YesNo:
                {
                    if (defaultResult == MessageBoxResult.No)
                    {
                        return MessageBoxResult.No;
                    }
                    else
                    {
                        return MessageBoxResult.Yes;
                    }
                }
                default: return MessageBoxResult.OK;
            }
        }

        private static readonly DependencyProperty MessageBoxTextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MessageBox),
            new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty MessageBoxCaptionProperty = DependencyProperty.Register(
            "Caption", typeof(string), typeof(MessageBox),
            new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty MessageBoxButtonProperty = DependencyProperty.Register(
            "Button", typeof(MessageBoxButton), typeof(MessageBox),
            new PropertyMetadata(MessageBoxButton.OK));

        private static readonly DependencyProperty MessageBoxDefaultButtonProperty = DependencyProperty.Register(
            "DefaultButton", typeof(MessageBoxResult), typeof(MessageBox),
            new PropertyMetadata(MessageBoxResult.None));

        private static readonly DependencyProperty MessageBoxIconProperty = DependencyProperty.Register(
            "Icon", typeof(MessageBoxImage), typeof(MessageBox),
            new PropertyMetadata(MessageBoxImage.None));

        private static readonly DependencyProperty MessageBoxCloseProperty = DependencyProperty.Register(
            "Close", typeof(DelegateCommand), typeof(MessageBox),
            new PropertyMetadata(null));

        private MessageBoxResultCallback _callback;

        #endregion
    }
}
