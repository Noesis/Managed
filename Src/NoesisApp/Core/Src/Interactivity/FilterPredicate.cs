using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    /// Event handler type for FilterRequired event
    public delegate void FilterRequiredEventHandler();

    /// <summary>
    /// Determines if an item of a list should be filtered out or not
    /// </summary>
    public abstract class FilterPredicate
    {
        /// <summary>
        /// Called to evaluate the predicate against the provided item
        /// </summary>
        public abstract bool Matches(object item);

        /// <summary>
        /// Indicates if filter should be re-evaluated after an item property change
        /// </summary>
        public abstract bool NeedsRefresh(object item, string propertyName);

        /// <summary>
        /// Re-evaluates the filter over the list
        /// </summary>
        public void Refresh()
        {
            FilterRequired?.Invoke();
        }

        /// <summary>
        /// Event that indicates that conditions changed and sort needs to be re-evaluated
        /// </summary>
        public event FilterRequiredEventHandler FilterRequired;
    }
}
