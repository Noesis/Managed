using Noesis;
using System;

namespace NoesisApp
{
    internal static class ComparisonLogic
    {
        public static bool Evaluate(object left, ComparisonConditionType comparison, object right)
        {
            if (comparison == ComparisonConditionType.Equal)
            {
                return object.Equals(left, right);
            }
            else if (comparison == ComparisonConditionType.NotEqual)
            {
                return !object.Equals(left, right);
            }
            else
            {
                IComparable leftComparable = left as IComparable;
                IComparable rightComparable = right as IComparable;
                if (leftComparable == null || rightComparable == null)
                {
                    return false;
                }

                switch (comparison)
                {
                    case ComparisonConditionType.LessThan:
                        return leftComparable.CompareTo(rightComparable) < 0;
                    case ComparisonConditionType.LessThanOrEqual:
                        return leftComparable.CompareTo(rightComparable) <= 0;
                    case ComparisonConditionType.GreaterThan:
                        return leftComparable.CompareTo(rightComparable) > 0;
                    case ComparisonConditionType.GreaterThanOrEqual:
                        return leftComparable.CompareTo(rightComparable) >= 0;
                }
            }

            return false;
        }
    }
}
