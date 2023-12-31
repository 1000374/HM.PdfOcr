﻿namespace HM.PdfOcr
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._openFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this._page = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this._zoom = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._fitBest = new System.Windows.Forms.ToolStripButton();
            this._matching = new System.Windows.Forms.ToolStripButton();
            this._getTextFromPage = new System.Windows.Forms.ToolStripButton();
            this._copy = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.转换双层pdfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.直接生成pdfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pdf操作ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.合并ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拆分ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.转成图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.页合并成1页ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导出内嵌图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.添加水印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.密码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.添加密码ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.去除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pdf信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.pdfViewerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.取消选中ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.旋转ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pdfViewer1 = new PdfiumViewer.PdfViewer();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.pdfViewerContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openFile,
            this.toolStripLabel1,
            this._page,
            this.toolStripSeparator1,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this._zoom,
            this.toolStripSeparator7,
            this.toolStripButton4,
            this.toolStripButton3,
            this.toolStripSeparator3,
            this._fitBest,
            this._matching,
            this._getTextFromPage,
            this._copy,
            this.toolStripButton7,
            this.toolStripButton6,
            this.toolStripDropDownButton1,
            this.toolStripButton5});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1059, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _openFile
            // 
            this._openFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._openFile.Image = ((System.Drawing.Image)(resources.GetObject("_openFile.Image")));
            this._openFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._openFile.Name = "_openFile";
            this._openFile.Size = new System.Drawing.Size(60, 22);
            this._openFile.Text = "打开文件";
            this._openFile.Click += new System.EventHandler(this._openFile_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(35, 22);
            this.toolStripLabel1.Text = "页码:";
            // 
            // _page
            // 
            this._page.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this._page.Name = "_page";
            this._page.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "<";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = ">";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(35, 22);
            this.toolStripLabel2.Text = "缩放:";
            // 
            // _zoom
            // 
            this._zoom.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this._zoom.Name = "_zoom";
            this._zoom.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "+";
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "-";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // _fitBest
            // 
            this._fitBest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._fitBest.Image = ((System.Drawing.Image)(resources.GetObject("_fitBest.Image")));
            this._fitBest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitBest.Name = "_fitBest";
            this._fitBest.Size = new System.Drawing.Size(48, 22);
            this._fitBest.Text = "自适应";
            this._fitBest.Click += new System.EventHandler(this._fitBest_Click);
            // 
            // _matching
            // 
            this._matching.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._matching.Image = ((System.Drawing.Image)(resources.GetObject("_matching.Image")));
            this._matching.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._matching.Name = "_matching";
            this._matching.Size = new System.Drawing.Size(60, 22);
            this._matching.Text = "框选文本";
            this._matching.Click += new System.EventHandler(this._matching_Click);
            // 
            // _getTextFromPage
            // 
            this._getTextFromPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._getTextFromPage.Image = ((System.Drawing.Image)(resources.GetObject("_getTextFromPage.Image")));
            this._getTextFromPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._getTextFromPage.Name = "_getTextFromPage";
            this._getTextFromPage.Size = new System.Drawing.Size(60, 22);
            this._getTextFromPage.Text = "提取文本";
            this._getTextFromPage.Click += new System.EventHandler(this._getTextFromPage_Click);
            // 
            // _copy
            // 
            this._copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._copy.Name = "_copy";
            this._copy.Size = new System.Drawing.Size(36, 22);
            this._copy.Text = "复制";
            this._copy.Click += new System.EventHandler(this._copy_Click);
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton7.Text = "书签";
            this.toolStripButton7.Click += new System.EventHandler(this.toolStripButton7_Click);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton6.Text = "结果栏";
            this.toolStripButton6.Click += new System.EventHandler(this.toolStripButton6_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.转换双层pdfToolStripMenuItem,
            this.直接生成pdfToolStripMenuItem,
            this.pdf操作ToolStripMenuItem,
            this.密码ToolStripMenuItem,
            this.pdf信息ToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton1.Text = "工具";
            // 
            // 转换双层pdfToolStripMenuItem
            // 
            this.转换双层pdfToolStripMenuItem.Name = "转换双层pdfToolStripMenuItem";
            this.转换双层pdfToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.转换双层pdfToolStripMenuItem.Text = "转换双层pdf";
            this.转换双层pdfToolStripMenuItem.Click += new System.EventHandler(this.转换双层pdfToolStripMenuItem_Click);
            // 
            // 直接生成pdfToolStripMenuItem
            // 
            this.直接生成pdfToolStripMenuItem.Name = "直接生成pdfToolStripMenuItem";
            this.直接生成pdfToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.直接生成pdfToolStripMenuItem.Text = "直接生成pdf";
            this.直接生成pdfToolStripMenuItem.Click += new System.EventHandler(this.直接生成pdfToolStripMenuItem_Click);
            // 
            // pdf操作ToolStripMenuItem
            // 
            this.pdf操作ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.合并ToolStripMenuItem,
            this.拆分ToolStripMenuItem,
            this.转成图片ToolStripMenuItem,
            this.页合并成1页ToolStripMenuItem,
            this.导出内嵌图片ToolStripMenuItem,
            this.添加水印ToolStripMenuItem});
            this.pdf操作ToolStripMenuItem.Name = "pdf操作ToolStripMenuItem";
            this.pdf操作ToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pdf操作ToolStripMenuItem.Text = "pdf操作";
            // 
            // 合并ToolStripMenuItem
            // 
            this.合并ToolStripMenuItem.Name = "合并ToolStripMenuItem";
            this.合并ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.合并ToolStripMenuItem.Text = "合并";
            this.合并ToolStripMenuItem.Click += new System.EventHandler(this.合并PDFToolStripMenuItem_Click);
            // 
            // 拆分ToolStripMenuItem
            // 
            this.拆分ToolStripMenuItem.Name = "拆分ToolStripMenuItem";
            this.拆分ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.拆分ToolStripMenuItem.Text = "拆分";
            this.拆分ToolStripMenuItem.Click += new System.EventHandler(this.拆分PDFToolStripMenuItem_Click);
            // 
            // 转成图片ToolStripMenuItem
            // 
            this.转成图片ToolStripMenuItem.Name = "转成图片ToolStripMenuItem";
            this.转成图片ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.转成图片ToolStripMenuItem.Text = "转成图片";
            this.转成图片ToolStripMenuItem.Click += new System.EventHandler(this.pdf转成图片ToolStripMenuItem_Click);
            // 
            // 页合并成1页ToolStripMenuItem
            // 
            this.页合并成1页ToolStripMenuItem.Name = "页合并成1页ToolStripMenuItem";
            this.页合并成1页ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.页合并成1页ToolStripMenuItem.Text = "2页合并成1页";
            this.页合并成1页ToolStripMenuItem.Click += new System.EventHandler(this.两页pdf合并成页ToolStripMenuItem_Click);
            // 
            // 导出内嵌图片ToolStripMenuItem
            // 
            this.导出内嵌图片ToolStripMenuItem.Name = "导出内嵌图片ToolStripMenuItem";
            this.导出内嵌图片ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.导出内嵌图片ToolStripMenuItem.Text = "导出内嵌图片";
            this.导出内嵌图片ToolStripMenuItem.Click += new System.EventHandler(this.导出pdf内资源图片ToolStripMenuItem_Click);
            // 
            // 添加水印ToolStripMenuItem
            // 
            this.添加水印ToolStripMenuItem.Name = "添加水印ToolStripMenuItem";
            this.添加水印ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.添加水印ToolStripMenuItem.Text = "添加水印";
            this.添加水印ToolStripMenuItem.Click += new System.EventHandler(this.添加水印ToolStripMenuItem_Click);
            // 
            // 密码ToolStripMenuItem
            // 
            this.密码ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.添加密码ToolStripMenuItem1,
            this.去除ToolStripMenuItem});
            this.密码ToolStripMenuItem.Name = "密码ToolStripMenuItem";
            this.密码ToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.密码ToolStripMenuItem.Text = "密码";
            // 
            // 添加密码ToolStripMenuItem1
            // 
            this.添加密码ToolStripMenuItem1.Name = "添加密码ToolStripMenuItem1";
            this.添加密码ToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.添加密码ToolStripMenuItem1.Text = "添加密码";
            this.添加密码ToolStripMenuItem1.Click += new System.EventHandler(this.添加密码ToolStripMenuItem1_Click);
            // 
            // 去除ToolStripMenuItem
            // 
            this.去除ToolStripMenuItem.Name = "去除ToolStripMenuItem";
            this.去除ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.去除ToolStripMenuItem.Text = "去除密码";
            this.去除ToolStripMenuItem.Click += new System.EventHandler(this.去除ToolStripMenuItem_Click);
            // 
            // pdf信息ToolStripMenuItem
            // 
            this.pdf信息ToolStripMenuItem.Name = "pdf信息ToolStripMenuItem";
            this.pdf信息ToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pdf信息ToolStripMenuItem.Text = "pdf信息";
            this.pdf信息ToolStripMenuItem.Click += new System.EventHandler(this.pdf信息ToolStripMenuItem_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton5.Text = "关于";
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // pdfViewerContextMenu
            // 
            this.pdfViewerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.取消选中ToolStripMenuItem,
            this.旋转ToolStripMenuItem,
            this.删除ToolStripMenuItem});
            this.pdfViewerContextMenu.Name = "pdfViewerContextMenu";
            this.pdfViewerContextMenu.Size = new System.Drawing.Size(181, 136);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.copyToolStripMenuItem.Text = "复制";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.selectAllToolStripMenuItem.Text = "选中所有";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // 取消选中ToolStripMenuItem
            // 
            this.取消选中ToolStripMenuItem.Name = "取消选中ToolStripMenuItem";
            this.取消选中ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.取消选中ToolStripMenuItem.Text = "取消选中";
            this.取消选中ToolStripMenuItem.Click += new System.EventHandler(this.取消选中ToolStripMenuItem_Click);
            // 
            // 旋转ToolStripMenuItem
            // 
            this.旋转ToolStripMenuItem.Name = "旋转ToolStripMenuItem";
            this.旋转ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.旋转ToolStripMenuItem.Text = "旋转";
            this.旋转ToolStripMenuItem.Click += new System.EventHandler(this.旋转ToolStripMenuItem_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(298, 689);
            this.richTextBox1.TabIndex = 10;
            this.richTextBox1.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel1.Controls.Add(this.pdfViewer1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1059, 689);
            this.splitContainer1.SplitterDistance = 757;
            this.splitContainer1.TabIndex = 11;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(757, 689);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // pdfViewer1
            // 
            this.pdfViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfViewer1.Location = new System.Drawing.Point(0, 0);
            this.pdfViewer1.Name = "pdfViewer1";
            this.pdfViewer1.Size = new System.Drawing.Size(757, 689);
            this.pdfViewer1.TabIndex = 8;
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 714);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PdfOcr";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.pdfViewerContextMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private PdfiumViewer.PdfViewer pdfViewer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox _page;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox _zoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ContextMenuStrip pdfViewerContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton _matching;
        private System.Windows.Forms.ToolStripButton _getTextFromPage;
        private System.Windows.Forms.ToolStripButton _openFile;
        private System.Windows.Forms.ToolStripButton _fitBest;
        private System.Windows.Forms.ToolStripButton _copy;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem 转换双层pdfToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem 直接生成pdfToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripMenuItem 取消选中ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 旋转ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pdf信息ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 密码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 添加密码ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 去除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pdf操作ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 合并ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 拆分ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 转成图片ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 页合并成1页ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导出内嵌图片ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 添加水印ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
    }
}

