namespace Noesis
{
    /// <summary>
    /// Used for resurrection of dependency properties.
    /// </summary>
    internal struct StoredPropertyMetadataEntry
    {
        public readonly CoerceValueCallback CoerceValueCallback;

        public readonly object DefaultValue;

        public readonly string DependencyPropertyName;

        public readonly PropertyChangedCallback PropertyChangedCallback;

        public StoredPropertyMetadataEntry(DependencyProperty dependencyProperty, PropertyMetadata metadata)
        {
            this.DependencyPropertyName = dependencyProperty.Name;
            this.DefaultValue = metadata.DefaultValue;
            this.PropertyChangedCallback = metadata.PropertyChangedCallback;
            this.CoerceValueCallback = metadata.CoerceValueCallback;
        }

        public PropertyMetadata CreateFrameworkPropertyMetadata()
        {
            return new FrameworkPropertyMetadata(this.DefaultValue,
                                                 this.PropertyChangedCallback,
                                                 this.CoerceValueCallback);
        }
    }
}