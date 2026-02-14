namespace TextEdit
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainText = new RichTextBox();
            Counter = new Label();
            Clock = new Label();
            Notification = new Label();
            SuspendLayout();
            // 
            // MainText
            // 
            MainText.BackColor = Color.Black;
            MainText.BorderStyle = BorderStyle.None;
            MainText.Font = new Font("Courier New", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainText.ForeColor = Color.White;
            MainText.Location = new Point(12, 12);
            MainText.Name = "MainText";
            MainText.Size = new Size(1165, 680);
            MainText.TabIndex = 0;
            MainText.Text = "";
            MainText.TextChanged += MainText_TextChanged;
            // 
            // Counter
            // 
            Counter.AutoSize = true;
            Counter.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Counter.ForeColor = Color.Silver;
            Counter.Location = new Point(12, 695);
            Counter.Name = "Counter";
            Counter.Size = new Size(62, 17);
            Counter.TabIndex = 1;
            Counter.Text = "label1";
            // 
            // Clock
            // 
            Clock.AutoSize = true;
            Clock.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Clock.ForeColor = Color.Silver;
            Clock.Location = new Point(1115, 695);
            Clock.Name = "Clock";
            Clock.Size = new Size(62, 17);
            Clock.TabIndex = 2;
            Clock.Text = "label1";
            Clock.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Notification
            // 
            Notification.AutoSize = true;
            Notification.Font = new Font("Courier New", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Notification.ForeColor = Color.Silver;
            Notification.Location = new Point(1047, 695);
            Notification.Name = "Notification";
            Notification.Size = new Size(0, 17);
            Notification.TabIndex = 3;
            Notification.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1189, 724);
            Controls.Add(Notification);
            Controls.Add(Clock);
            Controls.Add(Counter);
            Controls.Add(MainText);
            Name = "Main";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox MainText;
        private Label Counter;
        private Label Clock;
        private Label Notification;
    }
}
