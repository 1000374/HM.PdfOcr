using System.Windows.Forms;

namespace HM.PdfOcr.UCControl
{
    partial class WaitingForm
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
            this.ucLoading1 = new HM.PdfOcr.UCControl.UCLoading();
            this.SuspendLayout();
            // 
            // ucLoading1
            // 
            this.ucLoading1.BackColor = System.Drawing.Color.White;
            this.ucLoading1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ucLoading1.Location = new System.Drawing.Point(190, 43);
            this.ucLoading1.MinimumSize = new System.Drawing.Size(40, 80);
            this.ucLoading1.Name = "ucLoading1";
            this.ucLoading1.Size = new System.Drawing.Size(95, 126);
            this.ucLoading1.TabIndex = 6;
            this.ucLoading1.Text = "ucLoading1";
            // 
            // WaitingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(471, 249);
            this.Controls.Add(this.ucLoading1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "WaitingForm";
            this.Opacity = 0.5D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.ResumeLayout(false);

        }

        #endregion
        private UCLoading ucLoading1;
    }
}