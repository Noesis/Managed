using Noesis;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NoesisApp
{
    public enum ComparisonConditionType
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    /// <summary>
    /// Represents a trigger that performs actions when the bound data meets a specified condition.
    /// </summary>
    public class DataTrigger : PropertyChangedTrigger
    {
        public new DataTrigger Clone()
        {
            return (DataTrigger)base.Clone();
        }

        public new DataTrigger CloneCurrentValue()
        {
            return (DataTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the binding object that the trigger listens to, and causes to fire actions
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(DataTrigger),
            new PropertyMetadata(null, OnTriggerChanged));

        /// <summary>
        /// Gets or sets the type of comparison to be performed between the specified values
        /// </summary>
        public ComparisonConditionType Comparison
        {
            get { return (ComparisonConditionType)GetValue(ComparisonProperty); }
            set { SetValue(ComparisonProperty, value); }
        }

        public static readonly DependencyProperty ComparisonProperty = DependencyProperty.Register(
            "Comparison", typeof(ComparisonConditionType), typeof(DataTrigger),
            new PropertyMetadata(ComparisonConditionType.Equal, OnTriggerChanged));

        static void OnTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataTrigger trigger = (DataTrigger)d;
            trigger.EvaluateBindingChange(e);
        }

        protected override void OnAttached()
        {
            // Bypass PropertyChangedTrigger.OnAttached because we are going to Evaluate always
            Actions.Attach(AssociatedObject);

            Evaluate(null);
        }

        protected override void EvaluateBindingChange(object args)
        {
            if (AssociatedObject != null)
            {
                DataBindingHelper.EnsureBindingValue(this, BindingProperty);
                DataBindingHelper.EnsureBindingValue(this, ComparisonProperty);
                DataBindingHelper.EnsureBindingValue(this, ValueProperty);

                Evaluate(args);
            }
        }

        private void Evaluate(object args)
        {
            bool sourceChanged = UpdateSourceType();
            bool valueChanged = UpdateTriggerValue();
            bool comparisonChanged = UpdateComparison();

            if (sourceChanged || valueChanged || comparisonChanged)
            {
                if (ComparisonLogic.Evaluate(_binding, _comparison, _value))
                {
                    InvokeActions(args);
                }
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Tested working if the type is registered.")]
        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Tested working if the type is registered.")]
        private bool UpdateSourceType()
        {
            object binding = Binding;
            Type type = binding != null ? binding.GetType() : null;

            if (_sourceType != type)
            {
                _sourceType = type;
                _converter = type != null ? TypeDescriptor.GetConverter(type) : null;
            }

            if (!object.Equals(_binding, binding))
            {
                _binding = binding;
                return true;
            }

            return false;
        }

        private bool UpdateTriggerValue()
        {
            object value = Value;
            Type type = value != null ? value.GetType() : null;

            if (_sourceType != type)
            {
                try
                {
                    if (_converter != null && type != null && _converter.CanConvertFrom(type))
                    {
                        value = _converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                    }
                }
                catch (Exception) { }
            }

            if (!object.Equals(_value, value))
            {
                _value = value;
                return true;
            }

            return false;
        }

        private bool UpdateComparison()
        {
            ComparisonConditionType comparison = Comparison;

            if (_comparison != comparison)
            {
                _comparison = comparison;
                return true;
            }

            return false;
        }

        private Type _sourceType;
        private TypeConverter _converter;
        private object _binding = DependencyProperty.UnsetValue;
        private object _value = DependencyProperty.UnsetValue;
        private ComparisonConditionType _comparison = ComparisonConditionType.Equal;
    }
}
