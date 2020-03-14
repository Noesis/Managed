using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// StringFilterPredicate matching modes
    /// </summary>
    public enum StringFilterMode
    {
        StartsWith,
        EndsWith,
        Contains
    }

    /// <summary>
    /// Predicate that matches a filter string against item text representation.
    /// </summary>
    public class StringFilterPredicate : FilterPredicate
    {
        /// <summary>
        /// Callback that gets the text from the item used to perform the match with the filter
        /// </summary>
        public delegate string GetItemTextCallback(object item);

        /// <summary>
        /// Callback that indicates if filter shold be re-evaluated because item changed
        /// </summary>
        public delegate bool NeedsRefreshCallback(object item, string propertyName);

        public StringFilterPredicate(StringFilterMode mode = StringFilterMode.StartsWith,
            bool isCaseSensitive = false, GetItemTextCallback getItemText = null,
            NeedsRefreshCallback needsRefresh = null)
        {
            _mode = mode;
            _isCaseSensitive = isCaseSensitive;
            _getItemText = getItemText;
            _needsRefresh = needsRefresh;

            if (_getItemText == null)
            {
                // by default we use item ToString()
                _getItemText = (item) => { return item.ToString(); };
            }

            if (_needsRefresh == null)
            {
                // by default changes in item don't affect this predicate
                _needsRefresh = (item, propertyName) => { return false; };
            }
        }

        /// <summary>
        /// String filter used to match item text representation
        /// </summary>
        public string Filter
        {
            get { return _filter; }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    Refresh();
                }
            }
        }

        #region FilterPredicate implementation
        public override bool Matches(object item)
        {
            string itemStr = _getItemText(item);
            StringComparison caseSensitive = _isCaseSensitive ?
                StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            switch (_mode)
            {
                default:
                case StringFilterMode.StartsWith: return itemStr.StartsWith(_filter, caseSensitive);
                case StringFilterMode.EndsWith: return itemStr.EndsWith(_filter, caseSensitive);
                case StringFilterMode.Contains: return itemStr.IndexOf(_filter, caseSensitive) != -1;
            }
        }

        public override bool NeedsRefresh(object item, string propertyName)
        {
            return _needsRefresh(item, propertyName);
        }
        #endregion

        #region Private members
        private StringFilterMode _mode;
        private bool _isCaseSensitive;
        private GetItemTextCallback _getItemText;
        private NeedsRefreshCallback _needsRefresh;
        private string _filter;
        #endregion
    }
}
