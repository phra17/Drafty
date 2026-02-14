namespace TextEdit
{
    partial class Search
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SearchText = new TextBox();
            Submit = new Button();
            SuspendLayout();
            // 
            // SearchText
            // 
            SearchText.Location = new Point(12, 15);
            SearchText.Name = "SearchText";
            SearchText.Size = new Size(303, 27);
            SearchText.TabIndex = 0;
            // 
            // Submit
            // 
            Submit.Location = new Point(327, 12);
            Submit.Name = "Submit";
            Submit.Size = new Size(104, 32);
            Submit.TabIndex = 1;
            Submit.Text = "Search";
            Submit.UseVisualStyleBackColor = true;
            Submit.Click += button1_Click;
            // 
            // Search
            // 
            AcceptButton = Submit;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(442, 54);
            Controls.Add(Submit);
            Controls.Add(SearchText);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Search";
            Text = "Search";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox SearchText;
        private Button Submit;
    }
}