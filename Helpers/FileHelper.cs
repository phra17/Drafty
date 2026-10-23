namespace ZenScribe.Helpers
{
    public sealed record OpenedFile(string Path, string Contents, string? BackupPath);

    public class FileHelper
    {
        private static readonly string backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Drafty",
            "Backups");

        public OpenedFile? OpenFile(IWin32Window owner)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text file|*.txt",
                Title = "Open"
            };

            if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }

            string? createdBackupPath = null;
            try
            {
                createdBackupPath = CreateBackup(openFileDialog.FileName);
            }
            catch (Exception ex) when (IsFileOperationError(ex))
            {
                MessageBox.Show(
                    owner,
                    $"Non è stato possibile creare il backup. Il file verrà comunque aperto.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Backup non creato",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            string fileContents = File.ReadAllText(openFileDialog.FileName);
            return new OpenedFile(openFileDialog.FileName, fileContents, createdBackupPath);
        }

        public void SaveFile(string filePath, RichTextBox mainText)
        {
            string fullPath = Path.GetFullPath(filePath);
            string directory = Path.GetDirectoryName(fullPath)
                ?? throw new ArgumentException("The file path must include a directory.", nameof(filePath));
            string temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

            try
            {
                mainText.SaveFile(temporaryPath, RichTextBoxStreamType.PlainText);
                FlushFileToDisk(temporaryPath);

                if (File.Exists(fullPath))
                {
                    File.Replace(temporaryPath, fullPath, null);
                }
                else
                {
                    File.Move(temporaryPath, fullPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public string SaveNewFile(RichTextBox mainText)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file|*.txt",
                Title = "Save"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFile(saveFileDialog.FileName, mainText);
                return saveFileDialog.FileName;
            }

            return "";
        }

        private static string CreateBackup(string sourcePath)
        {
            Directory.CreateDirectory(backupDirectory);
            string backupFileName =
                $"{Path.GetFileName(sourcePath)}.{DateTime.Now:yyyyMMdd-HHmmss-fff}.{Guid.NewGuid():N}.bak";
            string destinationPath = Path.Combine(backupDirectory, backupFileName);
            string temporaryPath = destinationPath + ".tmp";
            try
            {
                File.Copy(sourcePath, temporaryPath);
                FlushFileToDisk(temporaryPath);
                File.Move(temporaryPath, destinationPath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }

            return destinationPath;
        }

        private static void FlushFileToDisk(string filePath)
        {
            using FileStream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            stream.Flush(true);
        }

        private static bool IsFileOperationError(Exception exception)
        {
            return exception is IOException
                or UnauthorizedAccessException
                or ArgumentException
                or NotSupportedException
                or System.Security.SecurityException;
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
