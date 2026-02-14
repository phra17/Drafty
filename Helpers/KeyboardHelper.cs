using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenScribe.Helpers
{
    public class KeyboardHelper
    {
        public RichTextBox MainText { get; set; }
        public KeyboardHelper(RichTextBox mainText) { 
            MainText = mainText;
        }

        public bool AddTitle()
        {
            int currentLineIndex = MainText.GetLineFromCharIndex(MainText.SelectionStart);
            string lastLine = MainText.Lines[currentLineIndex];

            if (lastLine.StartsWith("#"))
            {
                MainText.Text = MainText.Text.Replace(lastLine, lastLine.Replace("#", "###") + "###");
                MainText.Text += "\n";
                MainText.SelectionStart = MainText.Text.Length;
                MainText.SelectionLength = 0;
                return true;
            }
            return false;
        }

        public bool CheckTab() {
            int currentLineIndex = MainText.GetLineFromCharIndex(MainText.SelectionStart);
            string lastLine = MainText.Lines[currentLineIndex];
            if (lastLine.StartsWith("\t")) {
                MainText.Text += "\n";
                MainText.Text += "\t";
                MainText.SelectionStart = MainText.Text.Length;
                MainText.SelectionLength = 0;
                return true;
            }
            return false;
        }

        public void AddTab()
        {
            int selectionStart = MainText.SelectionStart;
            MainText.Text = MainText.Text.Insert(selectionStart, "\t");
            MainText.SelectionStart = selectionStart + 1;
        }

        public void AddSeparator()
        {
            try
            {
                int textWidth = GenericHelper.CalculateMaxDescriptionLineLength(MainText);
                string separator = "-";
                for (int i = 0; i <= textWidth; i++)
                {
                    separator += "-";
                }
                MainText.Text = MainText.Text.Replace("___", separator + Environment.NewLine);
                MainText.SelectionStart = MainText.Text.Length;
                MainText.SelectionLength = 0;
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
