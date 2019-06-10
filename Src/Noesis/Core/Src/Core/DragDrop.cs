using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Noesis
{
    public delegate void DragDropCompletedCallback(DependencyObject source, IDataObject data,
        UIElement target, Point dropPoint, DragDropEffects effects);

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

        #region IDataObject
        public object GetData(Type format) { return _data; }
        public void SetData(Type format, object data) { _data = data; }
        #endregion

        private object _data;
    }
}

