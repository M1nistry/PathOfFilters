using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PathOfFilters
{
    internal class ColorizeRgb : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;
            var regex = Regex.Match(text, @"\d+\d{0,3}\s+\d+\d{0,3}\s\d+\d{0,3}");
            if (!regex.Success) return;
            while ((index = text.IndexOf(regex.Groups[0].Value, start, StringComparison.Ordinal)) >= 0)
            {
                var fontBrush = StringToRgbBrush(regex.Groups[0].Value);
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + regex.Groups[0].Length, // endOffset
                    (VisualLineElement element) =>
                    {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        Typeface tf = element.TextRunProperties.Typeface;
                        // Replace the typeface with a modified version of
                        // the same typeface
                        element.TextRunProperties.SetTypeface(new Typeface(
                            tf.FontFamily,
                            FontStyles.Normal,
                            FontWeights.Bold,
                            tf.Stretch
                            ));
                        element.TextRunProperties.SetForegroundBrush(fontBrush);
                    });
                start = index + 1; // search for next occurrence
            }
        }

        private Brush StringToRgbBrush(string values)
        {
            int value;
            var splitString = values.Split(' ');
            var splitInts = splitString.Select(item => int.TryParse(item, out value) ? value : -1).ToArray();
            
            if (splitInts.Any(rgbValue => rgbValue == -1 || rgbValue > 255))
            {
                return Brushes.Black;
            }
            //if (splitInts[0] > 255 || splitInts[1] > 255 || splitInts[2] > 255) return Brushes.Black;
            var color = Color.FromRgb((byte)splitInts[0], (byte)splitInts[1], (byte)splitInts[2]);

            return new SolidColorBrush(color);
        }
    }
}
