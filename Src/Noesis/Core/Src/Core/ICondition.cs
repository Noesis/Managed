using System;

namespace Noesis.Interactivity
{
    /// <summary>
    /// An interface that a given object must implement in order to be 
    /// set on a ConditionBehavior.Condition property.
    /// </summary>
    public interface ICondition
    {
        bool Evaluate();
    }
}
