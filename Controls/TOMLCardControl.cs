using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Controls
{
    public enum FieldType
    {
        PlainText,
        Multiline
    }

    public class TOMLCardControl : Control
    {
        private class FieldEntry
        {
            public string Label;
            public string Value;
            public FieldType Type;

            public int ScrollOffset = 0;
            public int TextHeight = 0;
            public Rectangle Area;
            public Rectangle ThumbRect;
            public Rectangle LabelArea;
        }

        private readonly List<FieldEntry> fields = new List<FieldEntry>();

        public Guid OperationId { get; set; }

        private int cardWidth = 350;
        private int cardHeight = 350;

        public int CardWidth
        {
            get => cardWidth;
            set { cardWidth = value; Width = value; Invalidate(); }
        }

        public int CardHeight
        {
            get => cardHeight;
            set { cardHeight = value; Height = value; Invalidate(); }
        }

        public bool EnableAnimations { get; set; } = true;
        public int AnimationSpeedMs { get; set; } = 150;

        private const int PlainTextRowHeight = 28;
        private const int MultilineAreaHeight = 120;
        private const int MultilineAreaSpacing = 22;

        private Font contentFont;
        private Font labelFont;

        private Timer borderAnimationTimer;
        private float borderThickness = 3f;
        private float targetBorderThickness = 3f;
        private Color currentBorderColor = Color.SeaGreen;
        private Color targetBorderColor = Color.SeaGreen;

        public bool IsSelected { get; private set; } = true;
        private Rectangle checkboxRect;
        private int cornerRadius = 10;

        private bool draggingThumb = false;
        private FieldEntry draggingField = null;
        private int dragStartY = 0;
        private int dragStartOffset = 0;

        // Per copia da click
        private FieldEntry pendingCopyField = null;
        private Point mouseDownLocation;

        // Stato OK/KO
        private bool hasStatus = false;
        private bool isOk = false;
        private string errorMessage = null;
        private Color statusBackColor = Color.White;

        public TOMLCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.Selectable, true);

            BackColor = Color.White;
            Width = cardWidth;
            Height = cardHeight;

            contentFont = new Font("Segoe UI", 10f, FontStyle.Regular);
            labelFont = new Font("Segoe UI", 10f, FontStyle.Bold);

            borderAnimationTimer = new Timer { Interval = 16 };
            borderAnimationTimer.Tick += AnimateBorder;

            UpdateCheckboxRect();

            this.MouseDown += OnMouseDownHandler;
            this.MouseMove += OnMouseMoveHandler;
            this.MouseUp += OnMouseUpHandler;
        }

        public void SetOk()
        {
            hasStatus = true;
            isOk = true;
            errorMessage = null;
            statusBackColor = Color.FromArgb(200, 230, 200); 
            Invalidate();
        }

        public void SetKo(string message)
        {
            hasStatus = true;
            isOk = false;
            errorMessage = message;
            statusBackColor = Color.FromArgb(255, 200, 200); 
            Invalidate();
        }

        public void ResetStatus()
        {
            hasStatus = false;
            isOk = false;
            errorMessage = null;
            statusBackColor = Color.White;
            Invalidate();
        }

        private void UpdateCheckboxRect()
        {
            checkboxRect = new Rectangle(Width - 34, 8, 24, 24);
        }

        public void AddField(string label, string value, FieldType type)
        {
            var entry = new FieldEntry();
            entry.Label = label;
            entry.Value = value ?? "";
            entry.Type = type;

            fields.Add(entry);
            RecalculateTextHeights();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateCheckboxRect();
            RecalculateTextHeights();
        }

        private void RecalculateTextHeights()
        {
            using (Graphics g = CreateGraphics())
            {
                foreach (var f in fields)
                {
                    if (f.Type == FieldType.Multiline)
                    {
                        SizeF size = g.MeasureString(
                            f.Value,
                            contentFont,
                            new SizeF(Width - 40 - 12, float.MaxValue)
                        );
                        f.TextHeight = Math.Max((int)size.Height, 0);
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            foreach (var f in fields)
            {
                if (f.Type != FieldType.Multiline) continue;

                if (f.Area.Contains(e.Location))
                {
                    f.ScrollOffset -= e.Delta / 2;
                    int maxScroll = Math.Max(0, f.TextHeight - f.Area.Height);
                    f.ScrollOffset = Math.Max(0, Math.Min(f.ScrollOffset, maxScroll));
                    Invalidate();
                    return;
                }
            }
        }

        private void OnMouseDownHandler(object sender, MouseEventArgs e)
        {
            pendingCopyField = null;
            mouseDownLocation = e.Location;

            Rectangle expandedCheckbox = checkboxRect;
            expandedCheckbox.Inflate(4, 4);

            if (expandedCheckbox.Contains(e.Location))
            {
                IsSelected = !IsSelected;
                StartBorderAnimation();
                Invalidate();
                return;
            }

            var hit = HitTestField(e.Location);

            if (hit.type == HitType.Thumb && hit.field != null)
            {
                draggingThumb = true;
                draggingField = hit.field;
                dragStartY = e.Y;
                dragStartOffset = hit.field.ScrollOffset;
                this.Capture = true;
                return;
            }

            if (hit.field != null && (hit.type == HitType.Label || hit.type == HitType.PlainValue || hit.type == HitType.MultilineArea))
            {
                pendingCopyField = hit.field;
                return;
            }

            this.Focus();
        }

        private void OnMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (draggingThumb && draggingField != null)
            {
                var f = draggingField;
                int trackH = f.Area.Height;
                int thumbH = f.ThumbRect.Height;
                int trackScrollable = Math.Max(1, trackH - thumbH);

                int dy = e.Y - dragStartY;
                float scrollable = Math.Max(0, f.TextHeight - f.Area.Height);

                if (trackScrollable > 0)
                {
                    float ratio = scrollable / (float)trackScrollable;
                    int newOffset = dragStartOffset + (int)(dy * ratio);
                    newOffset = Math.Max(0, Math.Min(newOffset, (int)scrollable));
                    f.ScrollOffset = newOffset;
                    Invalidate();
                }
            }

            if (pendingCopyField != null)
            {
                int dx = Math.Abs(e.X - mouseDownLocation.X);
                int dy = Math.Abs(e.Y - mouseDownLocation.Y);
                if (dx > 5 || dy > 5)
                {
                    pendingCopyField = null;
                }
            }
        }

        private void OnMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (this.Capture) this.Capture = false;

            if (draggingThumb)
            {
                draggingThumb = false;
                draggingField = null;
                pendingCopyField = null;
                return;
            }

            if (pendingCopyField != null)
            {
                var hit = HitTestField(e.Location);

                if (hit.field == pendingCopyField)
                {
                    CopyToClipboard(pendingCopyField.Value);
                }

                pendingCopyField = null;
                return;
            }
        }

        private void CopyToClipboard(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                    Clipboard.SetText(text);
            }
            catch { }
        }

        private void StartBorderAnimation()
        {
            if (!EnableAnimations)
            {
                currentBorderColor = IsSelected ? Color.SeaGreen : Color.LightGray;
                borderThickness = IsSelected ? 3f : 1f;
                Invalidate();
                return;
            }

            targetBorderColor = IsSelected ? Color.SeaGreen : Color.LightGray;
            targetBorderThickness = IsSelected ? 3f : 1f;
            borderAnimationTimer.Start();
        }

        private void AnimateBorder(object sender, EventArgs e)
        {
            float step = 0.2f;
            bool done = true;

            currentBorderColor = LerpColor(currentBorderColor, targetBorderColor, step);
            if (!ColorsClose(currentBorderColor, targetBorderColor))
                done = false;

            borderThickness = Lerp(borderThickness, targetBorderThickness, step);
            if (Math.Abs(borderThickness - targetBorderThickness) > 0.1f)
                done = false;

            if (done)
            {
                currentBorderColor = targetBorderColor;
                borderThickness = targetBorderThickness;
                borderAnimationTimer.Stop();
            }

            Invalidate();
        }

        private static bool ColorsClose(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) < 5 &&
                   Math.Abs(a.G - b.G) < 5 &&
                   Math.Abs(a.B - b.B) < 5;
        }

        private static Color LerpColor(Color from, Color to, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (int)(from.A + (to.A - from.A) * t),
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t)
            );
        }

        private static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RectangleF rect = new RectangleF(1f, 1f, Width - 2f, Height - 2f);

            Color bgColor = hasStatus ? statusBackColor : BackColor;

            using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
            {
                using (SolidBrush bg = new SolidBrush(bgColor))
                    g.FillPath(bg, path);

                using (Pen borderPen = new Pen(currentBorderColor, borderThickness))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPath(borderPen, path);
                }
            }

            DrawCheckbox(g);

            int y = 44;

            if (hasStatus && !isOk && !string.IsNullOrEmpty(errorMessage))
            {
                using (Font errorFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (SolidBrush errorBrush = new SolidBrush(Color.DarkRed))
                {
                    Rectangle errorRect = new Rectangle(10, y, Width - 20, 40);
                    g.DrawString(errorMessage, errorFont, errorBrush, errorRect);
                    y += 45;
                }
            }

            foreach (var f in fields)
            {
                SizeF labelSize = g.MeasureString(f.Label + ":", labelFont);
                f.LabelArea = new Rectangle(10, y, (int)labelSize.Width, (int)labelSize.Height);

                using (Brush labelBrush = new SolidBrush(Color.Black))
                    g.DrawString(f.Label + ":", labelFont, labelBrush, new PointF(10, y));

                if (f.Type == FieldType.PlainText)
                {
                    Rectangle valueRect = new Rectangle(130, y, Width - 140, PlainTextRowHeight);
                    f.Area = valueRect;

                    using (SolidBrush valueBrush = new SolidBrush(Color.DarkSlateGray))
                        g.DrawString(f.Value, contentFont, valueBrush, new RectangleF(valueRect.X, valueRect.Y, valueRect.Width, valueRect.Height));

                    y += PlainTextRowHeight;
                }
                else
                {
                    Rectangle rectArea = new Rectangle(10, y + MultilineAreaSpacing, Width - 30 - 12, MultilineAreaHeight);
                    f.Area = rectArea;

                    Region oldClip = g.Clip;
                    g.SetClip(rectArea);

                    using (SolidBrush textBrush = new SolidBrush(Color.DarkSlateGray))
                    {
                        g.DrawString(
                            f.Value,
                            contentFont,
                            textBrush,
                            new RectangleF(
                                rectArea.X,
                                rectArea.Y - f.ScrollOffset,
                                rectArea.Width,
                                Math.Max(f.TextHeight, rectArea.Height)
                            )
                        );
                    }

                    g.SetClip(oldClip, CombineMode.Replace);

                    using (Pen p = new Pen(Color.LightGray))
                        g.DrawRectangle(p, rectArea);

                    if (f.TextHeight > rectArea.Height)
                    {
                        int trackX = rectArea.Right + 4;
                        int trackY = rectArea.Y;
                        int trackW = 8;
                        int trackH = rectArea.Height;

                        using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                            g.FillRectangle(trackBrush, trackX, trackY, trackW, trackH);

                        float ratio = (float)rectArea.Height / f.TextHeight;
                        int thumbH = Math.Max(20, (int)(trackH * ratio));

                        float scrollable = Math.Max(1, f.TextHeight - rectArea.Height);
                        float scrollRatio = (float)f.ScrollOffset / scrollable;
                        int thumbY = trackY + (int)((trackH - thumbH) * scrollRatio);

                        Rectangle thumb = new Rectangle(trackX, thumbY, trackW, thumbH);
                        f.ThumbRect = thumb;

                        using (SolidBrush thumbBrush = new SolidBrush(Color.Gray))
                            g.FillRectangle(thumbBrush, thumb);

                        using (Pen pen = new Pen(Color.FromArgb(200, Color.Black), 1f))
                            g.DrawRectangle(pen, thumb);
                    }
                    else
                    {
                        f.ThumbRect = Rectangle.Empty;
                    }

                    y += MultilineAreaHeight + MultilineAreaSpacing + 10;
                }
            }
        }

        private void DrawCheckbox(Graphics g)
        {
            using (SolidBrush boxBg = new SolidBrush(Color.White))
                g.FillRectangle(boxBg, checkboxRect);

            using (Pen p = new Pen(Color.Gray, 1.5f))
                g.DrawRectangle(p, checkboxRect);

            if (IsSelected)
            {
                using (Pen p = new Pen(Color.SeaGreen, 2.5f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;

                    int x = checkboxRect.X;
                    int y = checkboxRect.Y;
                    int w = checkboxRect.Width;
                    int h = checkboxRect.Height;

                    g.DrawLine(p, x + 5, y + h / 2, x + w / 3 + 2, y + h - 6);
                    g.DrawLine(p, x + w / 3 + 2, y + h - 6, x + w - 5, y + 6);
                }
            }
        }

        private GraphicsPath CreateRoundedRectanglePath(RectangleF rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2f;

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddLine(rect.X + radius, rect.Y, rect.Right - radius, rect.Y);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom - radius);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddLine(rect.Right - radius, rect.Bottom, rect.X + radius, rect.Bottom);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.AddLine(rect.X, rect.Bottom - radius, rect.X, rect.Y + radius);

            path.CloseFigure();
            return path;
        }

        private enum HitType { None, Thumb, MultilineArea, PlainValue, Label }

        private (HitType type, FieldEntry field) HitTestField(Point p)
        {
            foreach (var f in fields)
            {
                if (f.ThumbRect != Rectangle.Empty && f.ThumbRect.Contains(p))
                    return (HitType.Thumb, f);

                // Poi la label
                if (f.LabelArea.Contains(p))
                    return (HitType.Label, f);

                if (f.Area.Contains(p))
                {
                    return f.Type == FieldType.PlainText
                        ? (HitType.PlainValue, f)
                        : (HitType.MultilineArea, f);
                }
            }

            return (HitType.None, null);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                contentFont?.Dispose();
                labelFont?.Dispose();
                borderAnimationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}