using System.Drawing;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Controls
{
    internal class SplitContainerWithHandles : SplitContainer
    {
        public SplitContainerWithHandles()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Calcola il rettangolo dello splitter
            Rectangle gripRect;
            if (Orientation == Orientation.Vertical)
            {
                gripRect = new Rectangle(
                    SplitterDistance,
                    0,
                    SplitterWidth,
                    Height
                );
            }
            else
            {
                gripRect = new Rectangle(
                    0,
                    SplitterDistance,
                    Width,
                    SplitterWidth
                );
            }

            var brush = new SolidBrush(Color.Gray);

            // Disegna solo 3 puntini centrati
            if (Orientation == Orientation.Vertical)
            {
                int centerX = gripRect.X + gripRect.Width / 2;
                int centerY = gripRect.Y + gripRect.Height / 2;

                // Spaziatura verticale
                int spacing = 10;

                for (int i = -1; i <= 1; i++)
                {
                    int y = centerY + (i * spacing);
                    e.Graphics.FillEllipse(brush, centerX - 2, y - 2, 4, 4);
                }
            }
            else
            {
                int centerY = gripRect.Y + gripRect.Height / 2;
                int centerX = gripRect.X + gripRect.Width / 2;

                // Spaziatura orizzontale
                int spacing = 10;

                for (int i = -1; i <= 1; i++)
                {
                    int x = centerX + (i * spacing);
                    e.Graphics.FillEllipse(brush, x - 2, centerY - 2, 4, 4);
                }
            }

        }
    }
}
