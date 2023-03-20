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
    /// Usage:
	///
	///    <Grid
	///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
	///      <TextBlock noesis:RichText.Text="Plain. [b]Bold, [i]bold italic, [/i]
	///        [size=60]Size 60 text.[/size] [color=Red]Red text.[/color] [img height=80]disk.png[/img]
	///        [br/] [url='https://www.noesisengine.com/']NoesisEngine.com[/url]" />
	///    </Grid>
	///
	///
	/// Default supported BBCode tags, with their Inline output:
	///
	/// * *b*: Bold,
	///   `"[b]bold.[/b]"`
	///
	///
	/// * *i*: Italic,
	///   `"[i]italic.[/i]"`
	///
	///
	/// * *u*: Underline,
	///   `"[u]underline.[/u]"`
	///
	///
	/// * *size*: Span with FontSize set to the parameter value,
	///   `"[size=60]size 60 text.[/size]"`
	///
	///
	/// * *font*: Span with FontFamily set to the parameter value,
	///   `"[font='#PT Root UI']PT Root UI font.[/]"`
	///
	///
	/// * *color*: Span with Foreground set to the parameter value (a color name, or ARBG hex),
	///   `"[color=Red]red.[/color][color=#FF0000FF]blue.[/color]"`
	///
	///
	/// * *url*: Hyperlink with NavigateUri set to the parameter value,
	///   `"[url='https://www.noesisengine.com/']NoesisEngine.com[/url]"`
	///
	///
	/// * *br*: A LineBreak,
	///   `"Line one.[br/]Line two."`
	///
	///
	/// * *img*: Image contained in an InlineUIContainer,
	///   `"[img height=80]disk.png[/img]"`
	///
	///
	/// * *style*: Span with the Style property set to the resource key provided by the parameter value,
	///   `"[style='Header1']Styled text.[/style]"`
	///
	///
	/// Usage:
	///
	///      <Grid
	///        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	///        xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
	///        <Grid.Resources>
	///          <Style x:Key="Header1" TargetType="{x:Type Span}">
	///            <Setter Property="FontSize" Value="30"/>
	///            <Setter Property="Foreground" Value="Green"/>
	///          </Style>
	///        </Grid.Resources>
	///        <TextBlock noesis:RichText.Text="[style='Header1']Styled text.[/style]" />
	///      </Grid>
	///
	///
	/// * *bind*: Run containing a Binding with the Path property set to the tag contents. This tag
	///   has an optional "format" parameter which can be used to modify the StringFormat property
	///   of the Binding,
	///   `"[bind format='{0:0}']Path[/bind]"`
	///
	///
	/// Usage:
	///
	///      <Grid
	///        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	///        xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions"
	///        xmlns:local="clr-namespace:MyGame">
	///        <Grid.DataContext>
	///          <local:MyViewModel CurrentHealth="66.75" MaxHealth="100" />
	///        </Grid.DataContext>
	///        <TextBlock noesis:RichText.Text="Health is [bind format='{0:0}']CurrentHealth[/bind] out of
	///          [bind format='{0:0}']MaxHealth[/bind]" />
	///      </Grid>
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
                
                TryParse((string)args.NewValue, 0, textBlock, collection, out _);
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

        private static void TryParse(string input, int current, TextBlock parent, InlineCollection inlineCollection, out int output)
        {
            if (input.Length - current == 0)
            {
                output = current;
                return;
            }

            while (input.Length - current > 0)
            {
                if (input[current] == '[')
                {
                    if (input.Length - current > 1 && input[current + 1] == '/')
                    {
                        output = current + 1;
                        return;
                    }
                    ParseTag(input, current, parent, inlineCollection, out current);
                }
                else
                {
                    ParseText(input, current, inlineCollection, out current);
                }
            }

            output = current;
        }

        private static void ParseText(string input, int begin, InlineCollection inlineCollection, out int output)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            int current = begin;
            for (; current < input.Length; current++)
            {
                if (input[current] == '\\')
                {
                    if (current - 1 > begin)
                    {
                        stringBuilder.Append(input.Substring(begin, current - begin));
                    }
                    ++current;
                    begin = current;
                }
                else if (input[current] == ']')
                {
                    Console.WriteLine("NOESIS: BBCode contains a malformed closing bracket");
                    output = input.Length;
                    return;
                }
                else if (input[current] == '[')
                {
                    break;
                }
            }

            if (current != begin)
            {
                stringBuilder.Append(input.Substring(begin, current - begin));
            }

            inlineCollection.Add(new Run(stringBuilder.ToString()));

            output = current;
        }

        private static bool TryParseContent(string input, int begin, out string content, out int output)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            int current = begin;

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
                    stringBuilder.Append(input.Substring(begin, current - begin));
                    ++current;
                    begin = current;
                }
                else if (useQuotationMarks)
                {
                    if (singleQuotes && input[current] == '\'' || !singleQuotes && input[current] == '"')
                    {
                        if (current - 1 > begin)
                        {
                            stringBuilder.Append(input.Substring(begin, current - begin));
                        }
                        content = stringBuilder.ToString();
                        output = current + 1;
                        return true;
                    }
                    if (input[current] == '[' || input[current] == ']')
                    {
                        Console.WriteLine("NOESIS: BBCode parameter value is missing a closing quotation mark");
                        content = string.Empty;
                        output = input.Length;
                        return false;
                    }
                }
                else if (input[current] == '[' || input[current] == ']')
                {
                    break;
                }
            }

            if (current != begin)
            {
                stringBuilder.Append(input.Substring(begin, current - begin));
            }

            content = stringBuilder.ToString();
            output = current;
            return true;
        }

        private static void ParseName(string input, int begin, out string name, out int output)
        {
            for (int current = begin; current < input.Length; current++)
            {
                if (!char.IsLetter(input[current]))
                {
                    name = input.Substring(begin, current - begin).ToLowerInvariant();
                    output = current;
                    return;
                }
            }

            name = string.Empty;
            output = input.Length;
        }

        private static void ParseKeyValuePair(string input, int current, List<Parameter> parameters, out int output)
        {
            while (char.IsWhiteSpace(input[current]) && current < input.Length - 1)
            {
                ++current;
            }

            if (current == input.Length - 1 || !char.IsLetter(input[current]))
            {
                output = current;
                return;
            }

            ParseName(input, current, out string key, out current);

            if (key == null)
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key is malformed");
                output = input.Length;
                return;
            }

            if (input.Length - current == 0 || input[current] != '=')
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key '{0}' is malformed (missing '=')", key);
                output = input.Length;
                return;
            }

            if (!TryParseContent(input, current + 1, out string value, out output))
            {
                Console.WriteLine("NOESIS: A parameter for BBCode tag key '{0}' is malformed", key);
                output = input.Length;
                return;
            }

            parameters.Add(new Parameter
            {
                Key = key,
                Value = value
            });
        }

        private static void ParseTag(string input, int current, TextBlock parent, InlineCollection inlineCollection, out int output)
        {
            int begin = current;
            ParseName(input, current + 1, out string tagName, out current);

            if (string.IsNullOrEmpty(tagName))
            {
                Console.WriteLine("NOESIS: A BBCode tag is malformed (contains no name)");
                output = input.Length;
                return;
            }

            List<Parameter> parameters = new List<Parameter>();

            if (input[current] == '=')
            {
                if (!TryParseContent(input, current + 1, out string value, out current))
                {
                    output = input.Length;
                    return;
                }

                parameters.Add(new Parameter() { Key = tagName, Value = value });
            }
            else
            {
                while (input[current] == ' ')
                {
                    ParseKeyValuePair(input, current, parameters, out current);
                    if (current == input.Length)
                    {
                        output = input.Length;
                        return;
                    }
                }
            }

            if (input.Length - current > 1 && input[current] == '/' && input[current + 1] == ']')
            {
                TryCreateInlineForTag(tagName, "", parameters, inlineCollection);
                if (input.Length == 2)
                {
                    output = input.Length;
                }
                else
                {
                    output = current + 2;
                }
                return;
            }

            if (input[current] != ']' || input.Length - current < 5) // 5 allows for the necessary brackets and forward slash for a minimal closing bracket
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' has a malformed closing tag", tagName);
                output = input.Length;
                return;
            }

            current += 1;

            InlineCollection newCollection = TryCreateSpanForTag(tagName, parameters,
                parent, inlineCollection);

            if (newCollection != null)
            {
                inlineCollection = newCollection;
                TryParse(input, current, parent, inlineCollection, out current);
            }
            else
            {
                if (!TryParseContent(input, current, out string value, out current))
                {
                    output = input.Length;
                    return;
                }

                current += 1;

                TryCreateInlineForTag(tagName, value, parameters,
                    inlineCollection);
            }

            if (input.Length - current < 2)
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' is missing a closing tag", tagName);
                output = current;
                return;
            }

            if (input[current] != '/')
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' is missing a closing tag", tagName);
                output = input.Length;
                return;
            }

            ParseName(input, current + 1, out string closingTagName, out current);

            if (closingTagName != tagName)
            {
                Console.WriteLine("NOESIS: BBCode tag opening name '{0}' does not match closing tag name '{0}'", tagName, closingTagName);
                output = input.Length;
                return;
            }

            if (input[current] != ']')
            {
                Console.WriteLine("NOESIS: BBCode tag '{0}' has a malformed closing tag", tagName);
                output = input.Length;
                return;
            }

            if (input.Length - current == 1)
            {
                output = input.Length;
            }
            else
            {
                output = current + 1;
            }
        }

#endregion
    }
}
