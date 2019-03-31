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

    public partial class frmOption : Form
    {
        public frmOption()
        {
            InitializeComponent();
        }

        public class WrongInputData : System.Exception
        {
            public WrongInputData() : base() { }
            public WrongInputData(string message) : base(message) { }
            public WrongInputData(string message, System.Exception inner) : base(message, inner) { }

            // A constructor is needed for serialization when an
            // exception propagates from a remoting server to the client. 
            protected WrongInputData(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                frmGame.N = 10;
                frmGame.M = 10;
                frmGame.K = 10;
                frmGame.TypeGame = 0;
            }
            if (radioButton2.Checked)
            {
                frmGame.N = 16;
                frmGame.M = 16;
                frmGame.K = 40;
                frmGame.TypeGame = 1;
            }
            if (radioButton3.Checked)
            {
                frmGame.N = 30;
                frmGame.M = 16;
                frmGame.K = 100;
                frmGame.TypeGame = 2;
            }
            try
            {
                if (radioButton4.Checked)
                {
                    int M, N, K;
                    M = Convert.ToInt32(textBox1.Text);
                    N = Convert.ToInt32(textBox2.Text);
                    K = Convert.ToInt32(textBox3.Text);
                    string S = "";
                    if (M < 9)
                    {
                        S += "Height field is too small!\n";
                    }
                    if (M > 24)
                    {
                        S += "Field height is too high!\n";
                    }
                    if (N < 9)
                    {
                        S += "Width field is too small!\n";
                    }
                    if (N > 30)
                    {
                        S += "Field width is too high!\n";
                    }
                    if (K < 9)
                    {
                        S += "Small number of bombs!\n";
                    }
                    if (K > 250)
                    {
                        S += "Large number of bombs!";
                    }
                    if (S != "")
                    {
                        WrongInputData WrData = new WrongInputData(S);
                        throw WrData;
                    }
                    frmGame.TypeGame = 3;
                    frmGame.N = (ushort)N;
                    frmGame.M = (ushort)M;
                    frmGame.K = (ushort)K;
                }
                Close();
            }
            catch (WrongInputData WData)
            {

                MessageBox.Show(WData.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            catch (FormatException F)
            {
                MessageBox.Show("Fields are filled in incorrectly!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }


        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            label7.Enabled = true;
            label8.Enabled = true;
            label9.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
