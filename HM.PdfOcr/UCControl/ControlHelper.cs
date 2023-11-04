using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.UI;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;

namespace HM.PdfOcr.UCControl
{
    internal static class ControlExtension
    {
        public static void ShowLoading(this Control parent, string strMessage = "加载中...")
        {
            //var childs = (parent as Form).OwnedForms;
            //foreach (var item in childs)
            //{
            //    item?.Close();
            //}
            //var waitingForm = new WaitingForm()
            //{
            //    Width = parent.Width - 20,
            //    Height = parent.Height - 20,
            //};
            //waitingForm.Msg = strMessage;
            //waitingForm.Show(parent);
            parent.EnableTool(false);
            ShowProcessPanel(parent, strMessage);
        }

        public static void CloseLoading(this Control parent)
        {
            //var childs = (parent as Form).OwnedForms;
            //foreach (var item in childs)
            //{
            //    item?.Close();
            //}
            parent.EnableTool(true);
            CloseWaiting(parent);
        }
        private static void EnableTool(this Control parent,bool state)
        {
            var tool = parent.Controls.OfType<ToolStrip>()?.FirstOrDefault();
            if (tool != null)
                tool.Enabled = state;
        }
        private static void ShowProcessPanel(Control parent, string strMessage, int intSplashScreenDelayTime = 0)
        {
            if (parent.InvokeRequired)
            {
                parent.BeginInvoke(new MethodInvoker(delegate
                {
                    ShowProcessPanel(parent, strMessage, intSplashScreenDelayTime);
                }));
            }
            else
            {
                parent.VisibleChanged -= new EventHandler(parent_VisibleChanged);
                parent.VisibleChanged += new EventHandler(parent_VisibleChanged);
                parent.FindForm().FormClosing -= ControlHelper_FormClosing;
                parent.FindForm().FormClosing += ControlHelper_FormClosing;
                Control control = null;
                lock (parent)
                {
                    control = HaveProcessPanelControl(parent);
                    if (control == null)
                    {
                        control = CreateProgressPanel();
                        parent.Controls.Add(control);
                        control.Dock = DockStyle.Fill;
                    }
                }
                control.BringToFront();
                var frmWaitingEx = control.Tag as WaitingForm;
                frmWaitingEx.Msg = strMessage;
                frmWaitingEx.ShowForm();
            }
        }
        private static Control HaveProcessPanelControl(Control parent)
        {
            Control[] array = parent.Controls.Find("myprogressPanelext", false);
            Control result;
            if (array.Length > 0)
            {
                result = array[0];
            }
            else
            {
                result = null;
            }
            return result;
        }
        private static Control CreateProgressPanel()
        {
            var panle = new UCPanel
            {
                Name = "myprogressPanelext",
                BackColor = Color.Transparent,
                Opacity = 0
            };
            var form = new WaitingForm
            {
                TopLevel = false,
            };
            panle.Tag = form;
            form.Dock = DockStyle.Fill;
            panle.Controls.Add(form);
            return panle;
        }

        private static void CloseWaiting(Control control)
        {
            Control[] array = control.Controls.Find("myprogressPanelext", false);
            if (array.Length > 0)
            {
                Control control2 = array[0];
                if (control2.Tag != null && control2.Tag is WaitingForm)
                {
                    WaitingForm frmWaitingEx = control2.Tag as WaitingForm;
                    if (frmWaitingEx != null && !frmWaitingEx.IsDisposed && frmWaitingEx.Visible)
                    {
                        frmWaitingEx.Hide();
                    }
                }
                control2.SendToBack();
            }
        }

        private static void ControlHelper_FormClosing(object sender, FormClosingEventArgs e)
        {
            Control control = sender as Control;
            control.FindForm().FormClosing -= ControlHelper_FormClosing;
            CloseWaiting(control);
        }


        private static void parent_VisibleChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            control.VisibleChanged -= new EventHandler(parent_VisibleChanged);
            if (!control.Visible)
            {
                CloseWaiting(control);
            }
        }
    }

 }
