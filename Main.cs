using Microsoft.VisualBasic.Devices;
using NAudio.Vorbis;
using NAudio.Wave;
using System.IO;
using System.Windows.Forms;
using ZenScribe.Helpers;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TextEdit
{
    public partial class Main : Form
    {
        // Variables - Constants
        const int updateInterval = 1000;
        const int autosaveInterval = 300000;
        const int colorCombinations = 3;
        List<string> commandList = new List<string>();
        public List<Color> backgroundColors = new List<Color>();
        public List<Color> foregroundColors = new List<Color>();
        public List<string> fonts = new List<string>();
        int currentColorSchemeIndex = 0;
        int currentFontIndex = 0;
        int currentFontSize = 18;
        public string currentFontName;
        public string currentFile = "";
        public int wordCount = 0;
        public int charactersCount = 0;
        public int charactersCountWithOutSpaces = 0;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer autoSaveTimer = new System.Windows.Forms.Timer();
        KeyboardHelper kh;
        FileHelper fh;

        private WaveOutEvent outputDevice;
        private VorbisWaveReader vorbisReader;
        int currentTrack = 0;   

        public Main()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CustomInitialize();
        }
        private void MainText_TextChanged(object sender, EventArgs e)
        {
            if (MainText.Text.Length > 3) {
                if (MainText.Text.Substring(MainText.TextLength - 3, 3) == "___")
                {
                    kh.AddSeparator();
                }
            }
            UpdateCounters();
        }
        private void MainText_KeyDown(object sender, KeyEventArgs e)
        {
            // ENTER
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                if (!kh.AddTitle() && !kh.CheckTab()) {
                    MainText.Text += "\n";
                    MainText.SelectionStart = MainText.Text.Length;
                    MainText.SelectionLength = 0;
                }
            }

            // HELP (F1)
            if (e.KeyCode == Keys.F1)
            {
                string text = string.Join(Environment.NewLine, commandList);
                MessageBox.Show(text, "Help",MessageBoxButtons.OK,MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,MessageBoxOptions.RightAlign);
            }

            // TAB 
            if (e.KeyCode == Keys.Tab){
                e.SuppressKeyPress = true;
                kh.AddTab();
            }

            // FONT
            if (e.Alt && e.KeyCode == Keys.F) {
                currentFontIndex++;
                if (currentFontIndex > fonts.Count - 1) currentFontIndex = 0;
                UpdateFont();
                UpdateCounters();
            }

            // COLOR
            if (e.Alt && e.KeyCode == Keys.C) { 
                this.TransparencyKey = Color.Empty;
                currentColorSchemeIndex++;
                if (currentColorSchemeIndex > backgroundColors.Count - 1) currentColorSchemeIndex = 0;
                UpdateColors();
            }

            // TRANSPARENT
            if (e.Alt && e.KeyCode == Keys.T) {
                if (this.TransparencyKey == Color.LimeGreen) {
                    this.TransparencyKey = Color.Empty;
                    currentColorSchemeIndex = 0;
                    UpdateColors();
                }
                else {
                    this.TransparencyKey = Color.LimeGreen;
                    MainText.BackColor = Color.LimeGreen;
                    MainText.ForeColor = Color.Black;
                }
            }

            // MUSIC
            if (e.Control && e.KeyCode == Keys.M)
            {
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                path += "\\Data\\Music";
                currentTrack++;
                string[] tracklist = Directory.GetFiles(path);
                int totalTracks = tracklist.Length;
                if (currentTrack > totalTracks) currentTrack = 0;

                if (outputDevice != null)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                    outputDevice = null;
                }

                if (vorbisReader != null)
                {
                    vorbisReader.Dispose();
                    vorbisReader = null;
                }

                if (currentTrack > 0)
                {
                    try
                    {
                        string nextSong = tracklist[currentTrack - 1];
                        outputDevice = new WaveOutEvent();
                        vorbisReader = new VorbisWaveReader(nextSong);
                        outputDevice.Init(vorbisReader);
                        outputDevice.Play();
                        Notification.Text = "Now playing: " + nextSong.Split('\\').Last();
                        Notification.Refresh();
                        System.Threading.Thread.Sleep(3000);
                        Notification.Text = "";
                        Notification.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error trying to play music: " + ex.Message);
                    }
                }
                else {
                    Notification.Text = "Music Off";
                    Notification.Refresh();
                    System.Threading.Thread.Sleep(3000);
                    Notification.Text = "";
                    Notification.Refresh();
                }
            }

            // FONT UP
            if (e.Alt && e.KeyCode == Keys.Oemplus){
                currentFontSize++;
                if (currentFontSize > 64) currentFontSize = 64;
                UpdateFont();
                UpdateCounters();
            }

            // FONT DOWN
            if (e.Alt && e.KeyCode == Keys.OemMinus){
                currentFontSize--;
                if(currentFontSize < 8) currentFontSize = 8;
                UpdateFont();
                UpdateCounters();
            }

            // SAVE
            if (e.Control && e.KeyCode == Keys.S){
                if (!string.IsNullOrEmpty(currentFile))
                {
                    fh.SaveFile(currentFile, MainText);
                    ShowSavedFile();
                }
                else {
                    currentFile = fh.SaveNewFile(currentFile, MainText);
                }
            }

            // NEW FILE
            if (e.Control && e.KeyCode == Keys.N)
            {
                if (!string.IsNullOrEmpty(currentFile))
                {
                    fh.SaveFile(currentFile, MainText);
                    ShowSavedFile();
                    currentFile = "";
                    MainText.Text = "";
                }
            }

            // SEARCH
            if (e.Control && e.KeyCode == Keys.F){
                Search s = new Search();
                s.ShowDialog();
                if (!string.IsNullOrEmpty(s.ReturnValue)) {
                    MainText.Find(s.ReturnValue);
                }
            }

            // OPEN FILE
            if (e.Control && e.KeyCode == Keys.O) {
                Tuple<string,string> openFileResult = fh.OpenFile();
                MainText.Text = openFileResult.Item2;
                MainText.SelectionStart = MainText.Text.Length;
                MainText.SelectionLength = 0;
                currentFile = openFileResult.Item1;
                UpdateCounters();
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }
        private void autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFile))
            {
                fh.SaveFile(currentFile, MainText);
                ShowSavedFile();
            }
        }
        private void ShowSavedFile() {
            Notification.Text = "File saved!";
            Notification.Refresh();
            System.Threading.Thread.Sleep(1000);
            Notification.Text = "";
            Notification.Refresh();
        }
        private void UpdateFont() {
            System.Drawing.Font currentFont = new System.Drawing.Font(fonts[currentFontIndex], currentFontSize);
            MainText.Font = currentFont;
            currentFontName = currentFont.Name;
            UpdateCounters();
        }
        private void UpdateColors() {
            MainText.BackColor = backgroundColors[currentColorSchemeIndex];
            MainText.ForeColor = foregroundColors[currentColorSchemeIndex];
        }
        private void CustomInitialize() {
            kh = new KeyboardHelper(MainText);
            fh = new FileHelper();
            System.Drawing.Font currentFont = new System.Drawing.Font("Courier New", 18);
            MainText.Font = currentFont;    
            currentFontName = currentFont.Name;       
            MainText.KeyDown += new KeyEventHandler(MainText_KeyDown);
            MainText.AcceptsTab = true;
            fonts = GenericHelper.LoadFonts();
            commandList = GenericHelper.LoadCommandList();
            GenericHelper.LoadColorSchemes(backgroundColors, foregroundColors);
            StyleInitializer();
            UpdateClock();
            UpdateCounters();
            UpdateColors();
            TimerHandler();
        }
        private void StyleInitializer() {
            MainText.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            Counter.Anchor = Notification.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            Clock.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
        private void TimerHandler() {
            timer.Tick += new EventHandler(timer_Tick);
            autoSaveTimer.Tick += new EventHandler(autoSaveTimer_Tick);
            Clock.TextAlign = ContentAlignment.MiddleRight;
            timer.Interval = updateInterval;
            autoSaveTimer.Interval = autosaveInterval;
            timer.Start();
            autoSaveTimer.Start();
        }
        private void UpdateCounters() {
            charactersCount = MainText.Text.Replace("#","").Replace("-","").Length;
            charactersCountWithOutSpaces = MainText.Text.Replace("#", "").Replace("-", "").Replace(" ", "").Length;
            wordCount = GenericHelper.CountWords(MainText.Text);
            Counter.Text = 
                $"WORDS: {wordCount} | CHARACTERS: {charactersCount} | WITHOUT SPACES: {charactersCountWithOutSpaces} | " +
                $"FONT: {currentFontName} at {currentFontSize}pt";
        }
        private void UpdateClock() { 
            Clock.Text = DateTime.Now.DayOfWeek + " " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
        }
    }
}
