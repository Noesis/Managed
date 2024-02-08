using Noesis;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;

namespace NoesisApp
{
    /// <summary>
    /// Executes a specified ICommand when invoked.
    /// </summary>
    public class InvokeCommandAction : TriggerAction<DependencyObject>
    {
        public new InvokeCommandAction Clone()
        {
            return (InvokeCommandAction)base.Clone();
        }

        public new InvokeCommandAction CloneCurrentValue()
        {
            return (InvokeCommandAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the name of the command this action should invoke
        /// </summary>
        public string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        /// <summary>
        /// Gets or sets the command this action should invoke. Has more priority than CommandName
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(InvokeCommandAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command parameter
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(InvokeCommandAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the property path used to extract a value from the EventArgs to pass to the
        /// Command as a parameter. If the CommandParameter propert is set, this property is ignored
        /// </summary>
        public string EventArgsParameterPath
        {
            get { return (string)GetValue(EventArgsParameterPathProperty); }
            set { SetValue(EventArgsParameterPathProperty, value); }
        }

        public static readonly DependencyProperty EventArgsParameterPathProperty = DependencyProperty.Register(
            "EventArgsParameterPath", typeof(string), typeof(InvokeCommandAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the IValueConverter that is used to convert the EventArgs passed to the
        /// Command as a parameter. If the CommandParameter or EventArgsParameterPath properties are
        /// set, this property is ignored
        /// </summary>
        public Noesis.IValueConverter EventArgsConverter
        {
            get { return (Noesis.IValueConverter)GetValue(EventArgsConverterProperty); }
            set { SetValue(EventArgsConverterProperty, value); }
        }

        public static readonly DependencyProperty EventArgsConverterProperty = DependencyProperty.Register(
            "EventArgsConverter", typeof(Noesis.IValueConverter), typeof(InvokeCommandAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the parameter that is passed to the EventArgsConverter
        /// </summary>
        public object EventArgsConverterParameter
        {
            get { return GetValue(EventArgsConverterParameterProperty); }
            set { SetValue(EventArgsConverterParameterProperty, value); }
        }

        public static readonly DependencyProperty EventArgsConverterParameterProperty = DependencyProperty.Register(
            "EventArgsConverterParameter", typeof(object), typeof(InvokeCommandAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Specifies whether the EventArgs of the event that triggered this action should be passed to
        /// the Command as a parameter. If the Command, EventArgsParameterPath, or EventArgsConverter
        /// properties are set, this property is ignored
        /// </summary>
        public bool PassEventArgsToCommand
        {
            get; set;
        }

        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                ICommand command = ResolveCommand();
                if (command != null)
                {
                    object commandParam = CommandParameter;

                    // If no CommandParameter has been provided, let's check the EventArgsParameterPath
                    if (commandParam == null && parameter != null)
                    {
                        string path = EventArgsParameterPath;
                        if (!string.IsNullOrEmpty(path))
                        {
                            commandParam = ResolvePath(path, parameter);
                        }
                    }

                    // Next let's see if an event args converter has been supplied
                    if (commandParam == null && parameter != null)
                    {
                        Noesis.IValueConverter converter = EventArgsConverter;
                        if (converter != null)
                        {
                            commandParam = converter.Convert(parameter, typeof(object),
                                EventArgsConverterParameter, CultureInfo.CurrentCulture);
                        }
                    }

                    // Last resort, let see if they want to force the event args to be passed as a parameter
                    if (commandParam == null && PassEventArgsToCommand)
                    {
                        commandParam = parameter;
                    }

                    if (command.CanExecute(commandParam))
                    {
                        command.Execute(commandParam);
                    }
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = Command;
            if (command != null)
            {
                return command;
            }

            if (!string.IsNullOrEmpty(_commandName))
            {
                DependencyObject associatedObject = AssociatedObject;

                Type type = associatedObject.GetType();
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    if (typeof(ICommand).IsAssignableFrom(propertyInfo.PropertyType) &&
                        string.Equals(propertyInfo.Name, _commandName, StringComparison.Ordinal))
                    {
                        return (ICommand)propertyInfo.GetValue(associatedObject, null);
                    }
                }
            }

            return null;
        }

        private object ResolvePath(string path, object parameter)
        {
            object commandParameter;
            object propertyValue = parameter;
            string[] parts = path.Split('.');
            foreach (string part in parts)
            {
                PropertyInfo propInfo = propertyValue.GetType().GetProperty(part);
                propertyValue = propInfo.GetValue(propertyValue, null);
            }

            commandParameter = propertyValue;
            return commandParameter;
        }

        private string _commandName = string.Empty;
    }
}
