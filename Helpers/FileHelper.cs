using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenScribe.Helpers
{
    public class FileHelper
    {
        public FileHelper() { }
        public Tuple<string,string> OpenFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text file|*.txt";
            openFileDialog1.Title = "Open";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)openFileDialog1.OpenFile();
                string fileContents;
                using (StreamReader reader = new StreamReader(fs))
                {
                    fileContents = reader.ReadToEnd();
                }
                return new Tuple<string, string>(openFileDialog1.FileName, fileContents);
            }
            return new Tuple<string, string>("", "");
        }

        public void SaveFile(string currentFile, RichTextBox MainText)
        {
            MainText.SaveFile(currentFile, RichTextBoxStreamType.PlainText);
            //MessageBox.Show("File Saved.","", MessageBoxButtons.OK,MessageBoxIcon.Information);
            
        }

        public string SaveNewFile(string currentFile, RichTextBox MainText)
        {
            string retval = "";
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text file|*.txt";
            saveFileDialog1.Title = "Save";
            saveFileDialog1.FileName = currentFile;
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                MainText.SaveFile(fs, RichTextBoxStreamType.PlainText);

                fs.Close();
                retval = saveFileDialog1.FileName;
            }
            return retval;
        }
    }

    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}
