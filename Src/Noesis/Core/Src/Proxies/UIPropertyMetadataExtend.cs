using System;
using System.Runtime.InteropServices;

namespace Noesis
{

    public partial class UIPropertyMetadata
    {
        public UIPropertyMetadata() :
            this(Noesis_UIPropertyMetadata_Create(), true)
        {
        }

        public UIPropertyMetadata(PropertyChangedCallback propertyChangedCallback) :
            this()
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public UIPropertyMetadata(object defaultValue) :
            this()
        {
            this.DefaultValue = defaultValue;
        }

        public UIPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback) :
            this(defaultValue)
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public UIPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) :
            this(defaultValue, propertyChangedCallback)
        {
            this.CoerceValueCallback = coerceValueCallback;
        }

        public UIPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited) :
            this(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
        }

        #region Imports

        static UIPropertyMetadata()
        {
            Noesis.GUI.Init();
        }

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_UIPropertyMetadata_Create();

        #endregion
    }

}
