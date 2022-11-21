using System;
using System.Collections.Generic;
using System.Text;
using Noesis;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Adds a *Text* attached property for TextBlock which formats
    /// BBCode <https://www.bbcode.org/reference.php> into Inlines.
    ///
    /// Default supported BBCode tags, with their Inline output:
    ///
    /// * *b*: Bold
    ///
    /// * *i*: Italic
    ///
    /// * *u*: Underline
    ///
    /// * *size*: Span with FontSize set to the parameter value
    ///
    /// * *font*: Span with FontFamily set to the parameter value
    ///
    /// * *color*: Span with Foreground set to the parameter value
    ///
    /// * *style*: Span with the Style property set to the resource key provided by the parameter value
    ///
    /// * *img*: Image contained in an InlineUIContainer
    ///
    /// * *bind*: Run containing a Binding with the Path property set to the tag contents. This tag
    ///   has an optional "format" parameter which can be used to modify the StringFormat property
    ///   of the Binding.
    ///
    ///
    /// Usage:
    ///
    ///    <Grid
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
    ///      <TextBlock noesis:RichText.Text="Plain. [b]Bold, [i]bold italic, [/i]
    ///        [size=60]Size 60 text.[/size] [img height=80]disk.png[/img]" />
    ///    </Grid>
    ///
    ///
    /// Usage:
    ///
    ///    <Grid
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
    ///      <Grid.Resources>
    ///        <Style x:Key="Header1" TargetType="{x:Type Span}">
    ///          <Setter Property="FontSize" Value="30"/>
    ///          <Setter Property="Foreground" Value="Green"/>
    ///        </Style>
    ///      </Grid.Resources>
    ///      <TextBlock noesis:RichText.Text="[style='Header1']Styled text.[/style]" />
    ///    </Grid>
    ///
    ///
    /// Usage:
    ///
    ///    <Grid
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions"
    ///      xmlns:local="clr-namespace:MyGame">
    ///      <Grid.DataContext>
    ///        <local:MyViewModel CurrentHealth="66.75" MaxHealth="100" />
    ///      </Grid.DataContext>
    ///      <TextBlock noesis:RichText.Text="Health is [bind format='{0:0}']CurrentHealth[/bind] out of
    ///        [bind format='{0:0}']MaxHealth[/bind]" />
    ///    </Grid>
    /// 
    /// </summary>
    public static class RichText
    {
        /// <summary>
        /// Gets or sets the BBCode text to apply to a TextBlock as formatted Inlines. 
        ///
        /// Usage:
        ///
        ///    <Grid
        ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions">
        ///      <TextBlock noesis:RichText.Text="Plain. [b]Bold, [i]bold italic, [/i]
        ///         [bind format='{0:#,#.0}']Path[/bind]
        ///         [size=60]Size 60 text.[/size] [img height=80]disk.png[/img]" />
        ///    </Grid>
        ///
        /// </summary>
        #region Text attached property

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string),
                typeof(RichText),
                new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            if (obj is TextBlock textBlock)
            {
                InlineCollection collection = textBlock.Inlines;
                collection.Clear();

                string newValue = (string)args.NewValue;
                TryParse(newValue.AsSpan(), textBlock, collection, out _);
            }
        }

        public static void SetText(UIElement element, string value)
        {
            element.SetValue(TextProperty, value);
        }

        public static string GetText(UIElement element)
        {
            return (string)element.GetValue(TextProperty);
        }

        #endregion

        #region BBCode Parser

        private class Parameter
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// This method allows for the creation of Spans for BBCode tags which act as containers for other
        /// Inlines.
        ///
        /// Extend this method to add new container BBCode tags, creating a Span (or Span derived type)
        /// for each.
        ///
        /// If a Span has been created, return a pointer to that Span's InlineCollection. If no Span has
        /// been created, return nullptr.
        /// </summary>
        static InlineCollection TryCreateSpanForTag(string tagName,
            List<Parameter> parameters, TextBlock parent, InlineCollection inlineCollection)
        {
            switch (tagName)
            {
                case "b":
                    {
                        Bold bold = new Bold();
                        inlineCollection.Add(bold);
                        return bold.Inlines;
                    }
                case "i":
                    {
                        Italic italic = new Italic();
                        inlineCollection.Add(italic);
                        return italic.Inlines;
                    }
                case "u":
                    {
                        Underline underline = new Underline();
                        inlineCollection.Add(underline);
                        return underline.Inlines;
                    }
                case "url":
                    {
                        foreach (Parameter element in parameters)
                        {
                            if (element.Key == "url")
                            {
                                // Only create Hyperlink as a container if the url is specified as a parameter
                                Hyperlink hyperlink = new Hyperlink();
                                hyperlink.NavigateUri = new Uri(element.Value);
                                inlineCollection.Add(hyperlink);
                                return hyperlink.Inlines;
                            }
                        }

                        break;
                    }
                case "style":
                    {
                        Span span = new Span();
                        foreach (Parameter element in parameters)
                        {
                            if (element.Key == "style")
                            {
                                try
                                {
                                    Style style = (Style)parent.FindResource(element.Value);
                                    if (style != null)
                                    {
                                        span.Style = style;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[NOESIS/E] BBCode style tag parameter value '{element.Value}' not found.");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"[NOESIS/E] BBCode style tag error. {e.Message}");
                                }
                            }
                        }
                        inlineCollection.Add(span);
                        return span.Inlines;
                    }
                case "font":
                case "size":
                case "color":
                    {
                        Span span = new Span();
                        foreach (Parameter element in parameters)
                        {
                            if (element.Key == "size")
                            {
                                if (float.TryParse(element.Value, out float fontSize))
                                {
                                    span.FontSize = fontSize;
                                }
                                else
                                {
                                    Console.WriteLine("NOESIS: BBCode tag '%s' size property value '%s' is not a valid float");
                                }
                            }
                            else if (element.Key == "font")
                            {
                                FontFamily fontFamily = new FontFamily(element.Value);
                                span.FontFamily = fontFamily;
                            }
                            else if (element.Key == "color")
                            {
                                BrushConverter brushConverter = new BrushConverter();
                                SolidColorBrush brush = (SolidColorBrush)brushConverter.ConvertFromString(element.Value);
                                if (brush != null)
                                {
                                    span.Foreground = brush;
                                }
                                else
                                {
                                    Console.WriteLine("NOESIS: BBCode tag '%s' color property value '%s' is not a valid color string");
                                }
                            }
                        }
                        inlineCollection.Add(span);
                        return span.Inlines;
                    }
            }

            return null;
        }

        /// <summary>
        /// This method allows for the creation of Inlines for BBCode tags which do not act as containers
        /// for other Inlines.
        /// 
        /// Extend this method to add new non-container BBCode tags, creating an Inline derived type for
        /// each.
        /// </summary>
        static void TryCreateInlineForTag(string tagName, string content,
            List<Parameter> parameters, InlineCollection inlineCollection)
        {
            switch (tagName)
            {
                case "br":
                    {
                        LineBreak lineBreak = new LineBreak();
                        inlineCollection.Add(lineBreak);

                        return;
                    }
                case "url":
                    {
                        // Only create Hyperlink as a container if the url is specified as a parameter
                        Hyperlink hyperlink = new Hyperlink(new Run(content));
                        hyperlink.NavigateUri = new Uri(content);
                        inlineCollection.Add(hyperlink);

                        return;
                    }
                case "bind":
                    {
                        Run run = new Run();
                        Binding binding = new Binding(content);

                        foreach (Parameter element in parameters)
                        {
                            if (element.Key == "format")
                            {
                                binding.StringFormat = element.Value;
                            }
                        }

                        run.SetBinding(Run.TextProperty, binding);
                        inlineCollection.Add(run);

                        return;
                    }
                case "img":
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(content));
                        inlineCollection.Add(new InlineUIContainer(image));
                        foreach (Parameter element in parameters)
                        {
                            if (element.Key == "width")
                            {
                                if (float.TryParse(element.Value, out float width))
                                {
                                    image.Width = width;
                                }
                                else
                                {
                                    Console.WriteLine("NOESIS: Invalid value for BBCode img tag width parameter");
                                }
                            }
                            else if (element.Key == "height")
                            {
                                if (float.TryParse(element.Value, out float height))
                                {
                                    image.Height = height;
                                }
                                else
                                {
                                    Console.WriteLine("NOESIS: Invalid value for BBCode img tag height parameter");
                                }
                            }
                        }
                        if (parameters.Count > 1)
                        {
                            image.Stretch = Stretch.Fill;
                        }
                        return;
                    }
            }

            Console.WriteLine("NOESIS: BBCode tag '%s' is not currently supported", tagName);
        }

        private static void TryParse(ReadOnlySpan<char> input, TextBlock parent, InlineCollection inlineCollection, out ReadOnlySpan<char> output)
        {
            if (input.Length == 0)
            {
                output = input;
                return;
            }

            while (input.Length > 0)
            {
                if (input[0] == '[')
                {
                    if (input.Length > 1 && input[1] == '/')
                    {
                        output = input.Slice(1);
                        return;
                    }
                    ParseTag(input, parent, inlineCollection, out input);
                }
                else
                {
                    ParseText(input, inlineCollection, out input);
                }
            }

            output = input;
        }

        private static void ParseText(in ReadOnlySpan<char> input, InlineCollection inlineCollection, out ReadOnlySpan<char> output)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int begin = 0;
            int current = 0;
            for (; current < input.Length; current++)
            {
                if (input[current] == '\\')
                {
                    if (current - 1 > begin)
                    {
                        stringBuilder.Append(input.Slice(begin, current - begin).ToString());
                    }
                    ++current;
                    begin = current;
                }
                else if (input[current] == ']')
                {
                    Console.WriteLine("NOESIS: BBCode contains a malformed closing bracket");
                    output = ReadOnlySpan<char>.Empty;
                    return;
                }
                else if (input[current] == '[')
                {
                    break;
                }
            }

            if (current != begin)
            {
                stringBuilder.Append(input.Slice(begin, current - begin).ToString());
            }

            inlineCollection.Add(new Run(stringBuilder.ToString()));

            output = input.Slice(current);
        }

        private static bool TryParseContent(in ReadOnlySpan<char> input, out string content, out ReadOnlySpan<char> output)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int begin = 0;
            int current = 0;

            bool useQuotationMarks = false;
            bool singleQuotes = false;
            if (input[current] == '\'' || input[current] == '"')
            {
                useQuotationMarks = true;
                singleQuotes = input[current] == '\'';
                ++begin;
                ++current;
            }

            for (; current < input.Length - 1; current++)
            {
                if (input[current] == '\\')
                {
                    stringBuilder.Append(input.Slice(begin, current - begin).ToString());
                    ++current;
                    begin = current;
                }
                else if (useQuotationMarks)
                {
                    if (singleQuotes && input[current] == '\'' || !singleQuotes && input[current] == '"')
                    {
                        if (current - 1 > begin)
                        {
                            stringBuilder.Append(input.Slice(begin, current - begin).ToString());
                        }
                        content = stringBuilder.ToString();
                        output = input.Slice(current + 1);
                        return true;
                    }
                    if (input[current] == '[' || input[current] == ']')
                    {
                        Console.WriteLine("NOESIS: BBCode parameter value is missing a closing quotation mark");
                        content = string.Empty;
                        output = ReadOnlySpan<char>.Empty;
                    }
                }
                else if (input[current] == '[' || input[current] == ']')
                {
                    break;
                }
            }

            if (current != begin)
            {
                stringBuilder.Append(input.Slice(begin, current - begin).ToString());
            }

            content = stringBuilder.ToString();
            output = input.Slice(current);
            return true;
        }

        private static void ParseName(in ReadOnlySpan<char> input, out string name, out ReadOnlySpan<char> output)
        {
            int current = 0;
            for (; current < input.Length; current++)
            {
                if (!char.IsLetter(input[current]))
                {
                    name = input.Slice(0, current).ToString().ToLowerInvariant();
                    output = input.Slice(current);
                    return;
                }
            }

            name = String.Empty;
            output = ReadOnlySpan<char>.Empty; ;
        }

        private static void ParseKeyValuePair(ReadOnlySpan<char> input, List<Parameter> parameters, out ReadOnlySpan<char> output)
        {
            int current = 0;
            while (char.IsWhiteSpace(input[current]) && current < input.Length - 1)
            {
                ++current;
            }

            if (current == input.Length - 1 || !char.IsLetter(input[current]))
            {
                output = input.Slice(current);
                return;
            }

            input = input.Slice(current);
            ParseName(input, out string key, out input);

            if (key == null)
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key is malformed");
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            if (input.Length == 0 || input[0] != '=')
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key '{0}' is malformed (missing '=')", key);
                output = ReadOnlySpan<char>.Empty;
            }

            if (!TryParseContent(input.Slice(1), out string value, out output))
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key '{0}' is malformed", key);
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            parameters.Add(new Parameter
            {
                Key = key,
                Value = value
            });
        }

        private static void ParseTag(ReadOnlySpan<char> input, TextBlock parent, InlineCollection inlineCollection, out ReadOnlySpan<char> output)
        {
            ParseName(input.Slice(1), out string tagName, out input);

            if (string.IsNullOrEmpty(tagName))
            {
                Console.WriteLine("NOESIS: A BBCode tag is malformed (contains no name)");
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            List<Parameter> parameters = new List<Parameter>();

            if (input[0] == '=')
            {
                if (!TryParseContent(input.Slice(1), out string value, out input))
                {
                    output = ReadOnlySpan<char>.Empty;
                    return;
                }

                parameters.Add(new Parameter() { Key = tagName, Value = value });
            }
            else
            {
                while (input[0] == ' ')
                {
                    ParseKeyValuePair(input, parameters, out input);
                }
            }

            if (input.Length > 1 && input[0] == '/' && input[1] == ']')
            {
                TryCreateInlineForTag(tagName, "", parameters, inlineCollection);
                if (input.Length == 2)
                {
                    output = ReadOnlySpan<char>.Empty;
                }
                else
                {
                    output = input.Slice(2);
                }
                return;
            }

            if (input[0] != ']' || input.Length < 5) // 5 allows for the necessary brackets and forward slash for a minimal closing bracket
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' has a malformed closing tag", tagName);
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            input = input.Slice(1);

            InlineCollection newCollection = TryCreateSpanForTag(tagName, parameters,
                parent, inlineCollection);

            if (newCollection != null)
            {
                inlineCollection = newCollection;
                TryParse(input, parent, inlineCollection, out input);
            }
            else
            {
                if (!TryParseContent(input, out string value, out input))
                {
                    output = ReadOnlySpan<char>.Empty;
                    return;
                }

                input = input.Slice(1);

                TryCreateInlineForTag(tagName, value, parameters,
                    inlineCollection);
            }

            if (input.Length < 2)
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' is missing a closing tag", tagName);
                output = input;
                return;
            }

            if (input[0] != '/')
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' is missing a closing tag", tagName);
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            ParseName(input.Slice(1), out string closingTagName, out input);

            if (closingTagName != tagName)
            {
                Console.WriteLine("NOESIS: BBCode tag opening name '{0}' does not match closing tag name '{0}'", tagName, closingTagName);
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            if (input[0] != ']')
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' has a malformed closing tag", tagName);
                output = ReadOnlySpan<char>.Empty;
                return;
            }

            if (input.Length == 1)
            {
                output = ReadOnlySpan<char>.Empty;
            }
            else
            {
                output = input.Slice(1);
            }
        }

        #endregion
    }
}
