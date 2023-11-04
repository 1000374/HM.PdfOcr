using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HM.PdfOcr.UCControl
{
    public class UCProcess : Control
    {
        private Color rollColor = Color.FromArgb(0, 122, 204);

        [Description("滚动的颜色"), Category("自定义")]
        public Color RollColor
        {
            get { return rollColor; }
            set { rollColor = value; }
        }

        [Description("是否滚动"), Category("自定义")]
        public bool Roll
        {
            get { return timer.Enabled; }
            set
            {
                timer.Enabled = value;
                if (!value)
                {
                    workRect = new Rectangle(-this.Width / 3, 0, this.Width / 3, this.Height);
                    Invalidate();
                }
            }
        }

        [Description("滚动间隔时间"), Category("自定义")]
        public int SplitTime
        {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

        Timer timer = new Timer();
        public UCProcess()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SizeChanged += UCProcessRoll_SizeChanged;
            this.Size = new Size(300, 3);
            this.BackColor = Color.White;
            timer.Interval = 30;
            timer.Tick += timer_Tick;
            timer.Enabled = true;
        }


        Rectangle workRect;

        void UCProcessRoll_SizeChanged(object sender, EventArgs e)
        {
            workRect = new Rectangle(-this.Width / 3, 0, this.Width / 3, this.Height);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;  //使绘图质量最高，即消除锯齿
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var r1 = new RectangleF(new Point(workRect.Left - 1, workRect.Top), new Size(workRect.Width / 3 + 1, workRect.Height));
            var r2 = new RectangleF(new Point(workRect.Right - workRect.Width / 3, workRect.Top), new Size(workRect.Width / 3, workRect.Height));
            LinearGradientBrush lgb1 = new LinearGradientBrush(r1, Color.FromArgb(0, rollColor), rollColor, 0f);
            LinearGradientBrush lgb2 = new LinearGradientBrush(r2, rollColor, Color.FromArgb(0, rollColor), 0f);
            g.FillRectangle(lgb1, new Rectangle(new Point(workRect.Left, workRect.Top), new Size(workRect.Width / 3, workRect.Height)));
            g.FillRectangle(new SolidBrush(rollColor), new RectangleF(workRect.Left + workRect.Width / 3 - 1, workRect.Top, workRect.Width / 3 + 3, workRect.Height));
            g.FillRectangle(lgb2, r2);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (workRect != null)
            {
                workRect = new Rectangle(workRect.Left + 10, 0, this.Width / 3, this.Height);

                if (workRect.Left >= this.ClientRectangle.Right)
                {
                    workRect = new Rectangle(-this.Width / 3, 0, this.Width / 3, this.Height);
                }
                Invalidate();
            }
        }
    }


    public partial class UCLoading : Control
    {
        Color beginColor = Color.White;
        Color endColor = Color.Black;
        int wid = 10;
        int curindex = 0;
        Timer timer;
        int instervel = 200;
        string loadStr=>this.Text;
       
        public UCLoading()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.MinimumSize = new Size(80, 40);
            if (!DesignMode)
            {
                Start();
            }
        }

        public void Start()
        {
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = instervel;
                timer.Tick += Timer_Tick;
            }
            timer.Enabled = true;
        }
        public void Stop()
        {
            if (timer != null)
            {
                timer.Enabled = false;
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            curindex++;
            curindex = curindex >= wid ? 0 : curindex;
            Refresh();
        }
        //计算各种圈圈相关
        Point getPoint(double d, double r, Point center)
        {
            int x = (int)(r * Math.Cos(d * Math.PI / 180.0));
            int y = (int)(r * Math.Sin(d * Math.PI / 180.0));
            return new Point(center.X + x, center.Y - y);
        }
        GraphicsPath getPath(Point a, Point b)
        {
            Point c, d, e, f;
            int h = 2;
            Vertical(a, b, h, out c, out d);
            Vertical(b, a, h, out e, out f);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new Point[] { c, d, e, f });
            path.CloseAllFigures();
            return path;


        }
        bool Vertical(Point pointa, Point pointb, double R, out Point pointc, out Point pointd)
        {
            pointc = new Point();
            pointd = new Point();
            try
            {
                //（X-xa)^2+(Y-ya)^2=R*R    距离公式
                //(X-xa)*(xb-xa)+(Y-ya)*(yb-ya)=0   垂直
                //解方程得两点即为所求点
                var cx = pointa.X - (pointb.Y - pointa.Y) * R / Distance(pointa, pointb);
                var cy = pointa.Y + (pointb.X - pointa.X) * R / Distance(pointa, pointb);

                var dx = pointa.X + (pointb.Y - pointa.Y) * R / Distance(pointa, pointb);
                var dy = pointa.Y - (pointb.X - pointa.X) * R / Distance(pointa, pointb);
                pointc = new Point((int)cx, (int)cy);
                pointd = new Point((int)dx, (int)dy);
                return true;
            }
            catch
            {
                //如果A,B两点重合会报错，那样就返回false
                return false;
            }
        }
        double Distance(double xa, double ya, double xb, double yb)
        {
            double L;
            L = Math.Sqrt(Math.Pow(xa - xb, 2) + Math.Pow(ya - yb, 2));
            return L;
        }
        double Distance(Point pa, Point pb)
        {
            return Distance(pa.X, pa.Y, pb.X, pb.Y);
        }
        GraphicsPath getPath(double d, double r, Point c)
        {
            var p1 = getPoint(d, r / 2.0, c);
            var p2 = getPoint(d, r, c);
            return getPath(p1, p2);
        }
        //算渐变色
        Color[] getColors()
        {
            int dr = (int)((endColor.R - beginColor.R) / (double)wid);
            int dg = (int)((endColor.G - beginColor.G) / (double)wid);
            int db = (int)((endColor.B - beginColor.B) / (double)wid);
            List<Color> colors = new List<Color>();
            for (int i = 0; i < wid; i++)
            {
                colors.Add(Color.FromArgb(beginColor.R + dr * i, beginColor.G + dg * i, beginColor.B + db * i));
            }
            return colors.ToArray();

        }

        //画圈圈
        void drawRect(Graphics g)
        {
            int r = (int)(Size.Width / 2.0);
            Point center = new Point(r, r);
            var colors = getColors();
            int findex = curindex;
            for (int i = 0; i < wid; i++)
            {
                double d = (360.0 / wid) * i;
                var p = getPath(d, r, center);
                int cindex = findex + i;
                cindex = cindex >= wid ? cindex - wid : cindex;
                g.FillPath(new SolidBrush(colors[cindex]), p);
            }
        }
        //画字符串
        void drawString(Graphics g)
        {
            if (Size.Width > Size.Height) return;
            if (string.IsNullOrEmpty(loadStr)) return;
            var size = g.MeasureString(loadStr, Font);
            RectangleF rect = new RectangleF(new Point(((int)(Size.Width - size.Width) / 2), Size.Width), size);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString(loadStr, Font, Brushes.Black, rect, sf);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            drawRect(g);
            drawString(g);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Size.Height < Size.Width)
            {
                Size = new Size(Size.Width, Size.Width);
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }
    }

}
