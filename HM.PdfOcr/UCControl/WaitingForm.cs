using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HM.PdfOcr.UCControl
{
    public partial class WaitingForm : Form
    {
        public string Msg { get { return ucLoading1.Text; } set { ucLoading1.Text = value; } }
        public WaitingForm()
        {
            InitializeComponent();
        }
        protected override void OnResize(EventArgs e)
        {
            ucLoading1.Location = new Point(this.Width / 2 - ucLoading1.Width / 2, this.Height / 2 - ucLoading1.Height / 2);
            base.OnResize(e);
        }
        public void ShowForm()
        {
            base.Show();
        }
    }
}
