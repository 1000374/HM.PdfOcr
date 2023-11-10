using EZFontResolver1;
using HM.PdfOcr.UCControl;
using OpenCvSharp;
using PdfiumViewer;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using PdfDocument = PdfiumViewer.PdfDocument;
using Point = System.Drawing.Point;

namespace HM.PdfOcr
{
    public partial class MainForm : Form
    {
        FileType fileType;
        string fileName;
        public MainForm()
        {
            InitializeComponent();
        }
        private void OpenFile()
        {
            _copy.Checked = false;
            _matching.Checked = false;
            using (var form = new OpenFileDialog())
            {
                form.RestoreDirectory = true;
                //"PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                form.Filter = "文件 (*.pdf)|*.pdf|图片(*.jpg)|*.gif;*.jpg;*.jpeg;*.bmp;*.jfif;*.png;|All Files (*.*)|*.*";
                form.Title = "打开文件";
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    // Dispose();
                    return;
                }
                fileName = form.FileName;
                fileType = string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.CurrentCultureIgnoreCase) ? FileType.PDF : FileType.Image;
                if (fileType == FileType.PDF)
                {
                    pictureBox1.Visible = false;
                    pdfViewer1.Visible = true;
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = null;
                    pdfViewer1.Document?.Dispose();
                    pdfViewer1.Document = OpenDocument(fileName);
                    toolStripButton7.Enabled = true;
                    pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.TextSelection;
                    _copy.Checked = true;
                }
                else
                {
                    pictureBox1.Visible = true;
                    pdfViewer1.Visible = false;
                    var image = OpenImage(fileName);
                    pdfViewer1.Document?.Dispose();
                    pictureBox1.Image = null;
                    pictureBox1.Image = image;
                }
                toolStripButton1.Enabled = true;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
                _fitBest.Enabled = true;
                _matching.Enabled = true;
                _getTextFromPage.Enabled = true;
                _copy.Enabled = true;
                toolStripDropDownButton1.Enabled = true;
            }
        }
        private PdfDocument OpenDocument(string fileName)
        {
            try
            {   // Remove ReadOnly attribute from the copy.
                File.SetAttributes(fileName, File.GetAttributes(fileName) & ~FileAttributes.ReadOnly);
                return PdfDocument.Load(this, new MemoryStream(File.ReadAllBytes(fileName)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private Image OpenImage(string fileName)
        {
            try
            {
                var bytes = File.ReadAllBytes(fileName);
                return Image.FromStream(new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void FitPage(PdfViewerZoomMode zoomMode)
        {
            int page = pdfViewer1.Renderer.Page;
            pdfViewer1.ZoomMode = zoomMode;
            pdfViewer1.Renderer.Zoom = 1;
            pdfViewer1.Renderer.Page = page;
        }
        private byte[] ImageToByte(System.Drawing.Image image)
        {
            MemoryStream ms = new MemoryStream();
            if (image == null)
                return new byte[ms.Length];
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] BPicture = new byte[ms.Length];
            BPicture = ms.GetBuffer();
            return BPicture;
        }
        private SizeF ConvertToRenderSize(float width, float height, float dpiX, float dpiY, PdfRotation rotate, PdfRenderFlags flags)
        {
            if ((flags & PdfRenderFlags.CorrectFromDpi) != 0)
            {
                width = width * dpiX / 72;
                height = height * dpiY / 72;
            }
            var size = new SizeF(width, height);
            return size;
        }

        private System.Drawing.RectangleF ConvertToPDFSize(System.Drawing.RectangleF rectangle, float dpiX, float dpiY, PdfRotation rotate, PdfRenderFlags flags)
        {
            var width = rectangle.Width;
            var height = rectangle.Height;
            var x = rectangle.X;
            var y = rectangle.Y;
            if ((flags & PdfRenderFlags.CorrectFromDpi) != 0)
            {
                width = (width / dpiX * 72);
                height = (height / dpiY * 72);
                x = (x / dpiX * 72);
                y = (y / dpiY * 72);
            }
            return new RectangleF(x, y, width, height);
        }
        private void AppendTextToRich(string input)
        {
            richTextBox1.SafeInvoke(x => x.AppendText($"【{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}】 {input}{Environment.NewLine}"));
            richTextBox1.SafeInvoke(x => x.SelectionStart = x.Text.Length);
            richTextBox1.SafeInvoke(x => x.ScrollToCaret());
        }

        private async Task<bool> GengeneratePDF(string filePath, IPdfDocument document, bool drawImage)
        {
            return await Task.Run<bool>(() =>
                 {
                     if (document == null)
                     {
                         AppendTextToRich($"请先打开pdf...");
                         return false;
                     }
                     AppendTextToRich($"开始转换,总共{document.PageCount}页，请稍后...");
                     using (PdfSharp.Pdf.PdfDocument xDoc = new PdfSharp.Pdf.PdfDocument())
                     {
                         for (int page = 0; page < document.PageCount; page++)
                         {
                             if (document.GetPdfEditable(page))
                             {
                                 AppendTextToRich($"当前{page}不需要转换");
                                 continue;
                             }
                             int dpiX = 96 * 5;
                             int dpiY = 96 * 5;
                             var pdfWidth = (int)document.PageSizes[page].Width * 4 / 3;
                             var pdfHeight = (int)document.PageSizes[page].Height * 4 / 3;
                             var rotate = PdfRotation.Rotate0;
                             var flags = PdfRenderFlags.Annotations | PdfRenderFlags.CorrectFromDpi;
                             using (var image = document.Render(page, pdfWidth, pdfHeight, dpiX, dpiY, rotate, flags))
                             {
                                 PaddleOcrResult result = OcrImage(image);

                                 xDoc.Pages.Add(new PdfPage() { Width = pdfWidth, Height = pdfHeight });
                                 PdfPage xPage = xDoc.Pages[page];
                                 XGraphics gfx = XGraphics.FromPdfPage(xPage);
                                 if (drawImage)
                                 {
                                     gfx.BeginMarkedContentPropList("oc1");
                                     //var font = new XFont("微软雅黑", 10, XFontStyle.Regular);
                                     foreach (PaddleOcrResultRegion region in result.Regions)
                                     {
                                         var center = region.Rect.Center;
                                         var rect = region.Rect.BoundingRect();//rect.Y + rect.Height / 2
                                         var size = ConvertToPDFSize(new RectangleF(rect.X, center.Y + rect.Height / 4, rect.Width, rect.Height), dpiX, dpiY, rotate, flags);
                                         #region 字体等比例放大不适用
                                         //var width = size.Width;
                                         //var height = size.Height;
                                         //var textSize = gfx.MeasureString(region.Text, font);
                                         //var scale = Math.Min(width / textSize.Width, height / textSize.Height);
                                         //gfx.ScaleTransform(scale, scale,XMatrixOrder.Prepend);
                                         #endregion
                                         var font = GetFont(region.Text, gfx, size.Width, size.Height);
                                         gfx.DrawString(region.Text, font, XBrushes.Black, size.X, size.Y);
                                     }

                                     gfx.EndMarkedContent();
                                     PdfResources rsx = (xPage.Elements["/Resources"] as PdfResources);
                                     rsx.AddOCG("oc1", "Layer 1");
                                     gfx.DrawImage(image, 0, 0, pdfWidth, pdfHeight);
                                 }
                                 else
                                 {
                                     foreach (PaddleOcrResultRegion region in result.Regions)
                                     {
                                         var center = region.Rect.Center;
                                         var rect = region.Rect.BoundingRect();//rect.Y + rect.Height / 2
                                         var size = ConvertToPDFSize(new RectangleF(rect.X, center.Y + rect.Height / 4, rect.Width, rect.Height), dpiX, dpiY, rotate, flags);
                                         var font = GetFont(region.Text, gfx, size.Width, size.Height);
                                         gfx.DrawString(region.Text, font, XBrushes.Black, size.X, size.Y);
                                     }

                                 }
                                 AppendTextToRich($"第{page}页提取成功");
                             }
                         }
                         if (xDoc.PageCount > 0)
                         {
                             xDoc.Save(filePath);
                             AppendTextToRich($"提取{filePath}成功");
                         }
                         else
                         {
                             AppendTextToRich($"没有要转换的pdf");
                         }
                         xDoc.Close();
                     }
                     return true;
                 });
        }
        List<XFont> _fontCache = new List<XFont>();
        private XFont GetFont(string str, XGraphics g, float imgWidth, float imgHeight)
        {
            if (_fontCache.Count == 0)
            {
                for (int i = 1; i <= 26; i++)
                {
                    var font = new XFont("微软雅黑", i, XFontStyle.Regular);
                    _fontCache.Add(font);
                }
            }

            var _maxFontSize = 25;
            var _minFontSize = 5;

            // Measure with maximum sized font
            var baseSize = g.MeasureString(str, _fontCache[_maxFontSize]);

            // Downsample to actual image size
            var widthRatio = imgWidth / baseSize.Width;
            var heightRatio = imgHeight / baseSize.Height;
            var minRatio = Math.Min(widthRatio, heightRatio);
            int estimatedFontSize = (int)(_maxFontSize * minRatio);

            // Make sure the precomputed font list is always hit
            if (estimatedFontSize > _maxFontSize)
                estimatedFontSize = _maxFontSize;
            else if (estimatedFontSize < _minFontSize)
                estimatedFontSize = _minFontSize;

            // Make sure the estimated size is not too large
            var estimatedSize = g.MeasureString(str, _fontCache[estimatedFontSize]);
            bool estimatedSizeWasReduced = false;
            while (estimatedSize.Width > imgWidth || estimatedSize.Height > imgHeight)
            {
                if (estimatedFontSize == _minFontSize)
                    break;
                --estimatedFontSize;
                estimatedSizeWasReduced = true;

                estimatedSize = g.MeasureString(str, _fontCache[estimatedFontSize]);
                //++counter;
            }

            // Can we increase the size a bit?
            if (!estimatedSizeWasReduced)
            {
                while (estimatedSize.Width < imgWidth && estimatedSize.Height < imgHeight)
                {
                    if (estimatedFontSize == _maxFontSize)
                        break;
                    ++estimatedFontSize;

                    estimatedSize = g.MeasureString(str, _fontCache[estimatedFontSize]);
                }

                // We increase the size until it is larger than the image, so we need to go back one step afterwards
                if (estimatedFontSize > _minFontSize)
                    --estimatedFontSize;
            }

            return _fontCache[estimatedFontSize];
        }

        private async Task<PaddleOcrResult> OcrImageAsync(Image image)
        {
            var task = await Task.Run(() =>
              {
                  return OcrImage(image);
              }).ConfigureAwait(false);
            return task;
        }
        private PaddleOcrResult OcrImage(Image image)
        {
            byte[] sampleImageData = ImageToByte(image);
            FullOcrModel model = LocalFullModels.ChineseV3;
            using (PaddleOcrAll all = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
            {
                AllowRotateDetection = true, /* 允许识别有角度的文字 */
                Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
            })
            {
                // Load local file by following code:
                using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                {
                    PaddleOcrResult result = all.Run(src);
                    return result;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _copy.Checked = false;
            _matching.Checked = false;
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = false;
            toolStripButton7.Enabled = false;
            _fitBest.Enabled = false;
            _matching.Enabled = false;
            _getTextFromPage.Enabled = false;
            _copy.Enabled = false;
            toolStripDropDownButton1.Enabled = false;
            pdfViewer1.ShowBookmarks = false;
            // Get the EZFontResolver.
            EZFontResolver fontResolver = EZFontResolver.Get;
            // Assign it to PDFsharp.
            GlobalFontSettings.FontResolver = fontResolver;
            fontResolver.AddFont("微软雅黑", XFontStyle.Regular, @"fonts\msyh.ttf", true, true);

            pdfViewer1.Renderer.ContextMenuStrip = pdfViewerContextMenu;
            pdfViewer1.Renderer.DisplayRectangleChanged += Renderer_DisplayRectangleChanged;
            pdfViewer1.Renderer.ZoomChanged += Renderer_ZoomChanged;
            pdfViewer1.Renderer.BoundedTextHandler += Renderer_BoundedTextHandler;
            _zoom.Text = pdfViewer1.Renderer.Zoom.ToString();
            Disposed += (s, ea) =>
            {
                pictureBox1.Image?.Dispose();
                pdfViewer1.Document?.Dispose();
            };

            this.pictureBox1.MouseWheel += pictureBox1_MouseWheel;
            this.pictureBox1.MouseHover += pictureBox1_MouseHover;
            this.pictureBox1.MouseDown += pictureBox1_MouseDown;
            this.pictureBox1.MouseMove += pictureBox1_MouseMove;
            this.pictureBox1.MouseUp += pictureBox1_MouseUp;
            splitContainer1.Panel1.Resize += (s, n) =>
            {
                var width = splitContainer1.Panel1.Width;
                var height = splitContainer1.Panel1.Height;
                pictureBox1.Width = width;
                pictureBox1.Height = height;
                pictureBox1.Location = new Point(0, 0);
            };
        }

        #region 图片缩放
        double zoomfactor = 1.0;
        int zoompos_x = 0;
        int zoompos_y = 0;
        bool Mousedown = false;
        Point Mousepos = new Point(0, 0);
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Mousewheel down
            if (e.Delta < 0)
            {
                ZoomOut();
            }
            // Mousewheel up
            else
            {
                ZoomIn();
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (zoomfactor > 1.01)
                {
                    Mousepos = e.Location;
                    Mousedown = true;
                }
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mousedown)
            {
                int dx = e.X - Mousepos.X;
                int dy = e.Y - Mousepos.Y;
                pictureBox1.Location = new Point(pictureBox1.Left + dx, pictureBox1.Top + dy);

                if (zoomfactor > 1.01)
                {
                    zoompos_x += (int)(dx / zoomfactor);
                    zoompos_y += (int)(dy / zoomfactor);
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Mousedown = false;
            }
        }

        private void ZoomIn()
        {
            // With first zoom-step center field of view
            if (zoomfactor > 0.99 && zoomfactor < 1.01)
            {
                zoompos_x = 0;
                zoompos_y = 0;
                zoomfactor = 1.5;
            }

            else if (zoomfactor > 1.49 && zoomfactor < 1.51) zoomfactor = 2.0;
            else if (zoomfactor > 1.99 && zoomfactor < 2.01) zoomfactor = 2.5;
            else if (zoomfactor > 2.49 && zoomfactor < 2.51) zoomfactor = 3.0;
            else if (zoomfactor > 2.99 && zoomfactor < 3.01) zoomfactor = 3.5;
            else if (zoomfactor > 3.49 && zoomfactor < 3.51) zoomfactor = 4.0;

            ShowPicZoomed(zoomfactor);
        }

        private void ZoomOut()
        {
            if (zoomfactor > 3.99) zoomfactor = 3.5;
            else if (zoomfactor > 3.49 && zoomfactor < 3.51) zoomfactor = 3.0;
            else if (zoomfactor > 2.99 && zoomfactor < 3.01) zoomfactor = 2.5;
            else if (zoomfactor > 2.49 && zoomfactor < 2.51) zoomfactor = 2.0;
            else if (zoomfactor > 1.99 && zoomfactor < 2.01) zoomfactor = 1.5;
            else zoomfactor = 1.0;

            ShowPicZoomed(zoomfactor);
        }
        private void ShowPicZoomed(double zoomfactor)
        {
            var width = splitContainer1.Panel1.Width;
            var height = splitContainer1.Panel1.Height;
            if (zoomfactor > 1)
            {
                pictureBox1.Width = (int)(width * zoomfactor);
                pictureBox1.Height = (int)(height * zoomfactor);

                var screensize_x = RectangleToScreen(this.ClientRectangle).Width;
                var screensize_y = RectangleToScreen(this.ClientRectangle).Height;

                // move picturebox to display desired field of view
                pictureBox1.Location = new Point((int)(screensize_x / 2 - (width * zoomfactor / 2) + zoompos_x * zoomfactor), (int)(screensize_y / 2 - (height * zoomfactor / 2) + zoompos_y * zoomfactor));
            }
            else
            {
                pictureBox1.Width = width;
                pictureBox1.Height = height;
                pictureBox1.Location = new Point(0, 0);
            }

            // Update titlebar & text in taskbar
            _zoom.Text = zoomfactor.ToString();
        }

        #endregion

        void Renderer_DisplayRectangleChanged(object sender, EventArgs e)
        {
            _page.Text = (pdfViewer1.Renderer.Page + 1).ToString();
        }

        void Renderer_ZoomChanged(object sender, EventArgs e)
        {
            _zoom.Text = pdfViewer1.Renderer.Zoom.ToString();
        }
        private void Renderer_BoundedTextHandler(int page, int x, int y, int x1, int y1, string txt, bool isEdit)
        {
            try
            {
                this.ShowLoading();
                if (isEdit)
                {
                    AppendTextToRich($"页码:{page},x:{x},y:{y},x1:{x1},y1:{y1} \r\n{txt}");
                }
                else
                {
                    AppendTextToRich($"正在提取框选坐标:{page},x:{x},y:{y},x1:{x1},y1:{y1}");
                    System.Drawing.Image image = pdfViewer1.Renderer.GetImage(page);
                    if (image == null)
                        return;
                    PaddleOcrResult ocrResult = OcrImageAsync(image).Result;
                    var text = ocrResult.Text;
                    var pageBounds = pdfViewer1.Renderer.GetBoundsOffset(page);
                    var sb = new StringBuilder();
                    Graphics g = pdfViewer1.Renderer.CreateGraphics();
                    Pen pen = new Pen(Color.Blue, 2.0f);
                    var pen1 = new Pen(Color.Yellow, 2.0f);

                    var pdfrect = new PdfiumViewer.PdfRectangle(page, new RectangleF(Math.Min(x, x1), Math.Min(y, y1), Math.Abs(x1 - x), Math.Abs(y1 - y)));
                    var rect = pdfViewer1.Renderer.BoundsFromPdf(pdfrect);
                    g.DrawRectangle(pen1, rect);

                    var showOcr = pdfViewer1.Renderer.GetBounds(page).Width;
                    var backSize = ConvertToRenderSize(pdfViewer1.Document.PageSizes[page].Width * 4 / 3, pdfViewer1.Document.PageSizes[page].Height * 4 / 3, 96, 96, PdfRotation.Rotate0, PdfRenderFlags.Annotations | PdfRenderFlags.CorrectFromDpi);
                    var scaleOcr = backSize.Width / showOcr;
                    var toImageRect = new RectangleF((rect.X - pageBounds.X) * scaleOcr, (rect.Y - pageBounds.Y) * scaleOcr, rect.Width * scaleOcr, rect.Height * scaleOcr);

                    foreach (PaddleOcrResultRegion region in ocrResult.Regions)
                    {
                        Rect boundRect = region.Rect.BoundingRect();
                        var rect1 = new Rectangle(boundRect.X + pageBounds.X, boundRect.Y + pageBounds.Y, boundRect.Width, boundRect.Height);
                        if (rect.Contains(rect1))
                        {
                            g.DrawRectangle(pen, rect1);
                            sb.Append(region.Text + " ");
                        }
                    }

                    g.Dispose();
                    txt = sb.ToString();
                    AppendTextToRich($"页码:{page},x:{toImageRect.X},y:{toImageRect.Y},Width:{toImageRect.Width},Hight:{toImageRect.Height} \r\n{txt}");
                }
            }
            finally { this.CloseLoading(); }
            MessageBox.Show("提取成功！");
        }

        private void _openFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                pdfViewer1.Renderer.Page--;
            }
            else
            {
                MessageBox.Show("图片不支持翻页");
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                pdfViewer1.Renderer.Page++;
            else
            {
                MessageBox.Show("图片不支持翻页");
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                pdfViewer1.Renderer.ZoomIn();
            else
            {
                ZoomIn();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                pdfViewer1.Renderer.ZoomOut();
            else
            {
                ZoomOut();
            }

        }
        private void _matching_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                _matching.Checked = !_matching.Checked;
                if (_matching.Checked)
                {
                    pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.Cross;
                    _copy.Checked = false;
                }
                else
                {
                    pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.Pan;
                }
            }
        }
        private void _copy_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                _copy.Checked = !_copy.Checked;
                if (_copy.Checked)
                {
                    pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.TextSelection;
                    _matching.Checked = false;
                }
                else
                {
                    pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.Pan;
                }
            }
        }
        private async void _getTextFromPage_Click(object sender, EventArgs e)
        {
            try
            {
                this.ShowLoading();
                if (fileType == FileType.PDF)
                {
                    int page = pdfViewer1.Renderer.Page;
                    if (pdfViewer1.Document.GetPdfEditable(page))
                    {
                        string text = pdfViewer1.Document.GetPdfText(page);

                        AppendTextToRich($"页码： {page} \r\n{text}");
                    }
                    else
                    {
                        AppendTextToRich($"正在提取第{page}页");
                        System.Drawing.Image image = pdfViewer1.Renderer.GetImage(page);
                        PaddleOcrResult result = await OcrImageAsync(image);
                        var text = result.Text;
                        AppendTextToRich($"页码： {page} \r\n{text}");
                    }
                }
                else
                {
                    AppendTextToRich($"开始提取图片");
                    var image = pictureBox1.Image;
                    PaddleOcrResult result = await OcrImageAsync(image);
                    var text = result.Text;
                    AppendTextToRich($"解析图片： \r\n{text}");
                    int n = 1;
                    foreach (PaddleOcrResultRegion region in result.Regions)
                    {
                        Rect boundRect = region.Rect.BoundingRect();
                        var rect1 = new Rectangle(boundRect.X, boundRect.Y, boundRect.Width, boundRect.Height);
                        if (region.Score > 0.5)
                        {
                            using (var pen = new Pen(Color.Blue, 2.0f))
                            using (Graphics graphics = Graphics.FromImage(image))
                            {
                                graphics.DrawString(n.ToString(), new Font("宋体", 15), Brushes.Red, boundRect.X + boundRect.Width / 2, boundRect.Y + boundRect.Height / 2 - 10);
                                graphics.DrawRectangle(pen, rect1);
                            }
                            AppendTextToRich($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                            n++;
                        }
                    }
                    pictureBox1.Refresh();
                }
            }
            finally
            {
                this.CloseLoading();
            }
        }
        private void _fitBest_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                FitPage(PdfViewerZoomMode.FitBest);
            else
            {
                zoomfactor = 1;
                ZoomOut();
            }
        }
        private async void 转换双层pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.ShowLoading();
                if (fileType == FileType.PDF)
                {
                    var newFile = $@"{Path.GetDirectoryName(fileName)}\{Path.GetFileNameWithoutExtension(fileName)}_Double.pdf";
                    await GengeneratePDF(newFile, pdfViewer1.Document, true);
                }
            }
            finally { this.CloseLoading(); }

        }

        private async void 直接生成pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.ShowLoading();
                if (fileType == FileType.PDF)
                {
                    var newFile = $@"{Path.GetDirectoryName(fileName)}\{Path.GetFileNameWithoutExtension(fileName)}_NoDouble.pdf";
                    await GengeneratePDF(newFile, pdfViewer1.Document, false);
                }
            }
            finally { this.CloseLoading(); }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var show = new AboutForm();
            show.ShowDialog(this);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pdfViewer1.Renderer.CopySelection();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pdfViewer1.Renderer.SelectAll();
        }

        private void 取消选中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pdfViewer1.Renderer.DeSelectAll();
        }

        private void 旋转ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var document = pdfViewer1.Document;
                int page = pdfViewer1.Renderer.Page;
                var rotate = (int)document.GetRotation(page);
                if (rotate < 3)
                    rotate++;
                else
                    rotate = 0;
                pdfViewer1.Document = null;
                document.RotatePage(page, (PdfRotation)rotate);
                var memoryStream = new MemoryStream();
                document.Save(memoryStream);
                pdfViewer1.Document = PdfDocument.Load(this, memoryStream);
                pdfViewer1.Renderer.Page = page;
            }
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                int page = pdfViewer1.Renderer.Page;
                pdfViewer1.Document.DeletePage(page);
            }
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            toolStripButton7.Checked = !toolStripButton7.Checked;
            pdfViewer1.ShowBookmarks = toolStripButton7.Checked;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            toolStripButton6.Checked = !toolStripButton6.Checked;
            splitContainer1.Panel2Collapsed = toolStripButton6.Checked;
        }

        private void 合并PDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                try
                {
                    this.ShowLoading();
                    var file = "";
                    using (var form = new OpenFileDialog())
                    {
                        form.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                        form.RestoreDirectory = true;
                        form.Title = "Open PDF File";

                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            Dispose();
                            return;
                        }
                        file = form.FileName;
                    }
                    var document = pdfViewer1.Document;
                    var doc = PdfSupport.MergePDF(document, OpenDocument(file));
                    {
                        if (doc != null)
                        {
                            pdfViewer1.Document = null;
                            pdfViewer1.Document = doc;
                        }
                    }
                    AppendTextToRich($"合并pdf已完成{file}");
                }
                finally { this.CloseLoading(); }
            }
        }

        private void 拆分PDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                try
                {
                    this.ShowLoading();
                    var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var document = pdfViewer1.Document;
                    {
                        for (int i = 0; i < document.PageCount; i++)
                        {
                            var filePath = Path.Combine(path, $"{i}.pdf");
                            using (var doc = PdfSupport.GetPDFPage(document, i + 1))
                            {
                                doc.Save(filePath);
                            }
                        }
                    }
                    AppendTextToRich($"拆分pdf已完成{path}");
                }
                finally { this.CloseLoading(); }
            }
        }
        private void pdf信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                PdfInformation info = pdfViewer1.Document.GetInformation();
                StringBuilder sz = new StringBuilder();
                sz.AppendLine($"Author: {info.Author}");
                sz.AppendLine($"Creator: {info.Creator}");
                sz.AppendLine($"Keywords: {info.Keywords}");
                sz.AppendLine($"Producer: {info.Producer}");
                sz.AppendLine($"Subject: {info.Subject}");
                sz.AppendLine($"Title: {info.Title}");
                sz.AppendLine($"Create Date: {info.CreationDate}");
                sz.AppendLine($"Modified Date: {info.ModificationDate}");
                AppendTextToRich(sz.ToString());

            }
        }

        private void pdf转成图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                try
                {
                    this.ShowLoading();
                    var document = pdfViewer1.Document;
                    var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var dpiX = 96 * 2;
                    var dpiY = 96 * 2;
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        using (var image = document.Render(i, (int)document.PageSizes[i].Width * 4 / 3, (int)document.PageSizes[i].Height * 4 / 3, dpiX, dpiY, PdfRotation.Rotate0, PdfRenderFlags.Annotations | PdfRenderFlags.CorrectFromDpi))
                        {
                            image.Save(Path.Combine(path, i + ".png"));
                        }
                    }
                    AppendTextToRich($"pdf转图片已完成{path}");
                }
                finally
                {
                    this.CloseLoading();
                }
            }
        }

        private void 导出pdf内资源图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                try
                {
                    this.ShowLoading();
                    var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (var memoryStream = new MemoryStream())
                    {
                        pdfViewer1.Document.Save(memoryStream);
                        var document = PdfReader.Open(memoryStream);
                        var imageCount = 0;
                        // Iterate the pages.
                        foreach (var page in document.Pages)
                        {
                            // Get the resources dictionary.
                            var resources = page.Elements.GetDictionary("/Resources");
                            if (resources == null)
                                continue;

                            // Get the external objects dictionary.
                            var xObjects = resources.Elements.GetDictionary("/XObject");
                            if (xObjects == null)
                                continue;

                            var items = xObjects.Elements.Values;
                            // Iterate the references to external objects.
                            foreach (var item in items)
                            {
                                var reference = item as PdfReference;
                                if (reference == null)
                                    continue;

                                var xObject = reference.Value as PdfDictionary;
                                // Is external object an image?
                                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                {
                                    PDFHelper.ExportImage(xObject, path, ref imageCount);
                                }
                            }
                        }
                        AppendTextToRich($"导出pdf图片成功总数{imageCount} {path}");
                    }
                }
                finally { this.CloseLoading(); }
            }
        }

        private void 两页pdf合并成页ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                using (var stream = new MemoryStream())
                {
                    pdfViewer1.Document.Save(stream);
                    stream.Position = 0;

                    // Create the output document.
                    var outputDocument = new PdfSharp.Pdf.PdfDocument();
                    var outputmemoryStream = new MemoryStream();
                    // Show single pages.
                    // (Note: one page contains two pages from the source document)
                    outputDocument.PageLayout = PdfPageLayout.SinglePage;

                    var font = new XFont("微软雅黑", 8, XFontStyle.Regular);
                    var format = new XStringFormat();
                    format.Alignment = XStringAlignment.Center;
                    format.LineAlignment = XLineAlignment.Far;

                    // Open the external document as XPdfForm object.
                    var form = XPdfForm.FromStream(stream);

                    for (var idx = 0; idx < form.PageCount; idx += 2)
                    {
                        // Add a new page to the output document.
                        var page = outputDocument.AddPage();
                        page.Orientation = PageOrientation.Landscape;
                        double width = page.Width;
                        double height = page.Height;

                        var gfx = XGraphics.FromPdfPage(page);

                        // Set the page number (which is one-based).
                        form.PageNumber = idx + 1;

                        var box = new XRect(0, 0, width / 2, height);
                        // Draw the page identified by the page number like an image.
                        gfx.DrawImage(form, box);

                        // Write page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(String.Format("- {0} -", idx + 1),
                            font, XBrushes.Red, box, format);

                        if (idx + 1 >= form.PageCount)
                            continue;

                        // Set the page number (which is one-based).
                        form.PageNumber = idx + 2;

                        box = new XRect(width / 2, 0, width / 2, height);
                        // Draw the page identified by the page number like an image.
                        gfx.DrawImage(form, box);

                        // Write page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(String.Format("- {0} -", idx + 2),
                            font, XBrushes.Red, box, format);
                    }
                    outputDocument.Save(outputmemoryStream);
                    pdfViewer1.Document = null;
                    pdfViewer1.Document = PdfDocument.Load(this, outputmemoryStream);
                }
            }
        }

        private void 添加密码ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                using (var form = new PasswordForm())
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var txt = form.Password;
                        using (var stream = new MemoryStream())
                        {
                            pdfViewer1.Document.Save(stream);
                            stream.Position = 0;
                            // Open an existing document. Providing an unrequired password is ignored.
                            var document = PdfReader.Open(stream);

                            var securitySettings = document.SecuritySettings;

                            // Setting one of the passwords automatically sets the security level to 
                            // PdfDocumentSecurityLevel.Encrypted128Bit.
                            securitySettings.UserPassword = txt;
                            //securitySettings.OwnerPassword = "owner";

                            // Don't use 40 bit encryption unless needed for compatibility.
                            //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

                            // Restrict some rights.
                            securitySettings.PermitAccessibilityExtractContent = false;
                            securitySettings.PermitAnnotations = false;
                            securitySettings.PermitAssembleDocument = false;
                            securitySettings.PermitExtractContent = false;
                            securitySettings.PermitFormsFill = true;
                            securitySettings.PermitFullQualityPrint = false;
                            securitySettings.PermitModifyDocument = true;
                            securitySettings.PermitPrint = false;

                            // Save the document...
                            document.Save(Path.Combine(path, Path.GetFileName(fileName)));
                        }
                        AppendTextToRich($"添加密码成功{path}");
                    }
                }
            }

        }

        private void 去除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                using (var form = new PasswordForm())
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var txt = form.Password;
                        using (var stream = new MemoryStream())
                        {
                            pdfViewer1.Document.Save(stream);
                            stream.Position = 0;
                            // Open an existing document. Providing an unrequired password is ignored.
                            var document = PdfReader.Open(stream, txt, PdfDocumentOpenMode.Modify);
                            document.SecuritySettings.DocumentSecurityLevel = PdfSharp.Pdf.Security.PdfDocumentSecurityLevel.None;
                            document.Save(Path.Combine(path, Path.GetFileName(fileName)));
                        }
                        AppendTextToRich($"去除密码成功{path}");
                    }
                }
            }
        }

        private void 添加水印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var watermark = "";
                var fontsize = 50;
                var waterIndex = 0;
                var color = Color.Red;
                var water = new WatermarkForm();
                if (water.ShowDialog(this) == DialogResult.OK)
                {
                    watermark = water.WaterMark;
                    fontsize = water.FontSize;
                    waterIndex = water.WaterIndex;
                    color = water.WaterColor;
                }
                else
                {
                    return;
                }
                using (var stream = new MemoryStream())
                {
                    pdfViewer1.Document.Save(stream);
                    stream.Position = 0;
                    var font = new XFont("微软雅黑", fontsize, XFontStyle.Regular, XPdfFontOptions.UnicodeDefault);
                    var document = PdfReader.Open(stream);

                    // Set version to PDF 1.4 (Acrobat 5) because we use transparency.
                    if (document.Version < 14)
                        document.Version = 14;

                    for (var idx = 0; idx < document.Pages.Count; idx++)
                    {
                        var page = document.Pages[idx];
                        var gfx = XGraphics.FromPdfPage(page);
                        var size = gfx.MeasureString(watermark, font);
                        gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                        gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
                        gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);
                        if (waterIndex == 0)
                        {
                            var format = new XStringFormat();
                            format.Alignment = XStringAlignment.Near;
                            format.LineAlignment = XLineAlignment.Near;
                            XBrush brush = new XSolidBrush(XColor.FromArgb(color));
                            gfx.DrawString(watermark, font, brush,
                                new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                                format);
                        }
                        else if (waterIndex == 1)
                        {
                            var path = new XGraphicsPath();
                            var format = new XStringFormat();
                            format.Alignment = XStringAlignment.Near;
                            format.LineAlignment = XLineAlignment.Near;
                            path.AddString(watermark, font.FontFamily, XFontStyle.BoldItalic, fontsize,
                            new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                                format);
                            var pen = new XPen(XColor.FromArgb(color), 2);
                            gfx.DrawPath(pen, path);
                        } 
                        if (waterIndex == 2)
                        {
                            var path = new XGraphicsPath();
                            var format = new XStringFormat();
                            format.Alignment = XStringAlignment.Near;
                            format.LineAlignment = XLineAlignment.Near;
                            path.AddString(watermark, font.FontFamily, XFontStyle.BoldItalic, fontsize,
                                new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                                format);                 
                            var pen = new XPen(XColor.FromArgb(50, 75, 0, 130), 3);
                            XBrush brush = new XSolidBrush(XColor.FromArgb(color));
                            gfx.DrawPath(pen, brush, path);
                        }
                    }
                    var outputmemoryStream = new MemoryStream();
                    document.Save(outputmemoryStream);
                    pdfViewer1.Document = null;
                    pdfViewer1.Document = PdfDocument.Load(this, outputmemoryStream);
                }
            }
        } 
    }

    enum FileType
    {
        PDF,
        Image
    }
}
