using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sapper
{
    public partial class frmNameWinner : Form
    {
        public frmNameWinner()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                frmGame.NameWinner = "Anonym";
                Close();
            }
            else
                frmGame.NameWinner = textBox1.Text;
            frmResults frm3 = new frmResults();
            frm3.ShowDialog();
            Close();
        }
    }
}
