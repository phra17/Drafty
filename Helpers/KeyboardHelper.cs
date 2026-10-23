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

        public bool CheckTab() {
            int currentLineIndex = MainText.GetLineFromCharIndex(MainText.SelectionStart);
            string lastLine = MainText.Lines[currentLineIndex];
            if (lastLine.StartsWith("\t")) {
                int selectionStart = MainText.SelectionStart;
                MainText.SelectedText = Environment.NewLine + "\t";
                MainText.SelectionStart = selectionStart + Environment.NewLine.Length + 1;
                MainText.SelectionLength = 0;
                return true;
            }
            return false;
        }

        public void AddTab()
        {
            int selectionStart = MainText.SelectionStart;
            MainText.SelectedText = "\t";
            MainText.SelectionStart = selectionStart + 1;
            MainText.SelectionLength = 0;
        }
    }
}
