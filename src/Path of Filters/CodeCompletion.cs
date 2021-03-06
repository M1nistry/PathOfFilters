﻿using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace PathOfFilters
{
    internal class CodeCompletion : ICompletionData
    {

        public CodeCompletion(string text)
        {
            Text = text;
        }

        public System.Windows.Media.ImageSource Image 
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content 
        {
            get { return this.Text; }
        }

        public object Description 
        {
            get { return "Description for " + this.Text; }
        }

        public double Priority { get; set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
