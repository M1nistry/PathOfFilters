using System;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;

namespace PathOfFilters
{
    public class CustomIndentationStrategy : IIndentationStrategy
    {
        /// <inheritdoc/>
        public void IndentLine(TextDocument document, DocumentLine line)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (line == null)
                throw new ArgumentNullException("line");
            var previousLine = line.PreviousLine;
            if (previousLine == null) return;

            var indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
            var previousLineText = document.GetText(previousLine.Offset, document.GetLineByOffset(previousLine.Offset).Length);
            //Indent if previous line is show/hide
            if (previousLineText == "Show" || previousLineText == "Hide")
            {
                indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
                document.Replace(indentationSegment, "    ");
                return;
            }
            //If the previous line isn't null or whitespace, keep indenting the same amount
            if (!String.IsNullOrWhiteSpace(previousLineText))
            {
                indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
                var indentation = document.GetText(indentationSegment);
                indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
                document.Replace(indentationSegment, indentation);
            }
            else //if line is empty/whitespace, remove indent
            {
                document.Replace(indentationSegment, String.Empty);
            }
        }

        public void IndentLines(TextDocument document, int beginLine, int endLine)
        {
        }
    }
}
