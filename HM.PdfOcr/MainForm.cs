using DinkToPdf;
using DocumentFormat.OpenXml.Packaging;
using HM.PdfOcr.UCControl;
using OpenCvSharp;
using OpenXmlPowerTools;
using Pdfium.Net;
using Pdfium.Net.Native.Pdfium.Enums;
using Pdfium.Net.Wrapper;
using PdfiumViewer;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                form.Filter = "文件 (*.pdf)|*.pdf|图片(*.jpg)|*.gif;*.jpg;*.jpeg;*.bmp;*.jfif;*.png;|Word(*.docx)|*.docx|Word(*.doc)|*.doc|All Files (*.*)|*.*";
                form.Title = "打开文件";
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    // Dispose();
                    return;
                }
                fileName = form.FileName;
                fileType = string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.CurrentCultureIgnoreCase) ? FileType.PDF :
                   string.Equals(Path.GetExtension(fileName), ".DOCX", StringComparison.CurrentCultureIgnoreCase) ? FileType.Docx :
                   string.Equals(Path.GetExtension(fileName), ".DOC", StringComparison.CurrentCultureIgnoreCase) ? FileType.Doc : FileType.Image;
                if (fileType == FileType.PDF)
                {
                    LoadPdfFrom();
                }
                else if (fileType == FileType.Image)
                {
                    pictureBox1.Visible = true;
                    pdfViewer1.Visible = false;
                    var image = OpenImage(fileName);
                    pdfViewer1.Document?.Dispose(true);
                    pictureBox1.Image = null;
                    pictureBox1.Image = image;
                }
                else if (fileType == FileType.Docx)
                {
                    var stream = ParseDOCXToPdf(fileName);
                    LoadPdfFrom(stream);
                    fileType = FileType.PDF;
                }
                else if (fileType == FileType.Doc)
                {
                    string errorMsg = "";
                    var choosenOutputFile = WordHelper.DocToDocx(fileName, ref errorMsg);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        AppendTextToRich($"转换Doc2Docx失败{errorMsg}");
                        return;
                    }
                    if (File.Exists(choosenOutputFile))
                    {
                        var stream = ParseDOCXToPdf(choosenOutputFile);
                        LoadPdfFrom(stream);
                        fileType = FileType.PDF;
                        File.Delete(choosenOutputFile);
                    }
                    else
                    {
                        AppendTextToRich($"转换Doc2Docx失败");
                    }
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
        private void LoadPdfFrom(MemoryStream stream = null)
        {
            pictureBox1.Visible = false;
            pdfViewer1.Visible = true;
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
            pdfViewer1.Document?.Dispose(true);
            if (stream == null)
                pdfViewer1.Document = OpenDocument(fileName);
            else
                pdfViewer1.Document = PdfDocumentGdi.Load(this, stream);
            toolStripButton7.Enabled = true;
            pdfViewer1.Renderer.CursorMode = PdfViewerCursorMode.TextSelection;
            _copy.Checked = true;
            var fontPath = @"c:\Windows\fonts\simhei.ttf";
            var doc = pdfViewer1.Document;
            doc.LoadFont(fontPath);
        }

        private PdfDocument OpenDocument(string fileName)
        {
            try
            {   // Remove ReadOnly attribute from the copy.
                File.SetAttributes(fileName, File.GetAttributes(fileName) & ~FileAttributes.ReadOnly);
                return PdfDocumentGdi.Load(this, new MemoryStream(File.ReadAllBytes(fileName)));
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
        private SizeF ConvertToRenderSize(double width, double height, double dpiX, double dpiY, FpdfRotation rotate, RenderFlags flags)
        {
            if ((flags & RenderFlags.CorrectFromDpi) != 0)
            {
                width = width * dpiX / 72;
                height = height * dpiY / 72;
            }
            var size = new SizeF((float)width, (float)height);
            return size;
        }

        private System.Drawing.RectangleF ConvertToPDFSize(System.Drawing.RectangleF rectangle, float dpiX, float dpiY, FpdfRotation rotate, RenderFlags flags)
        {
            var width = rectangle.Width;
            var height = rectangle.Height;
            var x = rectangle.X;
            var y = rectangle.Y;
            if ((flags & RenderFlags.CorrectFromDpi) != 0)
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

        private async Task<bool> GengeneratePDF(string filePath, PdfDocument document, bool drawImage)
        {
            return await Task.Run<bool>(() =>
                 {
                     if (document == null)
                     {
                         AppendTextToRich($"请先打开pdf...");
                         return false;
                     }
                     AppendTextToRich($"开始转换,总共{document.PageCount}页，请稍后...");
                     using (PdfDocument xDoc = PdfDocument.CreateNew())
                     {
                         var fontPath = @"c:\Windows\fonts\simhei.ttf";
                         var doc = pdfViewer1.Document;
                         xDoc.LoadFont(fontPath);
                         for (int ipage = 0; ipage < document.PageCount; ipage++)
                         {
                             if (document.Pages[ipage].GetPdfEditable())
                             {
                                 AppendTextToRich($"当前{ipage}不需要转换");
                                 continue;
                             }
                             int dpiX = 96 * 5;
                             int dpiY = 96 * 5;
                             var pdfWidth = (int)document.Pages[ipage].Width * 4 / 3;
                             var pdfHeight = (int)document.Pages[ipage].Height * 4 / 3;
                             var rotate = FpdfRotation.Rotate0;
                             var flags = RenderFlags.Annotations | RenderFlags.CorrectFromDpi;
                             using (var image = document.Pages[ipage].Render(pdfWidth, pdfHeight, dpiX, dpiY, rotate, flags))
                             {
                                 PaddleOcrResult result = OcrImage(image);
                                 var page = xDoc.Pages.Insert(ipage, pdfWidth, pdfHeight);
                                 if (drawImage)
                                 {
                                     foreach (PaddleOcrResultRegion region in result.Regions)
                                     {
                                         var center = region.Rect.Center;
                                         var rect = region.Rect.BoundingRect();//rect.Y + rect.Height / 2
                                         var size = ConvertToPDFSize(new RectangleF(rect.X, center.Y + rect.Height / 4, rect.Width, rect.Height), dpiX, dpiY, rotate, flags);
                                         var fontSize = GetFont(region.Text, size.Width, size.Height).Size;
                                         page.AddString(region.Text, size.X, pdfHeight - size.Y, fontSize, Color.Black);
                                     }
                                     var scx = (float)pdfWidth / image.Width;
                                     var scy = (float)pdfHeight / image.Height;
                                     page.AddImage(image, 0, 0, scx, scy);
                                 }
                                 else
                                 {
                                     foreach (PaddleOcrResultRegion region in result.Regions)//心内1
                                     {
                                         var center = region.Rect.Center;
                                         var rect = region.Rect.BoundingRect();//rect.Y + rect.Height / 2
                                         var size = ConvertToPDFSize(new RectangleF(rect.X, (center.Y + rect.Height / 4), rect.Width, rect.Height), dpiX, dpiY, rotate, flags);
                                         var fontSize = GetFont(region.Text, size.Width, size.Height).Size;
                                         page.AddString(region.Text, size.X, pdfHeight - size.Y, fontSize, Color.Black);
                                     }
                                 }
                                 AppendTextToRich($"第{ipage}页提取成功");
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
                     }
                     return true;
                 });
        }
        List<Font> _fontCache = new List<Font>();
        private Font GetFont(string str, float imgWidth, float imgHeight)
        {
            var g = pdfViewer1.CreateGraphics();
            if (_fontCache.Count == 0)
            {
                for (int i = 1; i <= 26; i++)
                {
                    var font = new Font("微软雅黑", i, FontStyle.Regular);
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
        private MemoryStream ParseDOCXToPdf(string filePath)
        {
            var htmlText = "";
            try
            {
                htmlText = WordHelper.ParseDOCX(filePath);
            }
            catch (OpenXmlPackageException e)
            {
                if (e.ToString().Contains("Invalid Hyperlink"))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        UriFixer.FixInvalidUri(fs, brokenUri => WordHelper.FixUri(brokenUri));
                    }
                    htmlText = WordHelper.ParseDOCX(filePath);
                }
            }
            var converter = new BasicConverter(new PdfTools());
            //var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = DinkToPdf.ColorMode.Color,
                Orientation = DinkToPdf.Orientation.Portrait,
                PaperSize = PaperKind.A4,
                //Out="test2.pdf"
            },
                Objects = {
                   new ObjectSettings() {
                                         PagesCount = true,
                                         HtmlContent = htmlText,
                                         WebSettings = { DefaultEncoding = "utf-8" },
                                         //HeaderSettings = { FontSize = 9, Right = "页 [page] 总 [toPage]", Line = true },
                                         FooterSettings = { FontSize = 9, Right = "页 [page] 总 [toPage]" }
                                        }
                            }
            };
            var bytes = converter.Convert(doc);
            return new MemoryStream(bytes);
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

            pdfViewer1.Renderer.ContextMenuStrip = pdfViewerContextMenu;
            pdfViewer1.Renderer.DisplayRectangleChanged += Renderer_DisplayRectangleChanged;
            pdfViewer1.Renderer.ZoomChanged += Renderer_ZoomChanged;
            pdfViewer1.Renderer.BoundedTextHandler += Renderer_BoundedTextHandler;
            _zoom.Text = pdfViewer1.Renderer.Zoom.ToString();
            Disposed += (s, ea) =>
            {
                pictureBox1.Image?.Dispose();
                pdfViewer1.Document?.Dispose(true);
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
                pictureBox1.Location = new System.Drawing.Point(0, 0);
            };
        }

        #region 图片缩放
        double zoomfactor = 1.0;
        int zoompos_x = 0;
        int zoompos_y = 0;
        bool Mousedown = false;
        System.Drawing.Point Mousepos = new System.Drawing.Point(0, 0);
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
                pictureBox1.Location = new System.Drawing.Point(pictureBox1.Left + dx, pictureBox1.Top + dy);

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
                pictureBox1.Location = new System.Drawing.Point((int)(screensize_x / 2 - (width * zoomfactor / 2) + zoompos_x * zoomfactor), (int)(screensize_y / 2 - (height * zoomfactor / 2) + zoompos_y * zoomfactor));
            }
            else
            {
                pictureBox1.Width = width;
                pictureBox1.Height = height;
                pictureBox1.Location = new System.Drawing.Point(0, 0);
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

                    var pdfrect = new PdfRectangle(page, new RectangleF(Math.Min(x, x1), Math.Min(y, y1), Math.Abs(x1 - x), Math.Abs(y1 - y)));
                    var rect = pdfViewer1.Renderer.BoundsFromPdf(pdfrect);
                    g.DrawRectangle(pen1, rect);

                    var showOcr = pdfViewer1.Renderer.GetBounds(page).Width;
                    var backSize = ConvertToRenderSize(pdfViewer1.Document.Pages[page].Width * 4 / 3, pdfViewer1.Document.Pages[page].Height * 4 / 3, 96, 96, FpdfRotation.Rotate0, RenderFlags.Annotations | RenderFlags.CorrectFromDpi);
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
            try
            {
                this.ShowLoading();
                OpenFile();
            }
            finally { this.CloseLoading(); }

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
                    if (pdfViewer1.Document.Pages[page].GetPdfEditable())
                    {
                        string text = pdfViewer1.Document.Pages[page].GetPdfText();

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
                var rotate = (int)document.Pages[page].GetRotation();
                if (rotate < 3)
                    rotate++;
                else
                    rotate = 0;
                pdfViewer1.Document = null;
                document.RotatePage(page, (FpdfRotation)rotate);
                var memoryStream = new MemoryStream();
                document.Save(memoryStream);
                pdfViewer1.Document = PdfDocumentGdi.Load(this, memoryStream);
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
                    List<string> files = new List<string>();
                    using (var form = new OpenFileDialog())
                    {
                        form.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                        form.RestoreDirectory = true;
                        form.Title = "Open PDF File";
                        form.Multiselect = true;
                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            Dispose();
                            return;
                        }
                        files.AddRange(form.FileNames);
                    }
                    var document = pdfViewer1.Document;
                    var pdfdoc = new List<PdfDocument>();
                    pdfdoc.AddRange(files.ConvertAll(file => OpenDocument(file)));
                    document.MergePDF(pdfdoc.ToArray());
                    {
                        pdfViewer1.Document = null;
                        pdfViewer1.Document = document;
                    }
                    AppendTextToRich($"合并pdf已完成{files.Count()}");
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
                            using (var doc = document.GetPDFPage(i))
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
                Information info = pdfViewer1.Document.GetInformation();
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
                        using (var image = document.Pages[i].Render((int)document.Pages[i].Width * 4 / 3, (int)document.Pages[i].Height * 4 / 3, dpiX, dpiY, FpdfRotation.Rotate0, RenderFlags.Annotations | RenderFlags.CorrectFromDpi))
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
                    var doc = pdfViewer1.Document;
                    for (int i = 0; i < doc.PageCount; i++)
                    {
                        var image = pdfViewer1.Document.Pages[i].RenderThumbnail();
                        image?.Save(Path.Combine(path, "Page " + i + ".png"));
                        AppendTextToRich($"导出pdf图片成功总数{i} {path}");
                    }
                }
                finally { this.CloseLoading(); }
            }
        }

        private void 两页pdf合并成页ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var doc = (PdfDocument)pdfViewer1.Document;
                var page = doc.Pages[0];
                double width = page.Width;
                double height = page.Height;
                var docNew = doc.ImportNPagesToOne((float)width * 2, (float)height);
                pdfViewer1.Document = null;
                pdfViewer1.Document = docNew;
            }
        }

        private void 添加密码ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                //using (var form = new PasswordForm())
                //{
                //    if (form.ShowDialog(this) == DialogResult.OK)
                //    {
                //        var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                //        if (!Directory.Exists(path))
                //        {
                //            Directory.CreateDirectory(path);
                //        }
                //        var txt = form.Password;
                //        using (var stream = new MemoryStream())
                //        {
                //            pdfViewer1.Document.Save(stream);
                //            stream.Position = 0;
                //            // Open an existing document. Providing an unrequired password is ignored.
                //            var document = PdfReader.Open(stream);

                //            var securitySettings = document.SecuritySettings;

                //            // Setting one of the passwords automatically sets the security level to 
                //            // PdfDocumentSecurityLevel.Encrypted128Bit.
                //            securitySettings.UserPassword = txt;
                //            //securitySettings.OwnerPassword = "owner";

                //            // Don't use 40 bit encryption unless needed for compatibility.
                //            //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

                //            // Restrict some rights.
                //            securitySettings.PermitAccessibilityExtractContent = false;
                //            securitySettings.PermitAnnotations = false;
                //            securitySettings.PermitAssembleDocument = false;
                //            securitySettings.PermitExtractContent = false;
                //            securitySettings.PermitFormsFill = true;
                //            securitySettings.PermitFullQualityPrint = false;
                //            securitySettings.PermitModifyDocument = true;
                //            securitySettings.PermitPrint = false;

                //            // Save the document...
                //            document.Save(Path.Combine(path, Path.GetFileName(fileName)));
                //        }
                //        AppendTextToRich($"添加密码成功{path}");
                //    }
                //}
            }

        }

        private void 去除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (fileType == FileType.PDF)
            //{
            //    using (var form = new PasswordForm())
            //    {
            //        if (form.ShowDialog(this) == DialogResult.OK)
            //        {
            //            var path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            //            if (!Directory.Exists(path))
            //            {
            //                Directory.CreateDirectory(path);
            //            }
            //            var txt = form.Password;
            //            using (var stream = new MemoryStream())
            //            {
            //                pdfViewer1.Document.Save(stream);
            //                stream.Position = 0;
            //                // Open an existing document. Providing an unrequired password is ignored.
            //                var document = PdfReader.Open(stream, txt, PdfDocumentOpenMode.Modify);
            //                document.SecuritySettings.DocumentSecurityLevel = PdfSharp.Pdf.Security.PdfDocumentSecurityLevel.None;
            //                document.Save(Path.Combine(path, Path.GetFileName(fileName)));
            //            }
            //            AppendTextToRich($"去除密码成功{path}");
            //        }
            //    }
            //}
        }

        private void 添加水印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileType == FileType.PDF)
            {
                var watermark = "";
                var fontsize = 50;
                FpdfTextRenderMode waterIndex = 0;
                var color = Color.Red;
                var water = new WatermarkForm();
                if (water.ShowDialog(this) == DialogResult.OK)
                {
                    watermark = water.WaterMark;
                    fontsize = water.FontSize;
                    waterIndex = (FpdfTextRenderMode)water.WaterIndex;
                    color = water.WaterColor;
                }
                else
                {
                    return;
                }
                var doc = (PdfDocument)pdfViewer1.Document;
                doc.WaterMark(watermark, fontsize, color, totleHeight: 120, render_mode: waterIndex, strokeColor: color, strokeWidth: 0.1f);
                pdfViewer1.Document = null;
                pdfViewer1.Document = doc;
            }
        }
    }

    enum FileType
    {
        PDF,
        Image,
        Docx,
        Doc
    }
}
