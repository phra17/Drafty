namespace TextEdit
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new DraftyApplicationContext());
        }
    }

    internal sealed class DraftyApplicationContext : ApplicationContext
    {
        private readonly Main mainForm = new();

        public DraftyApplicationContext()
        {
            StartupSplashForm splashForm = new();
            splashForm.FormClosed += SplashForm_FormClosed;
            MainForm = splashForm;
            splashForm.Show();
        }

        private void SplashForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not StartupSplashForm splashForm)
            {
                return;
            }

            MainForm = mainForm;
            splashForm.Dispose();
            mainForm.Show();
        }
    }
}