using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public partial class FrameworkPropertyMetadata
    {
        public FrameworkPropertyMetadata() :
            this(Noesis_FrameworkPropertyMetadata_Create(), true)
        {
        }

        public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback) :
            this()
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback,
            CoerceValueCallback coerceValueCallback) :
            this(propertyChangedCallback)
        {
            this.CoerceValueCallback = coerceValueCallback;
        }

        public FrameworkPropertyMetadata(object defaultValue) :
            this()
        {
            this.DefaultValue = defaultValue;
        }

        public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback) :
            this(defaultValue)
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) :
            this(defaultValue, propertyChangedCallback)
        {
            this.CoerceValueCallback = coerceValueCallback;
        }

        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags) :
            this(defaultValue)
        {
            this.TranslateFlags(flags);
        }

        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback) :
            this(defaultValue, flags)
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) :
            this(defaultValue, flags, propertyChangedCallback)
        {
            this.CoerceValueCallback = coerceValueCallback;
        }

        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited) :
            this(defaultValue, flags, propertyChangedCallback, coerceValueCallback)
        {
        }

        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited, UpdateSourceTrigger defaultUpdateSourceTrigger) :
            this(defaultValue, flags, propertyChangedCallback, coerceValueCallback)
        {
            this.DefaultUpdateSourceTrigger = defaultUpdateSourceTrigger;
        }

        private void TranslateFlags(FrameworkPropertyMetadataOptions flags)
        {
            if ((flags & FrameworkPropertyMetadataOptions.AffectsMeasure) != 0)
            {
                this.AffectsMeasure = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.AffectsArrange) != 0)
            {
                this.AffectsArrange = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.AffectsParentMeasure) != 0)
            {
                this.AffectsParentMeasure = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.AffectsParentArrange) != 0)
            {
                this.AffectsParentArrange = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.AffectsRender) != 0)
            {
                this.AffectsRender = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.Inherits) != 0)
            {
                this.Inherits = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior) != 0)
            {
                this.OverridesInheritanceBehavior = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.NotDataBindable) != 0)
            {
                this.IsNotDataBindable = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.BindsTwoWayByDefault) != 0)
            {
                this.BindsTwoWayByDefault = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.Journal) != 0)
            {
                this.Journal = true;
            }
            if ((flags & FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender) != 0)
            {
                this.SubPropertiesDoNotAffectRender = true;
            }
        }

        #region Imports

        static FrameworkPropertyMetadata()
        {
            Noesis.GUI.Init();
        }

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_FrameworkPropertyMetadata_Create();

        #endregion
    }
}
