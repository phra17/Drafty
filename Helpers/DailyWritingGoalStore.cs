using System.Text.Json;
using System.Globalization;

namespace ZenScribe.Helpers
{
    public sealed class DailyWritingGoalState
    {
        public string Date { get; set; } = DateOnly.FromDateTime(DateTime.Today)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        public int Goal { get; set; }
        public int Progress { get; set; }
        public bool CelebrationShown { get; set; }
    }

    public sealed class DailyWritingGoalStore
    {
        private readonly string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Drafty",
            "writing-goal.json");

        public DailyWritingGoalState LoadForToday()
        {
            if (!File.Exists(filePath))
            {
                return new DailyWritingGoalState();
            }

            string json = File.ReadAllText(filePath);
            DailyWritingGoalState state = JsonSerializer.Deserialize<DailyWritingGoalState>(json)
                ?? throw new InvalidDataException("Il file dell'obiettivo giornaliero è vuoto o non valido.");

            if (!DateOnly.TryParseExact(
                state.Date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly savedDate)
                || state.Goal < 0
                || state.Progress < 0)
            {
                throw new InvalidDataException("Il file dell'obiettivo giornaliero contiene dati non validi.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            if (savedDate != today)
            {
                state.Date = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                state.Progress = 0;
                state.CelebrationShown = false;
            }

            return state;
        }

        public void Save(DailyWritingGoalState state)
        {
            string directory = Path.GetDirectoryName(filePath)
                ?? throw new InvalidOperationException("Il percorso delle impostazioni non contiene una cartella.");
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
                    JsonSerializer.Serialize(stream, state);
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
