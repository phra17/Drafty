using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEdit
{
    public partial class Search : Form
    {
        public string ReturnValue { get; set; }
        public Search()
        {
            InitializeComponent();
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ReturnValue = SearchText.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.ReturnValue = SearchText.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
