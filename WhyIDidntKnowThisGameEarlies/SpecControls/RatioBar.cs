using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhyIDidntKnowThisGameEarlies
{
    public class RatioBar : Control
    {
        private float firstPart;
        private float secondPart;
        private float step;
        private readonly SolidBrush firstPartBrush;
        private readonly SolidBrush secondPartBrush;
        private readonly StringFormat SF;

        public RatioBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            step = Width / 100;
            FirstColor = Color.LightBlue;
            SecondColor = Color.IndianRed;
            firstPartBrush = new SolidBrush(FirstColor);
            secondPartBrush = new SolidBrush(SecondColor);
            FirstPart = 50;

            SF = new StringFormat();
            SF.Alignment = StringAlignment.Center;
            SF.FormatFlags = StringFormatFlags.NoWrap;
            SF.Trimming = StringTrimming.EllipsisCharacter;
        }

        public float FirstPart
        {
            get { return firstPart; }
            set
            {
                if (value > 100)
                    value = 100;

                if (value < 0)
                    value = 0;

                firstPart = value;
                secondPart = 100 - firstPart;
            }
        }

        public float SecondPart
        {
            get { return firstPart; }
            set
            {
                if (value > 100)
                    value = 100;

                if (value < 0)
                    value = 0;

                secondPart = value;
                firstPart = 100 - secondPart;
            }
        }

        public Color FirstColor { get; set; }

        public Color SecondColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Size s = new Size(Width - 1, Height - 1);
            Point p = new Point(0, 0);
            Rectangle outline = new Rectangle(p, s);

            g.DrawRectangle(Pens.Black, outline);

            RectangleF firstRect = new RectangleF(new PointF(1, 1), new SizeF(firstPart * step, Height - 2));
            RectangleF secondRect = new RectangleF(new PointF(firstPart * step, 1), 
                new SizeF(secondPart * step, Height - 2));

            g.FillRectangle(firstPartBrush, firstRect);
            g.FillRectangle(secondPartBrush, secondRect);

            g.DrawRectangle(Pens.Black, firstPart * step - 2, 0, 4, Height);

            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                g.DrawString(firstPart.ToString(), Font, brush, firstRect, SF);
                g.DrawString(secondPart.ToString(), Font, brush, secondRect, SF);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            step = Width / 100.0f;
            base.OnResize(e);
        }
    }
}
