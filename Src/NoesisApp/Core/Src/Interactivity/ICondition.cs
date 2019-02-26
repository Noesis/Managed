using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Interface implemented by objects set in ConditionBehavior.Condition property.
    /// </summary>
    public interface ICondition
    {
        bool Evaluate();
    }
}
