using Noesis;
using System;
using System.ComponentModel;
using System.Globalization;

namespace NoesisApp
{
    /// <summary>
    /// Represents one ternary condition: left operand - operator - right operand. Used as condition
    /// in the list of conditions on a ConditionalExpression.
    /// </summary>
    public class ComparisonCondition : Noesis.Animatable
    {
        public object LeftOperand
        {
            get { return GetValue(LeftOperandProperty); }
            set { SetValue(LeftOperandProperty, value); }
        }

        public static readonly DependencyProperty LeftOperandProperty = DependencyProperty.Register(
            "LeftOperand", typeof(object), typeof(ComparisonCondition),
            new PropertyMetadata(null));

        public ComparisonConditionType Operator
        {
            get { return (ComparisonConditionType)GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register(
            "Operator", typeof(ComparisonConditionType), typeof(ComparisonCondition),
            new PropertyMetadata(ComparisonConditionType.Equal));

        public object RightOperand
        {
            get { return GetValue(RightOperandProperty); }
            set { SetValue(RightOperandProperty, value); }
        }

        public static readonly DependencyProperty RightOperandProperty = DependencyProperty.Register(
            "RightOperand", typeof(object), typeof(ComparisonCondition),
            new PropertyMetadata(null));

        public bool Evaluate()
        {
            EnsureBindingValues();
            EnsureOperands();
            return ComparisonLogic.Evaluate(_left, Operator, _right);
        }

        private void EnsureBindingValues()
        {
            DataBindingHelper.EnsureBindingValue(this, LeftOperandProperty);
            DataBindingHelper.EnsureBindingValue(this, OperatorProperty);
            DataBindingHelper.EnsureBindingValue(this, RightOperandProperty);
        }

        private void EnsureOperands()
        {
            // ensure left operand
            object left = LeftOperand;
            Type leftType = left != null ? left.GetType() : null;

            if (_sourceType != leftType)
            {
                _sourceType = leftType;
                _converter = leftType != null ? TypeDescriptor.GetConverter(leftType) : null;
            }

            _left = left;

            // ensure right operand
            object right = RightOperand;
            Type rightType = right != null ? right.GetType() : null;

            if (_sourceType != rightType)
            {
                try
                {
                    if (_converter != null && rightType != null && _converter.CanConvertFrom(rightType))
                    {
                        right = _converter.ConvertFrom(null, CultureInfo.InvariantCulture, right);
                    }
                }
                catch (Exception) { }
            }

            _right = right;
        }

        private Type _sourceType = null;
        private TypeConverter _converter = null;
        private object _left;
        private object _right;
    }
}
