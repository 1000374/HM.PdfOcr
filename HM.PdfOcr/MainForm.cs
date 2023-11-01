using EZFontResolver1;
using OpenCvSharp;
using PdfiumViewer;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfDocument = PdfiumViewer.PdfDocument;

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
                    Dispose();
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
                }
                else
                {
                    pictureBox1.Visible = true;
                    pdfViewer1.Visible = false;

                    pdfViewer1.Document?.Dispose();
                    pictureBox1.Image = null;
                    pictureBox1.Image = OpenImage(fileName);
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
            {
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
            if (document == null)
            {
                AppendTextToRich($"请先打开pdf...");
                return true;
            }
            AppendTextToRich($"开始转换...");
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
                        PaddleOcrResult result = await OcrImage(image);

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

        private async Task<PaddleOcrResult> OcrImage(Image image)
        {
            var task =await Task.Run(() =>
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
              }).ConfigureAwait(false);
            return task;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            _copy.Checked = false;
            _matching.Checked = false;
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = false;
            _fitBest.Enabled = false;
            _matching.Enabled = false;
            _getTextFromPage.Enabled = false;
            _copy.Enabled = false;
            toolStripDropDownButton1.Enabled = false;
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

            Disposed += (s, ea) => pdfViewer1.Document?.Dispose();
        }

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
                PaddleOcrResult ocrResult = OcrImage(image).Result;
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
                MessageBox.Show("提取成功！");
            }
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
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                pdfViewer1.Renderer.ZoomOut();
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
                    PaddleOcrResult result = await OcrImage(image);
                    var text = result.Text;
                    AppendTextToRich($"页码： {page} \r\n{text}");
                }
            }
            else
            {
                var image = pictureBox1.Image;
                PaddleOcrResult result = await OcrImage(image);
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
            }
        }
        private void _fitBest_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
                FitPage(PdfViewerZoomMode.FitBest);
        }
        private async void 转换双层pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var newFile = $@"{Path.GetDirectoryName(fileName)}\{Path.GetFileNameWithoutExtension(fileName)}_Double.pdf";
                await GengeneratePDF(newFile, pdfViewer1.Document, true);
            }
        }

        private async void 直接生成pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var newFile = $@"{Path.GetDirectoryName(fileName)}\{Path.GetFileNameWithoutExtension(fileName)}_NoDouble.pdf";
                await GengeneratePDF(newFile, pdfViewer1.Document, false);
            }
        }
    }

    enum FileType
    {
        PDF,
        Image
    }
}
