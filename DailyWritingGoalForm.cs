namespace TextEdit
{
    public sealed class DailyWritingGoalForm : Form
    {
        private static readonly Color WindowColor = Color.FromArgb(17, 20, 21);
        private static readonly Color CardColor = Color.FromArgb(29, 34, 34);
        private static readonly Color AccentColor = Color.FromArgb(169, 199, 158);
        private readonly NumericUpDown goalInput;

        public int Goal => (int)goalInput.Value;

        public DailyWritingGoalForm(int currentGoal)
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            ShowIcon = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(420, 250);
            BackColor = WindowColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            Text = "Obiettivo di scrittura";

            Label title = new Label
            {
                AutoSize = true,
                Text = "Obiettivo di oggi",
                Font = new Font("Segoe UI Semibold", 20F),
                ForeColor = Color.White,
                Location = new Point(28, 24)
            };
            Controls.Add(title);

            Label description = new Label
            {
                AutoSize = true,
                Text = "Quanti caratteri vuoi scrivere oggi?",
                ForeColor = Color.FromArgb(190, 198, 191),
                Location = new Point(31, 73)
            };
            Controls.Add(description);

            goalInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 10000000,
                Increment = 100,
                Value = Math.Clamp(currentGoal > 0 ? currentGoal : 500, 1, 10000000),
                ThousandsSeparator = true,
                Font = new Font("Segoe UI Semibold", 16F),
                BackColor = CardColor,
                ForeColor = Color.White,
                Location = new Point(30, 112),
                Size = new Size(360, 42)
            };
            Controls.Add(goalInput);

            Button startButton = new Button
            {
                Text = "Inizia a scrivere",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = WindowColor,
                Font = new Font("Segoe UI Semibold", 10F),
                Location = new Point(210, 183),
                Size = new Size(180, 40)
            };
            startButton.FlatAppearance.BorderSize = 0;
            Controls.Add(startButton);

            Button skipButton = new Button
            {
                Text = "Salta",
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = WindowColor,
                ForeColor = Color.FromArgb(190, 198, 191),
                Location = new Point(30, 183),
                Size = new Size(100, 40)
            };
            skipButton.FlatAppearance.BorderSize = 0;
            Controls.Add(skipButton);

            AcceptButton = startButton;
            CancelButton = skipButton;
        }
    }
}
