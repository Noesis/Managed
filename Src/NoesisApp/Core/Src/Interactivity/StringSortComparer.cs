using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Compares the text representation of two items for sorting.
    /// </summary>
    public class StringSortComparer : SortComparer
    {
        /// <summary>
        /// Callback that gets the text from the item used to compare list items
        /// </summary>
        public delegate string GetItemTextCallback(object item);

        /// <summary>
        /// Callback that indicates if sort shold be re-evaluated because item changed
        /// </summary>
        public delegate bool NeedsRefreshCallback(object item, string propertyName);

        public StringSortComparer(bool isCaseSensitive = false, GetItemTextCallback getItemText = null,
            NeedsRefreshCallback needsRefresh = null)
        {
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
                // by default changes in item don't affect this comparer
                _needsRefresh = (item, propertyName) => { return false; };
            }
        }

        #region SortComparer implementation
        public override int Compare(object i0, object i1)
        {
            string i0Str = _getItemText(i0);
            string i1Str = _getItemText(i1);
            return string.Compare(i0Str, i1Str, _isCaseSensitive == false);
        }

        public override bool NeedsRefresh(object item, string propertyName)
        {
            return _needsRefresh(item, propertyName);
        }
        #endregion

        #region Private members
        private bool _isCaseSensitive;
        private GetItemTextCallback _getItemText;
        private NeedsRefreshCallback _needsRefresh;
        #endregion
    }
}
