using System.Drawing;
using System.Windows.Forms;

namespace WhyIDidntKnowThisGameEarlier
{
    public class SpecialControl : Control
    {
        public SpecialControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle rect = new Rectangle(new Point(), new Size(Width - 1, Height - 1));

            e.Graphics.DrawRectangle(Pens.Black, rect);
        }
    }
}
