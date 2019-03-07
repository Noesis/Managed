//---------------------------------------------------------------------------
//
// <copyright file="INotifyCollectionChanged.cs" company="Microsoft">
//    Copyright (C) 2003 by Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: Allows collections to notify listeners of dynamic updates.
//
//  See spec at http://avalon/connecteddata/Specs/INotifyCollectionChanged.mht
//
//---------------------------------------------------------------------------

#if (UNITY_5_5 && (ENABLE_MONO || ENABLE_IL2CPP)) || NET_2_0 || NET_2_0_SUBSET || (UNITY_EDITOR && ENABLE_DOTNET)

namespace System.Collections.Specialized
{
    /// <summary>
    /// A collection implementing this interface will notify listeners of dynamic changes,
    /// e.g. when items get added and removed or the whole list is refreshed.
    /// </summary>
    public interface INotifyCollectionChanged
    {
        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// The event handler receives an argument of type
        /// <seealso cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" />
        /// containing data related to this event.
        /// </remarks>
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}

#endif
