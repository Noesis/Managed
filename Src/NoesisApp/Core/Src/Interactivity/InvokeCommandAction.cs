using Noesis;
using System;
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

        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                ICommand command = ResolveCommand();
                object commandParameter = CommandParameter;
                if (command != null && command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
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

        string _commandName = string.Empty;
    }
}
