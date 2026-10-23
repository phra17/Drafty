using Microsoft.VisualBasic.Devices;
using NAudio.Vorbis;
using NAudio.Wave;
using System.Globalization;
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
        const int notificationDuration = 4000;
        static readonly Color transparencyColor = Color.LimeGreen;
        List<(string Shortcut, string Description)> commandList = new List<(string, string)>();
        public List<Color> backgroundColors = new List<Color>();
        public List<Color> foregroundColors = new List<Color>();
        public List<string> fonts = new List<string>();
        int currentColorSchemeIndex = 0;
        TextWidthPreset currentTextWidthPreset;
        bool isTransparent;
        bool statusBarAtTop;
        bool hasUnsavedChanges;
        bool suppressWritingProgressTracking;
        bool goalPersistenceAvailable = true;
        bool settingsPersistenceAvailable = true;
        bool settingsLoaded;
        int currentFontIndex = 0;
        int currentFontSize = 18;
        string lastObservedEditorText = "";
        DailyWritingGoalState writingGoalState = new();
        DateTime notificationExpiresAt;
        public string currentFontName;
        public string currentFile = "";
        public int wordCount = 0;
        public int charactersCount = 0;
        public int charactersCountWithOutSpaces = 0;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer autoSaveTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer writingGoalSaveTimer = new System.Windows.Forms.Timer();
        KeyboardHelper kh;
        FileHelper fh;
        DailyWritingGoalStore writingGoalStore = new();
        UserSettingsStore userSettingsStore = new();
        UserSettings userSettings = new();

        private WaveOutEvent outputDevice;
        private VorbisWaveReader vorbisReader;
        int currentTrack = 0;   

        public Main()
        {
            InitializeComponent();
            Shown += Main_Shown;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CustomInitialize();
        }
        private void Main_Shown(object? sender, EventArgs e)
        {
            using DailyWritingGoalForm goalForm = new DailyWritingGoalForm(writingGoalState.Goal);
            if (goalForm.ShowDialog(this) == DialogResult.OK)
            {
                writingGoalState.Goal = goalForm.Goal;
                bool celebrate = writingGoalState.Goal > 0
                    && writingGoalState.Progress >= writingGoalState.Goal
                    && !writingGoalState.CelebrationShown;
                if (celebrate)
                {
                    writingGoalState.CelebrationShown = true;
                }

                PersistWritingGoalState();
                UpdateCounters();
                if (celebrate)
                {
                    ShowNotification("Obiettivo giornaliero raggiunto!");
                }
            }

            MainText.Focus();
        }
        private void MainText_TextChanged(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            TrackWritingProgress(MainText.Text);
            UpdateCounters();
        }
        private void MainText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Alt && e.KeyCode == Keys.B)
            {
                e.SuppressKeyPress = true;
                statusBarAtTop = !statusBarAtTop;
                LayoutMainContent();
                PersistUserSettings();
            }

            if (e.Alt && e.KeyCode == Keys.W)
            {
                e.SuppressKeyPress = true;
                currentTextWidthPreset = (TextWidthPreset)(
                    ((int)currentTextWidthPreset + 1) % Enum.GetValues<TextWidthPreset>().Length);
                LayoutMainContent();
                PersistUserSettings();
                string presetName = currentTextWidthPreset switch
                {
                    TextWidthPreset.NarrowLeft => "Narrow Left",
                    TextWidthPreset.NarrowRight => "Narrow Right",
                    _ => currentTextWidthPreset.ToString()
                };
                ShowNotification($"Larghezza testo: {presetName}");
            }

            // ENTER
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                if (!kh.CheckTab())
                {
                    int selectionStart = MainText.SelectionStart;
                    MainText.SelectedText = Environment.NewLine;
                    MainText.SelectionStart = selectionStart + Environment.NewLine.Length;
                    MainText.SelectionLength = 0;
                }
            }

            // HELP (F1)
            if (e.KeyCode == Keys.F1)
            {
                using HelpForm helpForm = new HelpForm(commandList);
                helpForm.ShowDialog(this);
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
            }

            // COLOR
            if (e.Alt && e.KeyCode == Keys.C) { 
                if (isTransparent)
                {
                    isTransparent = false;
                    currentColorSchemeIndex = 0;
                }
                else if (currentColorSchemeIndex == backgroundColors.Count - 1)
                {
                    isTransparent = true;
                }
                else
                {
                    currentColorSchemeIndex++;
                }
                UpdateColors();
            }

            // TRANSPARENT
            if (e.Alt && e.KeyCode == Keys.T) {
                isTransparent = !isTransparent;
                UpdateColors();
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
                        ShowNotification("Now playing: " + Path.GetFileName(nextSong));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error trying to play music: " + ex.Message);
                    }
                }
                else {
                    ShowNotification("Music Off");
                }
            }

            // FONT UP
            if (e.Alt && e.KeyCode == Keys.Oemplus){
                currentFontSize++;
                if (currentFontSize > 64) currentFontSize = 64;
                UpdateFont();
            }

            // FONT DOWN
            if (e.Alt && e.KeyCode == Keys.OemMinus){
                currentFontSize--;
                if(currentFontSize < 8) currentFontSize = 8;
                UpdateFont();
            }

            // SAVE
            if (e.Control && e.KeyCode == Keys.S){
                SaveCurrentFile();
            }

            // NEW FILE
            if (e.Control && e.KeyCode == Keys.N)
            {
                if (hasUnsavedChanges && !SaveCurrentFile())
                {
                    return;
                }

                currentFile = "";
                MainText.Clear();
                hasUnsavedChanges = false;
                UpdateCounters();
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
                if (!ConfirmOpenWithUnsavedChanges())
                {
                    return;
                }

                OpenedFile? openFileResult = null;
                try
                {
                    openFileResult = fh.OpenFile(this);
                }
                catch (Exception ex) when (IsFileOperationError(ex))
                {
                    MessageBox.Show(
                        this,
                        $"Impossibile aprire il file. Il documento corrente non è stato modificato.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                        "Apertura non riuscita",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (openFileResult is null)
                {
                    return;
                }

                suppressWritingProgressTracking = true;
                try
                {
                    MainText.Text = openFileResult.Contents;
                }
                finally
                {
                    suppressWritingProgressTracking = false;
                    lastObservedEditorText = MainText.Text;
                }
                MainText.SelectionStart = MainText.Text.Length;
                MainText.SelectionLength = 0;
                currentFile = openFileResult.Path;
                hasUnsavedChanges = false;
                UpdateCounters();
                if (openFileResult.BackupPath is null)
                {
                    ShowNotification("File aperto senza backup");
                }
                else
                {
                    ShowNotification($"Backup creato: {Path.GetFileName(openFileResult.BackupPath)}");
                }
                UpdateCounters();
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            EnsureWritingGoalDate();
            UpdateClock();
            if (Notification.Text.Length > 0 && DateTime.Now >= notificationExpiresAt)
            {
                Notification.Text = "";
            }
        }
        private void autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFile) && hasUnsavedChanges)
            {
                SaveCurrentFile();
            }
        }
        private void writingGoalSaveTimer_Tick(object? sender, EventArgs e)
        {
            writingGoalSaveTimer.Stop();
            PersistWritingGoalState();
        }
        private void TrackWritingProgress(string currentText)
        {
            if (suppressWritingProgressTracking)
            {
                lastObservedEditorText = currentText;
                return;
            }

            int insertedCharacters = CountInsertedWritingCharacters(lastObservedEditorText, currentText);
            lastObservedEditorText = currentText;
            if (insertedCharacters == 0)
            {
                return;
            }

            EnsureWritingGoalDate();
            int previousProgress = writingGoalState.Progress;
            writingGoalState.Progress = (int)Math.Min(
                int.MaxValue,
                (long)writingGoalState.Progress + insertedCharacters);

            bool celebrate = writingGoalState.Goal > 0
                && previousProgress < writingGoalState.Goal
                && writingGoalState.Progress >= writingGoalState.Goal
                && !writingGoalState.CelebrationShown;
            if (celebrate)
            {
                writingGoalState.CelebrationShown = true;
                ShowNotification("Obiettivo giornaliero raggiunto!");
            }

            writingGoalSaveTimer.Stop();
            writingGoalSaveTimer.Start();
        }
        private static int CountInsertedWritingCharacters(string previousText, string currentText)
        {
            int prefixLength = 0;
            while (prefixLength < previousText.Length
                && prefixLength < currentText.Length
                && previousText[prefixLength] == currentText[prefixLength])
            {
                prefixLength++;
            }

            int suffixLength = 0;
            while (suffixLength < previousText.Length - prefixLength
                && suffixLength < currentText.Length - prefixLength
                && previousText[previousText.Length - suffixLength - 1]
                    == currentText[currentText.Length - suffixLength - 1])
            {
                suffixLength++;
            }

            return Math.Max(0, currentText.Length - suffixLength - prefixLength);
        }
        private void EnsureWritingGoalDate()
        {
            string today = DateOnly.FromDateTime(DateTime.Today)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (writingGoalState.Date == today)
            {
                return;
            }

            writingGoalState.Date = today;
            writingGoalState.Progress = 0;
            writingGoalState.CelebrationShown = false;
            UpdateCounters();
        }
        private void PersistWritingGoalState()
        {
            if (!goalPersistenceAvailable)
            {
                return;
            }

            try
            {
                writingGoalStore.Save(writingGoalState);
            }
            catch (Exception ex) when (IsWritingGoalStorageError(ex))
            {
                goalPersistenceAvailable = false;
                writingGoalSaveTimer.Stop();
                MessageBox.Show(
                    this,
                    $"Non è stato possibile salvare l'obiettivo e i progressi di scrittura. I dati resteranno disponibili solo fino alla chiusura dell'applicazione.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Obiettivo non salvato",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        private static bool IsWritingGoalStorageError(Exception exception)
        {
            return exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException
                or System.Security.SecurityException
                or InvalidDataException
                or System.Text.Json.JsonException;
        }
        private bool SaveCurrentFile()
        {
            try
            {
                if (string.IsNullOrEmpty(currentFile))
                {
                    string savedPath = fh.SaveNewFile(MainText);
                    if (string.IsNullOrEmpty(savedPath))
                    {
                        return false;
                    }

                    currentFile = savedPath;
                }
                else
                {
                    fh.SaveFile(currentFile, MainText);
                }

                hasUnsavedChanges = false;
                UpdateCounters();
                return true;
            }
            catch (Exception ex) when (IsFileOperationError(ex))
            {
                MessageBox.Show(
                    this,
                    $"Il file non è stato salvato. I dati nell'editor sono ancora disponibili; riprova dopo aver risolto il problema.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Salvataggio non riuscito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
        private bool ConfirmOpenWithUnsavedChanges()
        {
            if (!hasUnsavedChanges)
            {
                return true;
            }

            DialogResult result = MessageBox.Show(
                this,
                "Ci sono modifiche non salvate. Vuoi salvarle prima di aprire un altro file?",
                "Modifiche non salvate",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            return result switch
            {
                DialogResult.Yes => SaveCurrentFile(),
                DialogResult.No => true,
                _ => false
            };
        }
        private static bool IsFileOperationError(Exception exception)
        {
            return exception is IOException
                or UnauthorizedAccessException
                or ArgumentException
                or NotSupportedException
                or System.Security.SecurityException
                or System.Runtime.InteropServices.ExternalException;
        }
        private void ShowNotification(string message, int duration = notificationDuration)
        {
            Notification.Text = message;
            notificationExpiresAt = DateTime.Now.AddMilliseconds(duration);
        }
        private void UpdateFont() {
            System.Drawing.Font currentFont = new System.Drawing.Font(fonts[currentFontIndex], currentFontSize);
            MainText.Font = currentFont;
            currentFontName = currentFont.Name;
            UpdateCounters();
            PersistUserSettings();
        }
        private void UpdateColors() {
            TransparencyKey = isTransparent ? transparencyColor : Color.Empty;
            BackColor = isTransparent ? transparencyColor : Color.Black;
            MainText.BackColor = isTransparent ? transparencyColor : backgroundColors[currentColorSchemeIndex];
            MainText.ForeColor = foregroundColors[currentColorSchemeIndex];
            if (isTransparent)
            {
                MainText.ForeColor = Color.Black;
            }
            PersistUserSettings();
        }
        private void CustomInitialize() {
            kh = new KeyboardHelper(MainText);
            fh = new FileHelper();
            System.Drawing.Font currentFont = new System.Drawing.Font("Courier New", 18);
            MainText.Font = currentFont;    
            currentFontName = currentFont.Name;       
            MainText.KeyDown += new KeyEventHandler(MainText_KeyDown);
            FormClosing += Main_FormClosing;
            MainText.AcceptsTab = true;
            fonts = GenericHelper.LoadFonts();
            commandList = GenericHelper.LoadCommandList();
            GenericHelper.LoadColorSchemes(backgroundColors, foregroundColors);
            try
            {
                writingGoalState = writingGoalStore.LoadForToday();
            }
            catch (Exception ex) when (IsWritingGoalStorageError(ex))
            {
                goalPersistenceAvailable = false;
                writingGoalState = new DailyWritingGoalState();
                MessageBox.Show(
                    this,
                    $"Non è stato possibile caricare l'obiettivo o i progressi salvati. Potrai comunque scrivere, ma i progressi di oggi non saranno ripristinati.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Obiettivo non caricato",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            try
            {
                userSettings = userSettingsStore.Load();
                currentColorSchemeIndex = Math.Clamp(
                    userSettings.ColorSchemeIndex,
                    0,
                    backgroundColors.Count - 1);
                currentFontSize = userSettings.FontSize;
                currentTextWidthPreset = userSettings.TextWidthPreset;
                statusBarAtTop = userSettings.StatusBarAtTop;
                isTransparent = userSettings.IsTransparent;
                int savedFontIndex = fonts.FindIndex(
                    font => string.Equals(font.Trim(), userSettings.FontName, StringComparison.OrdinalIgnoreCase));
                if (savedFontIndex >= 0)
                {
                    currentFontIndex = savedFontIndex;
                }
            }
            catch (Exception ex) when (IsUserSettingsStorageError(ex))
            {
                settingsPersistenceAvailable = false;
                userSettings = new UserSettings();
                MessageBox.Show(
                    this,
                    $"Non è stato possibile caricare le preferenze salvate. Verranno usati i valori predefiniti.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Preferenze non caricate",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            settingsLoaded = true;
            lastObservedEditorText = MainText.Text;
            StyleInitializer();
            UpdateClock();
            UpdateFont();
            UpdateColors();
            UpdateCounters();
            writingGoalSaveTimer.Interval = 1000;
            writingGoalSaveTimer.Tick += writingGoalSaveTimer_Tick;
            TimerHandler();
        }
        private void PersistUserSettings()
        {
            if (!settingsLoaded || !settingsPersistenceAvailable)
            {
                return;
            }

            userSettings.ColorSchemeIndex = currentColorSchemeIndex;
            userSettings.FontName = currentFontName;
            userSettings.FontSize = currentFontSize;
            userSettings.TextWidthPreset = currentTextWidthPreset;
            userSettings.StatusBarAtTop = statusBarAtTop;
            userSettings.IsTransparent = isTransparent;

            try
            {
                userSettingsStore.Save(userSettings);
            }
            catch (Exception ex) when (IsUserSettingsStorageError(ex))
            {
                settingsPersistenceAvailable = false;
                MessageBox.Show(
                    this,
                    $"Non è stato possibile salvare le preferenze. Le modifiche alle impostazioni resteranno attive solo fino alla chiusura dell'applicazione.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Preferenze non salvate",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        private static bool IsUserSettingsStorageError(Exception exception)
        {
            return exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException
                or System.Security.SecurityException
                or InvalidDataException
                or System.Text.Json.JsonException;
        }
        private void Main_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                DialogResult result = MessageBox.Show(
                    this,
                    "Ci sono modifiche non salvate. Vuoi salvarle prima di uscire?",
                    "Modifiche non salvate",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if ((result == DialogResult.Yes && !SaveCurrentFile())
                    || result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            if (!e.Cancel)
            {
                writingGoalSaveTimer.Stop();
                PersistWritingGoalState();
                writingGoalSaveTimer.Dispose();
            }
        }
        private void StyleInitializer() {
            MainText.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Counter.Anchor = Notification.Anchor = Clock.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Resize += (_, _) => LayoutMainContent();
            Notification.TextChanged += (_, _) => LayoutMainContent();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            LayoutMainContent();
        }
        private void LayoutMainContent()
        {
            int margin = 12;
            int gap = 8;
            int statusHeight = new[]
            {
                Counter.Height,
                Clock.Height,
                Notification.Height,
                DocumentStatus.Height,
                GoalStatus.Height
            }.Max();
            int statusY = statusBarAtTop
                ? margin
                : Math.Max(margin, ClientSize.Height - statusHeight - margin);
            Counter.Location = new Point(margin, statusY);
            Clock.Location = new Point(
                Math.Max(margin, ClientSize.Width - Clock.Width - margin),
                statusY);
            Notification.Location = new Point(
                Math.Max(margin, Clock.Left - Notification.Width - gap),
                statusY);
            GoalStatus.Location = new Point(
                Math.Max(margin, Notification.Left - GoalStatus.Width - gap),
                statusY);
            DocumentStatus.Location = new Point(
                Math.Max(margin, GoalStatus.Left - DocumentStatus.Width - gap),
                statusY);

            int textTop = statusBarAtTop ? statusY + statusHeight + gap : margin;
            int textBottom = statusBarAtTop
                ? margin
                : ClientSize.Height - statusY + gap;
            int availableTextWidth = Math.Max(0, ClientSize.Width - margin * 2);
            double widthRatio = currentTextWidthPreset switch
            {
                TextWidthPreset.Medium => 0.8,
                TextWidthPreset.Narrow or TextWidthPreset.NarrowLeft or TextWidthPreset.NarrowRight => 0.5,
                _ => 1.0
            };
            int textWidth = (int)Math.Round(availableTextWidth * widthRatio);
            int textX = currentTextWidthPreset switch
            {
                TextWidthPreset.NarrowLeft => margin,
                TextWidthPreset.NarrowRight => ClientSize.Width - margin - textWidth,
                _ => (ClientSize.Width - textWidth) / 2
            };
            int textHeight = Math.Max(0, ClientSize.Height - textTop - textBottom);
            MainText.Bounds = new Rectangle(textX, textTop, textWidth, textHeight);
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
            charactersCount = MainText.Text.Length;
            charactersCountWithOutSpaces = MainText.Text.Replace(" ", "").Length;
            wordCount = GenericHelper.CountWords(MainText.Text);
            Counter.Text = 
                $"WORDS: {wordCount} | CHARACTERS: {charactersCount} | WITHOUT SPACES: {charactersCountWithOutSpaces} | " +
                $"FONT: {currentFontName} at {currentFontSize}pt | TODAY: {writingGoalState.Progress}" +
                (writingGoalState.Goal > 0 ? $"/{writingGoalState.Goal}" : "");
            DocumentStatus.Text = hasUnsavedChanges
                ? "Modifiche non salvate"
                : string.IsNullOrEmpty(currentFile) && MainText.TextLength == 0
                    ? "Bozza pronta"
                    : "Salvato";
            DocumentStatus.ForeColor = hasUnsavedChanges
                ? Color.FromArgb(255, 196, 115)
                : Color.Silver;
            bool goalReached = writingGoalState.Goal > 0
                && writingGoalState.Progress >= writingGoalState.Goal
                && writingGoalState.CelebrationShown;
            GoalStatus.Text = goalReached ? "Obiettivo raggiunto" : "";
            LayoutMainContent();
        }
        private void UpdateClock() { 
            Clock.Text = DateTime.Now.DayOfWeek + " " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
        }
    }
}
