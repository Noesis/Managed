using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public delegate void DragDropCompletedCallback(DependencyObject source, IDataObject data,
        UIElement target, Point dropPoint, DragDropEffects effects);

    public delegate void DataObjectCopyingEventHandler(object sender, DataObjectCopyingEventArgs e);

    public delegate void DataObjectPastingEventHandler(object sender, DataObjectPastingEventArgs e);

    public interface IDataObject
    {
        //object GetData(string format);
        object GetData(Type format);
        //object GetData(string format, bool autoConvert);
        //bool GetDataPresent(string format);
        //bool GetDataPresent(Type format);
        //bool GetDataPresent(string format, bool autoConvert);
        //string[] GetFormats();
        //string[] GetFormats(bool autoConvert);
        //void SetData(object data);
        //void SetData(string format, object data);
        void SetData(Type format, object data);
        //void SetData(string format, object data, bool autoConvert);
    }

    internal struct DataObject : IDataObject
    {
        public DataObject(object data) { _data = data; }

        public static RoutedEvent CopyingEvent {
          get {
            IntPtr cPtr = DataObject_CopyingEvent_get();
            return (RoutedEvent)Noesis.Extend.GetProxy(cPtr, false);
          }
        }

        public static RoutedEvent PastingEvent {
          get {
            IntPtr cPtr = DataObject_PastingEvent_get();
            return (RoutedEvent)Noesis.Extend.GetProxy(cPtr, false);
          }
        }

        /// <summary>Adds a DataObject.Copying event handler to a specified element.</summary>
        public static void AddCopyingHandler(DependencyObject d, DataObjectCopyingEventHandler handler) {
          ((UIElement)d).AddHandler(CopyingEvent, handler);
        }

        /// <summary>Adds a DataObject.Pasting event handler to a specified element.</summary>
        public static void AddPastingHandler(DependencyObject d, DataObjectPastingEventHandler handler) {
          ((UIElement)d).AddHandler(PastingEvent, handler);
        }

        /// <summary>Removes a DataObject.Copying event handler from a specified element.</summary>
        public static void RemoveCopyingHandler(DependencyObject d, DataObjectCopyingEventHandler handler) {
          ((UIElement)d).RemoveHandler(CopyingEvent, handler);
        }

        /// <summary>Removes a DataObject.Pasting event handler from a specified element.</summary>
        public static void RemovePastingHandler(DependencyObject d, DataObjectPastingEventHandler handler) {
          ((UIElement)d).RemoveHandler(PastingEvent, handler);
        }

        #region IDataObject
        public object GetData(Type format) { return _data; }
        public void SetData(Type format, object data) { _data = data; }
        #endregion

        #region Private members
        private object _data;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        private static extern IntPtr DataObject_CopyingEvent_get();

        [DllImport(Library.Name)]
        private static extern IntPtr DataObject_PastingEvent_get();
        #endregion
    }
}

