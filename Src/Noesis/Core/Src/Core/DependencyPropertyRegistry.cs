using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using System.Linq;

namespace Noesis
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Manages Noesis Extensibility
    ////////////////////////////////////////////////////////////////////////////////////////////////
    internal class DependencyPropertyRegistry
    {
        public static void Register(DependencyProperty dp, string name, Type propertyType, Type ownerType, PropertyMetadata metadata)
        {
            List<PropertyEntry> properties;
            WeakType key = new WeakType(ownerType);
            if (!_typeProperties.TryGetValue(key, out properties))
            {
                properties = new List<PropertyEntry>();
                _typeProperties[key] = properties;
            }

            properties.Add(new PropertyEntry(name, dp, propertyType, metadata));
        }

        public static void Override(DependencyProperty dp, Type forType, PropertyMetadata metadata)
        {
            List<MetadataEntry> metadatas;
            WeakType key = new WeakType(forType);
            if (!_typePropertyOverrides.TryGetValue(key, out metadatas))
            {
                metadatas = new List<MetadataEntry>();
                _typePropertyOverrides[key] = metadatas;
            }

            metadatas.Add(new MetadataEntry(dp, metadata));
        }

        public static void Restore(Type type)
        {
            RestoreDependencyProperties(type);
            RestoreDependencyPropertyOverrides(type);
        }

        #region Private members
        private static void RestoreDependencyProperties(Type ownerType)
        {
            List<PropertyEntry> properties;
            WeakType key = new WeakType(ownerType);
            if (_typeProperties.TryGetValue(key, out properties))
            {
                foreach (PropertyEntry prop in properties)
                {
                    Type propertyType = prop.Type;
                    if (propertyType != null)
                    {
                        DependencyProperty.RegisterCommon(prop.Name, propertyType, ownerType, prop.Metadata.Create(), prop.Property);
                    }
                }
            }
        }

        private static void RestoreDependencyPropertyOverrides(Type forType)
        {
            List<MetadataEntry> metadatas;
            WeakType key = new WeakType(forType);
            if (_typePropertyOverrides.TryGetValue(key, out metadatas))
            {
                // Remove the reference as new overrides will be created during the following OverrideMetadata calls
                _typePropertyOverrides.Remove(key);

                // Perform DependencyProperty overrides
                foreach (MetadataEntry metadata in metadatas)
                {
                    PropertyInfo pi = forType.GetProperty(metadata.PropertyName + "Property", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (pi == null)
                    {
                        Log.Error("DependencyProperty '" + forType.FullName + "." + metadata.PropertyName + "' not found.");
                        continue;
                    }

                    DependencyProperty dp = pi.GetValue(null) as DependencyProperty;
                    if (dp == null)
                    {
                        Log.Error("DependencyProperty '" + forType.FullName + "." + metadata.PropertyName + "' not found.");
                        continue;
                    }

                    if (dp.IsDisposed)
                    {
                        Log.Error("DependencyProperty '" + forType.FullName + "." + metadata.PropertyName + "' already disposed.");
                        continue;
                    }

                    dp.OverrideMetadata(forType, metadata.Create());
                }
            }
        }

        private static readonly Dictionary<WeakType, List<PropertyEntry>> _typeProperties = new Dictionary<WeakType, List<PropertyEntry>>();
        private static readonly Dictionary<WeakType, List<MetadataEntry>> _typePropertyOverrides = new Dictionary<WeakType, List<MetadataEntry>>();
        #endregion

        #region WeakType
        private struct WeakType : IEquatable<WeakType>
        {
            public Type Type { get { return (Type)this.weakReference.Target; } }

            public WeakType(Type type)
            {
                this.weakReference = new WeakReference(type);
                this.hashCode = type.GetHashCode();
            }

            public bool Equals(WeakType other)
            {
                if (this.hashCode == other.hashCode)
                {
                    Type typeA = (Type)this.weakReference.Target;
                    Type typeB = (Type)other.weakReference.Target;
                    return typeA == typeB;
                }

                return false;
            }

            public override bool Equals(object obj)
            {
                return obj is WeakType && this.Equals((WeakType)obj);
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            private readonly int hashCode;
            private readonly WeakReference weakReference;
        }
        #endregion

        #region PropertyEntry

        private struct PropertyEntry
        {
            public readonly string Name;
            public DependencyProperty Property { get { return (DependencyProperty)weakProperty.Target; } }
            public Type Type { get { return (Type)weakType.Target; } }
            public readonly MetadataEntry Metadata;

            public PropertyEntry(string name, DependencyProperty property, Type type, PropertyMetadata metadata)
            {
                this.Name = name;
                this.weakProperty = new WeakReference(property);
                this.weakType = new WeakReference(type);
                this.Metadata = new MetadataEntry(property, metadata);
            }

            private WeakReference weakProperty;
            private WeakReference weakType;
        }
        #endregion

        #region MetadataEntry
        private struct MetadataEntry
        {
            public readonly string PropertyName;

            public MetadataEntry(DependencyProperty property, PropertyMetadata metadata)
            {
                this.PropertyName = property.Name;
                this.DefaultValue = metadata.DefaultValue;
                this.PropertyChangedCallback = metadata.PropertyChangedCallback;
                this.CoerceValueCallback = metadata.CoerceValueCallback;
                this.options = FrameworkPropertyMetadataOptions.None;
                if (metadata is FrameworkPropertyMetadata)
                {
                    this.type = MetadataType.Framework;

                    FrameworkPropertyMetadata metadata_ = (FrameworkPropertyMetadata)metadata;
                    if (metadata_.AffectsMeasure)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.AffectsMeasure;
                    }
                    if (metadata_.AffectsArrange)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.AffectsArrange;
                    }
                    if (metadata_.AffectsParentMeasure)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.AffectsParentMeasure;
                    }
                    if (metadata_.AffectsParentArrange)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.AffectsParentArrange;
                    }
                    if (metadata_.AffectsRender)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.AffectsRender;
                    }
                    if (metadata_.Inherits)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.Inherits;
                    }
                    if (metadata_.OverridesInheritanceBehavior)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior;
                    }
                    if (metadata_.IsNotDataBindable)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.NotDataBindable;
                    }
                    if (metadata_.BindsTwoWayByDefault)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
                    }
                    if (metadata_.Journal)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.Journal;
                    }
                    if (metadata_.SubPropertiesDoNotAffectRender)
                    {
                        this.options |= FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;
                    }
                }
                else if (metadata is UIPropertyMetadata)
                {
                    this.type = MetadataType.UI;
                }
                else
                {
                    this.type = MetadataType.Base;
                }
            }

            public PropertyMetadata Create()
            {
                switch (type)
                {
                    default:
                    case MetadataType.Base:
                        return new PropertyMetadata(this.DefaultValue, this.PropertyChangedCallback, this.CoerceValueCallback);
                    case MetadataType.UI:
                        return new UIPropertyMetadata(this.DefaultValue, this.PropertyChangedCallback, this.CoerceValueCallback);
                    case MetadataType.Framework:
                        return new FrameworkPropertyMetadata(this.DefaultValue, this.options, this.PropertyChangedCallback, this.CoerceValueCallback);
                }
            }

            private enum MetadataType
            {
                Base,
                UI,
                Framework
            }

            private readonly object DefaultValue;
            private readonly PropertyChangedCallback PropertyChangedCallback;
            private readonly CoerceValueCallback CoerceValueCallback;
            private readonly MetadataType type;
            private readonly FrameworkPropertyMetadataOptions options;
        }
        #endregion
    }
}
