using HM.PdfOcr.UCControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HM.PdfOcr
{
    public partial class WatermarkForm : Form
    {
        public string WaterMark => textBox1.Text;
        public int FontSize => Convert.ToInt16(textBox2.Text);
        public int WaterIndex => comboBox1.SelectedIndex;
        public Color WaterColor => Color.FromArgb(ucPanel1.Opacity * 255 / 100, ucPanel1.BackColor);
        public WatermarkForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            ucPanel1.Opacity = trackBar1.Value;
            ucPanel1.Refresh();
        }

        private void ucPanel1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ucPanel1.BackColor;
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            ucPanel1.BackColor = colorDialog.Color;
            ucPanel1.Refresh();
        }
    }
}
