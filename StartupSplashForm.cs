using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TextEdit
{
    internal sealed class StartupSplashForm : Form
    {
        private static readonly Color SplashColor = Color.FromArgb(17, 20, 21);
        private readonly WebView2 logoView;
        private readonly Label fallbackMessage;
        private readonly System.Windows.Forms.Timer closeTimer;

        public StartupSplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            ShowIcon = false;
            ClientSize = new Size(360, 360);
            BackColor = SplashColor;
            Text = "Drafty";

            logoView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = SplashColor
            };
            logoView.NavigationCompleted += LogoView_NavigationCompleted;
            Controls.Add(logoView);

            fallbackMessage = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Silver,
                BackColor = SplashColor,
                Font = new Font("Segoe UI", 10F),
                Visible = false
            };
            Controls.Add(fallbackMessage);

            closeTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            closeTimer.Tick += (_, _) =>
            {
                closeTimer.Stop();
                Close();
            };
            Shown += StartupSplashForm_Shown;
        }

        private async void StartupSplashForm_Shown(object? sender, EventArgs e)
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "Data", "logo.svg");
            if (!File.Exists(logoPath))
            {
                ShowFallback("Animazione del logo non disponibile.");
                return;
            }

            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Drafty",
                    "WebView2");
                CoreWebView2Environment environment =
                    await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                if (IsDisposed || Disposing)
                {
                    return;
                }

                await logoView.EnsureCoreWebView2Async(environment);
                if (IsDisposed || Disposing)
                {
                    return;
                }

                CoreWebView2Settings settings = logoView.CoreWebView2.Settings;
                settings.AreDefaultContextMenusEnabled = false;
                settings.AreDevToolsEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsZoomControlEnabled = false;

                string svgMarkup = await File.ReadAllTextAsync(logoPath);
                if (IsDisposed || Disposing)
                {
                    return;
                }

                logoView.CoreWebView2.NavigateToString(
                    $"<!doctype html><html><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
                    $"<style>html,body{{width:100%;height:100%;margin:0;overflow:hidden;background:#111415}}" +
                    $"body{{display:flex;align-items:center;justify-content:center}}" +
                    $"svg{{display:block;width:100%;height:100%}}</style></head><body>{svgMarkup}</body></html>");
            }
            catch (Exception ex) when (
                ex is WebView2RuntimeNotFoundException
                    or IOException
                    or UnauthorizedAccessException
                    or System.Security.SecurityException
                    or System.Runtime.InteropServices.COMException)
            {
                ShowFallback("Animazione non disponibile. Avvio di Drafty...");
            }
        }

        private void LogoView_NavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ShowFallback("Impossibile visualizzare il logo. Avvio di Drafty...");
                return;
            }

            closeTimer.Start();
        }

        private void ShowFallback(string message)
        {
            logoView.Visible = false;
            fallbackMessage.Text = message;
            fallbackMessage.Visible = true;
            closeTimer.Interval = 1200;
            closeTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                closeTimer.Dispose();
                logoView.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
