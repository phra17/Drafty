using System.Text.Json;

namespace ZenScribe.Helpers
{
    public enum TextWidthPreset
    {
        Wide,
        Medium,
        Narrow,
        NarrowLeft,
        NarrowRight
    }

    public sealed class UserSettings
    {
        public int ColorSchemeIndex { get; set; }
        public string FontName { get; set; } = "Courier New";
        public int FontSize { get; set; } = 18;
        public TextWidthPreset TextWidthPreset { get; set; } = TextWidthPreset.Wide;
        public bool StatusBarAtTop { get; set; }
        public bool IsTransparent { get; set; }
    }

    public sealed class UserSettingsStore
    {
        private readonly string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Drafty",
            "settings.json");

        public UserSettings Load()
        {
            if (!File.Exists(filePath))
            {
                return new UserSettings();
            }

            string json = File.ReadAllText(filePath);
            UserSettings settings = JsonSerializer.Deserialize<UserSettings>(json)
                ?? throw new InvalidDataException("Il file delle preferenze è vuoto o non valido.");

            if (settings.ColorSchemeIndex < 0
                || settings.FontSize is < 8 or > 64
                || string.IsNullOrWhiteSpace(settings.FontName)
                || !Enum.IsDefined(settings.TextWidthPreset))
            {
                throw new InvalidDataException("Il file delle preferenze contiene valori non validi.");
            }

            return settings;
        }

        public void Save(UserSettings settings)
        {
            string directory = Path.GetDirectoryName(filePath)
                ?? throw new InvalidOperationException("Il percorso delle preferenze non contiene una cartella.");
            Directory.CreateDirectory(directory);

            string temporaryPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    JsonSerializer.Serialize(stream, settings);
                    stream.Flush(true);
                }

                if (File.Exists(filePath))
                {
                    File.Replace(temporaryPath, filePath, null);
                }
                else
                {
                    File.Move(temporaryPath, filePath);
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
    }
}
