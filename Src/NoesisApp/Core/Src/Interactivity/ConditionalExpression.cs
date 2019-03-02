using Noesis;

namespace NoesisApp
{
    public enum ForwardChaining
    {
        And,
        Or
    };

    public class ConditionCollection : Noesis.FreezableCollection<ComparisonCondition>
    {
    }

    /// <summary>
    /// Represents a conditional expression that is set on a ConditionBehavior Condition property.
    /// Contains a list of conditions that gets evaluated in order.
    /// </summary>
    [ContentProperty("Conditions")]
    public class ConditionalExpression : Noesis.Animatable, ICondition
    {
        public ForwardChaining ForwardChaining
        {
            get { return (ForwardChaining)GetValue(ForwardChainingProperty); }
            set { SetValue(ForwardChainingProperty, value); }
        }

        public static readonly DependencyProperty ForwardChainingProperty = DependencyProperty.Register(
            "ForwardChaining", typeof(ForwardChaining), typeof(ConditionalExpression),
            new PropertyMetadata(ForwardChaining.And));

        public ConditionCollection Conditions
        {
            get { return (ConditionCollection)GetValue(ConditionsProperty); }
        }

        public static readonly DependencyProperty ConditionsProperty = DependencyProperty.Register(
            "Conditions", typeof(ConditionCollection), typeof(ConditionalExpression),
            new PropertyMetadata(null));


        bool ICondition.Evaluate()
        {
            bool flag = false;

            ForwardChaining chaining = ForwardChaining;
            ConditionCollection conditions = Conditions;
            int numConditions = conditions.Count;
            for (int i = 0; i < numConditions; ++i)
            {
                ComparisonCondition condition = conditions[i];

                flag = condition.Evaluate();

                if (!flag && chaining == ForwardChaining.And)
                {
                    return flag;
                }
                if (flag && chaining == ForwardChaining.Or)
                {
                    return flag;
                }
            }

            return flag;
        }
    }
}
