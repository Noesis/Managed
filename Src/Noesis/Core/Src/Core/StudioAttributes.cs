using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public abstract class StudioBaseAttribute : Attribute { }

/// <summary>
/// Overrides the default property inspector control in Noesis Studio.
/// By assigning a specific string identifier (e.g., "NumericProperty.Percentage" 
/// or "NumericProperty.Rotation"), developers can dictate exactly which custom 
/// UI widget Studio uses to display and edit the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class StudioEditorAttribute : StudioBaseAttribute
{
    public StudioEditorAttribute(string editor) { Editor = editor; }

    public string Editor { get; }
}

/// <summary>
/// Specifies a custom, high-level display name for a class or property within the Noesis Studio
/// interface, overriding the default type name.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class StudioNameAttribute : StudioBaseAttribute
{
    public StudioNameAttribute(string name) { Name = name; }

    public string Name { get; }
}

/// <summary>
/// Provides a detailed description or tooltip for a class to be displayed within the Noesis Studio
/// interface, helping users understand its purpose.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StudioDescAttribute : StudioBaseAttribute
{
    public StudioDescAttribute(string description) { Description = description; }

    public string Description { get; }
}

/// <summary>
/// Specifies a URI linking to online documentation or help resources for a class, allowing users
/// to easily access more information from Noesis Studio.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class StudioHelpUriAttribute : StudioBaseAttribute
{
    public StudioHelpUriAttribute(string uri) { HelpUri = uri; }

    public string HelpUri { get; }
}

/// <summary>
/// Assigns a vector icon (using a specific font string/URI and glyph character code) to visually
/// represent the class within the Noesis Studio interface, such as in the toolbox or element tree.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class StudioIconAttribute : StudioBaseAttribute
{
    public StudioIconAttribute(string font, int ch) { Font = font; Char = ch; }

    public string Font { get; }
    public int Char { get; }
}

/// <summary>
/// Specifies the minimum allowed value for a numeric property. Noesis Studio uses this to
/// constrain user input in the property inspector (e.g., setting the lower bound of a slider or
/// numeric text box).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class StudioMinAttribute : StudioBaseAttribute
{
    public StudioMinAttribute(float min) { Min = min; }
    public StudioMinAttribute(int min) { Min = min; }

    public float Min { get; }
}

/// <summary>
/// Defines both the minimum and maximum allowed values for a numeric property. Noesis Studio uses
/// this to constrain user input in the property inspector, typically presenting the property as
/// a slider clamped to this specified range.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class StudioRangeAttribute : StudioBaseAttribute
{
    public StudioRangeAttribute(float min, float max) { Min = min; Max = max; }
    public StudioRangeAttribute(int min, int max) { Min = min; Max = max; }

    public float Min { get; }
    public float Max { get; }
}

/// <summary>
/// Specifies the incremental step value used when modifying a numeric property in Noesis Studio.
/// This dictates how much the value changes when dragging the property with the mouse.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class StudioStepAttribute : StudioBaseAttribute
{
    public StudioStepAttribute(float step) { Step = step; }
    public StudioStepAttribute(int step) { Step = step; }

    public float Step { get; }
}

/// <summary>
/// Specifies the display order, visibility grouping, and categorization for a property or class
/// within the Noesis Studio inspector. Primary items are always visible, whereas secondary items
/// are hidden behind an expander. The category determines which group the element belongs to.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StudioOrderAttribute : StudioBaseAttribute
{
    public StudioOrderAttribute(int order) { Order = order; Primary = true; Category = ""; }
    public StudioOrderAttribute(int order, string category) { Order = order; Primary = true; Category = category; }
    public StudioOrderAttribute(int order, bool primary) { Order = order; Primary = primary; Category = ""; }
    public StudioOrderAttribute(int order, bool primary, string category) { Order = order; Primary = primary; Category = category; }

    public int Order { get; }
    public bool Primary { get; }
    public string Category { get; }
}

/// <summary>
/// Defines a list of named presets for a float property. Noesis Studio uses  this to populate a
/// dropdown or quick-select list in the inspector, allowing users to easily choose predefined,
/// meaningful values (e.g., "Center" = 0.5f) instead of manually entering numeric values.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class StudioFloatPresetsAttribute : StudioBaseAttribute
{
    public string[] Names { get; }
    public float[] Values { get; }

    // Friendly usage: [StudioFloatPresets("Left", 0.0f, "Center", 0.5f, "Right", 1.0f)]
    public StudioFloatPresetsAttribute(params object[] nameValuePairs)
    {
        if (nameValuePairs == null || nameValuePairs.Length % 2 != 0)
        {
            throw new ArgumentException("Presets must be provided in pairs of (string name, float value).");
        }

        int count = nameValuePairs.Length / 2;
        Names = new string[count];
        Values = new float[count];

        for (int i = 0; i < count; i++)
        {
            Names[i] = (string)nameValuePairs[i * 2];
            Values[i] = Convert.ToSingle(nameValuePairs[i * 2 + 1]); 
        }
    }
}

/// <summary>
/// Restricts which specific element types this attached property can be applied to within the
/// Noesis Studio inspector.
/// This attribute must be applied to the static <c>Get</c> method of the attached property.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StudioAttachToAttribute : StudioBaseAttribute
{
    public Type[] Types { get; }

    public StudioAttachToAttribute(params Type[] types)
    {
        Types = types ?? new Type[0];
    }
}

/// <summary>
/// Restricts the visibility of this attached property in the Noesis Studio inspector based on the
/// parent context (e.g., Grid.Row only appears for children of a Grid).
/// this attribute must be applied to the static <c>Get</c> method of the attached property.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StudioAttachOnAttribute : StudioBaseAttribute
{
    public Type[] Types { get; }

    public StudioAttachOnAttribute(params Type[] types)
    {
        Types = types ?? new Type[0];
    }
}

/// <summary>
/// Marks an attached property as read-only in the Noesis Studio inspector, preventing users from
/// modifying its value.
/// This attribute must be applied to the static <c>Get</c> method of the attached property.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StudioAttachReadOnlyAttribute : StudioBaseAttribute
{
}

/// <summary>
/// Specifies the name of a method to be executed for custom initialization when an instance of this
/// class is created (e.g., dragged and dropped from the toolbox) within the Noesis Studio editor.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class StudioInitAttribute : StudioBaseAttribute
{
    public string MethodName { get; }

    public StudioInitAttribute(string methodName)
    {
        MethodName = methodName;
    }
}

/// <summary>
/// Specifies that a property represents a URI of a specific type.
/// Noesis Studio uses this to provide appropriate asset pickers
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class StudioUriAttribute : StudioBaseAttribute
{
    public enum UriType
    {
        None,
        Font,
        Image,
        Audio,
        Video,
        Media,
        Xaml,
        Component,
        ResourceDictionary,
        Rive
    }

    public StudioUriAttribute(UriType type) 
    { 
        Type = type; 
    }

    public UriType Type { get; }
}

}