namespace TextEdit
{
    public sealed class HelpForm : Form
    {
        private static readonly Color WindowColor = Color.FromArgb(17, 20, 21);
        private static readonly Color CardColor = Color.FromArgb(29, 34, 34);
        private static readonly Color AccentColor = Color.FromArgb(169, 199, 158);
        private static readonly Color MutedColor = Color.FromArgb(157, 166, 159);

        public HelpForm(IEnumerable<(string Shortcut, string Description)> commands)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            ShowIcon = false;
            KeyPreview = true;
            ClientSize = new Size(860, 570);
            BackColor = WindowColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            Text = "Comandi rapidi";
            List<(string Shortcut, string Description)> commandList = commands.ToList();

            Panel accent = new Panel
            {
                BackColor = AccentColor,
                Dock = DockStyle.Top,
                Height = 3
            };
            Controls.Add(accent);

            Label title = new Label
            {
                AutoSize = true,
                Text = "Comandi rapidi",
                Font = new Font("Segoe UI Semibold", 20F),
                ForeColor = Color.White,
                Location = new Point(28, 22)
            };
            Controls.Add(title);

            Label subtitle = new Label
            {
                AutoSize = true,
                Text = "Tasti e scorciatoie disponibili nell'editor",
                Font = new Font("Segoe UI", 10F),
                ForeColor = MutedColor,
                Location = new Point(30, 61)
            };
            Controls.Add(subtitle);

            Button closeButton = new Button
            {
                Text = "X",
                Font = new Font("Segoe UI Semibold", 14F),
                ForeColor = Color.White,
                BackColor = CardColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(44, 44),
                Location = new Point(ClientSize.Width - 60, 18),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 70, 63);
            closeButton.Click += (_, _) => Close();
            Controls.Add(closeButton);

            TableLayoutPanel commandGrid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = (commandList.Count + 1) / 2,
                Location = new Point(24, 105),
                Size = new Size(812, ((commandList.Count + 1) / 2) * 48),
                BackColor = WindowColor,
                Padding = Padding.Empty
            };
            commandGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            commandGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int row = 0; row < commandGrid.RowCount; row++)
            {
                commandGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            }

            int commandIndex = 0;
            foreach ((string Shortcut, string Description) command in commandList)
            {
                int column = commandIndex / commandGrid.RowCount;
                int row = commandIndex % commandGrid.RowCount;
                commandGrid.Controls.Add(CreateCommandCard(command), column, row);
                commandIndex++;
            }
            Controls.Add(commandGrid);

            Label footer = new Label
            {
                AutoSize = true,
                Text = "Premi Esc o X per chiudere",
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedColor,
                Location = new Point(30, 529)
            };
            Controls.Add(footer);

            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };
        }

        private static Panel CreateCommandCard((string Shortcut, string Description) command)
        {
            Panel card = new Panel
            {
                BackColor = CardColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 4, 8, 4)
            };

            Label shortcut = new Label
            {
                AutoSize = false,
                Text = command.Shortcut,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = AccentColor,
                BackColor = Color.FromArgb(39, 48, 44),
                Location = new Point(8, 8),
                Size = new Size(132, 30)
            };
            card.Controls.Add(shortcut);

            Label description = new Label
            {
                AutoSize = false,
                Text = command.Description,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(232, 235, 231),
                Location = new Point(148, 2),
                Size = new Size(232, 36)
            };
            card.Controls.Add(description);
            return card;
        }
    }
}
