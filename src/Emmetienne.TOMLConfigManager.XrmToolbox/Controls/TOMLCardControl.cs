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
            public int TextWidth = 0;
            public Rectangle Area;
            public Rectangle ThumbRect;
            public Rectangle LabelArea;

            // Horizontal scroll for plain text
            public float HorizontalScrollOffset = 0;
            public bool IsHovered = false;
            public bool NeedsHorizontalScroll = false;
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
        private const int ScrollBarWidth = 12;
        private const int ContentPadding = 10;
        private const int CardTopPadding = 44;
        private const int CardBottomPadding = 10;
        private const int ErrorMessageMaxHeight = 36; // ~2 lines of text
        private const int ErrorFadeHeight = 12;

        private Font contentFont;
        private Font labelFont;
        private Font errorFont;

        private Timer borderAnimationTimer;
        private Timer horizontalScrollTimer;
        private Timer errorScrollTimer;
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

        private FieldEntry pendingCopyField = null;
        private Point mouseDownLocation;

        private bool hasStatus = false;
        private bool isOk = false;
        private string errorMessage = null;
        private Color statusBackColor = Color.White;

        private bool showCheckbox = true;

        private FieldEntry currentHoveredField = null;
        private const float HorizontalScrollSpeed = 1.5f;
        private const int HorizontalScrollPauseAtEnd = 30; // frames to pause at each end
        private int horizontalScrollPauseCounter = 0;
        private bool horizontalScrollForward = true;

        // Error message scrolling
        private Rectangle errorMessageArea;
        private int errorMessageTextHeight = 0;
        private float errorMessageScrollOffset = 0;
        private bool errorMessageIsHovered = false;
        private bool errorMessageNeedsScroll = false;
        private const float ErrorScrollSpeed = 1.0f;
        private const int ErrorScrollPauseAtEnd = 40;
        private int errorScrollPauseCounter = 0;
        private bool errorScrollForward = true;

        // Error message click handling
        private bool pendingErrorCopy = false;

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
            errorFont = new Font("Segoe UI", 9f, FontStyle.Bold);

            borderAnimationTimer = new Timer { Interval = 16 };
            borderAnimationTimer.Tick += AnimateBorder;

            horizontalScrollTimer = new Timer { Interval = 30 };
            horizontalScrollTimer.Tick += AnimateHorizontalScroll;

            errorScrollTimer = new Timer { Interval = 30 };
            errorScrollTimer.Tick += AnimateErrorScroll;

            UpdateCheckboxRect();

            this.MouseDown += OnMouseDownHandler;
            this.MouseMove += OnMouseMoveHandler;
            this.MouseUp += OnMouseUpHandler;
            this.MouseLeave += OnMouseLeaveHandler;
        }

        public void SetOk()
        {
            hasStatus = true;
            isOk = true;
            errorMessage = null;
            statusBackColor = Color.FromArgb(200, 230, 200);

            IsSelected = false;
            showCheckbox = false;
            currentBorderColor = Color.LightGray;
            targetBorderColor = Color.LightGray;
            borderThickness = 1f;
            targetBorderThickness = 1f;

            ResetErrorScroll();
            Invalidate();
        }

        public void SetKo(string message)
        {
            hasStatus = true;
            isOk = false;
            errorMessage = message;
            statusBackColor = Color.FromArgb(255, 200, 200);

            IsSelected = false;
            showCheckbox = true;

            currentBorderColor = Color.DarkRed;
            targetBorderColor = Color.DarkRed;
            borderThickness = 2f;
            targetBorderThickness = 2f;

            RecalculateErrorMessageHeight();
            Invalidate();
        }

        public void ResetStatus()
        {
            hasStatus = false;
            isOk = false;
            errorMessage = null;
            statusBackColor = Color.White;
            showCheckbox = true;
            ResetErrorScroll();
            Invalidate();
        }

        private void ResetErrorScroll()
        {
            errorMessageScrollOffset = 0;
            errorMessageIsHovered = false;
            errorScrollTimer.Stop();
            errorScrollForward = true;
            errorScrollPauseCounter = 0;
        }

        private void RecalculateErrorMessageHeight()
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessageTextHeight = 0;
                errorMessageNeedsScroll = false;
                return;
            }

            using (Graphics g = CreateGraphics())
            {
                SizeF size = g.MeasureString(
                    errorMessage,
                    errorFont,
                    new SizeF(Width - 20, float.MaxValue)
                );
                errorMessageTextHeight = (int)Math.Ceiling(size.Height);
                errorMessageNeedsScroll = errorMessageTextHeight > ErrorMessageMaxHeight;
            }
        }

        private void UpdateCheckboxRect()
        {
            checkboxRect = new Rectangle(Width - 34, 8, 24, 24);
        }

        private int GetMultilineContentWidth()
        {
            return Width - (ContentPadding * 2) - ScrollBarWidth - 8;
        }

        private int GetAvailableContentHeight()
        {
            int errorHeight = (hasStatus && !isOk && !string.IsNullOrEmpty(errorMessage)) ? ErrorMessageMaxHeight + 5 : 0;
            return Height - CardTopPadding - CardBottomPadding - errorHeight;
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
            RecalculateErrorMessageHeight();
        }

        private void RecalculateTextHeights()
        {
            using (Graphics g = CreateGraphics())
            {
                foreach (var f in fields)
                {
                    if (f.Type == FieldType.Multiline)
                    {
                        int contentWidth = GetMultilineContentWidth();
                        SizeF size = g.MeasureString(
                            f.Value,
                            contentFont,
                            new SizeF(contentWidth, float.MaxValue),
                            StringFormat.GenericTypographic
                        );
                        f.TextHeight = Math.Max((int)Math.Ceiling(size.Height) + 4, 0);
                    }
                    else
                    {
                        // Measure plain text width for horizontal scrolling
                        SizeF size = g.MeasureString(f.Value, contentFont);
                        f.TextWidth = (int)Math.Ceiling(size.Width);
                    }
                }
            }
        }

        private void OnMouseLeaveHandler(object sender, EventArgs e)
        {
            if (currentHoveredField != null)
            {
                currentHoveredField.IsHovered = false;
                currentHoveredField.HorizontalScrollOffset = 0;
                currentHoveredField = null;
                horizontalScrollTimer.Stop();
                horizontalScrollForward = true;
                horizontalScrollPauseCounter = 0;
                Invalidate();
            }

            if (errorMessageIsHovered)
            {
                errorMessageIsHovered = false;
                errorMessageScrollOffset = 0;
                errorScrollTimer.Stop();
                errorScrollForward = true;
                errorScrollPauseCounter = 0;
                Invalidate();
            }

            pendingErrorCopy = false;
        }

        private void AnimateHorizontalScroll(object sender, EventArgs e)
        {
            if (currentHoveredField == null || !currentHoveredField.NeedsHorizontalScroll)
            {
                horizontalScrollTimer.Stop();
                return;
            }

            var f = currentHoveredField;
            float maxScroll = Math.Max(0, f.TextWidth - f.Area.Width + 10);

            if (horizontalScrollPauseCounter > 0)
            {
                horizontalScrollPauseCounter--;
                return;
            }

            if (horizontalScrollForward)
            {
                f.HorizontalScrollOffset += HorizontalScrollSpeed;
                if (f.HorizontalScrollOffset >= maxScroll)
                {
                    f.HorizontalScrollOffset = maxScroll;
                    horizontalScrollForward = false;
                    horizontalScrollPauseCounter = HorizontalScrollPauseAtEnd;
                }
            }
            else
            {
                f.HorizontalScrollOffset -= HorizontalScrollSpeed;
                if (f.HorizontalScrollOffset <= 0)
                {
                    f.HorizontalScrollOffset = 0;
                    horizontalScrollForward = true;
                    horizontalScrollPauseCounter = HorizontalScrollPauseAtEnd;
                }
            }

            Invalidate();
        }

        private void AnimateErrorScroll(object sender, EventArgs e)
        {
            if (!errorMessageIsHovered || !errorMessageNeedsScroll)
            {
                errorScrollTimer.Stop();
                return;
            }

            float maxScroll = Math.Max(0, errorMessageTextHeight - ErrorMessageMaxHeight);

            if (errorScrollPauseCounter > 0)
            {
                errorScrollPauseCounter--;
                return;
            }

            if (errorScrollForward)
            {
                errorMessageScrollOffset += ErrorScrollSpeed;
                if (errorMessageScrollOffset >= maxScroll)
                {
                    errorMessageScrollOffset = maxScroll;
                    errorScrollForward = false;
                    errorScrollPauseCounter = ErrorScrollPauseAtEnd;
                }
            }
            else
            {
                errorMessageScrollOffset -= ErrorScrollSpeed;
                if (errorMessageScrollOffset <= 0)
                {
                    errorMessageScrollOffset = 0;
                    errorScrollForward = true;
                    errorScrollPauseCounter = ErrorScrollPauseAtEnd;
                }
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // Check error message area for scrolling
            if (errorMessageNeedsScroll && errorMessageArea.Contains(e.Location))
            {
                float maxScroll = Math.Max(0, errorMessageTextHeight - ErrorMessageMaxHeight);
                errorMessageScrollOffset -= e.Delta / 4f;
                errorMessageScrollOffset = Math.Max(0, Math.Min(errorMessageScrollOffset, maxScroll));
                Invalidate();
                return;
            }

            foreach (var f in fields)
            {
                if (f.Type != FieldType.Multiline) continue;

                if (f.Area.Contains(e.Location))
                {
                    int maxScroll = Math.Max(0, f.TextHeight - f.Area.Height);
                    if (maxScroll > 0)
                    {
                        f.ScrollOffset -= e.Delta / 2;
                        f.ScrollOffset = Math.Max(0, Math.Min(f.ScrollOffset, maxScroll));
                        Invalidate();
                    }
                    return;
                }
            }
        }

        private void OnMouseDownHandler(object sender, MouseEventArgs e)
        {
            pendingCopyField = null;
            pendingErrorCopy = false;
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

            // Check if clicking on error message area
            if (!errorMessageArea.IsEmpty && errorMessageArea.Contains(e.Location))
            {
                pendingErrorCopy = true;
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

            if (hit.type == HitType.ScrollTrack && hit.field != null)
            {
                var f = hit.field;
                int maxScroll = Math.Max(0, f.TextHeight - f.Area.Height);
                if (maxScroll > 0)
                {
                    int trackY = f.Area.Y;
                    int trackH = f.Area.Height;
                    float clickRatio = (float)(e.Y - trackY) / trackH;
                    f.ScrollOffset = (int)(clickRatio * maxScroll);
                    f.ScrollOffset = Math.Max(0, Math.Min(f.ScrollOffset, maxScroll));
                    Invalidate();
                }
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
            // Handle error message hover
            bool isOverError = errorMessageNeedsScroll && errorMessageArea.Contains(e.Location);
            if (isOverError != errorMessageIsHovered)
            {
                errorMessageIsHovered = isOverError;
                if (errorMessageIsHovered)
                {
                    errorScrollForward = true;
                    errorScrollPauseCounter = ErrorScrollPauseAtEnd;
                    errorScrollTimer.Start();
                }
                else
                {
                    errorScrollTimer.Stop();
                    errorMessageScrollOffset = 0;
                    errorScrollForward = true;
                    errorScrollPauseCounter = 0;
                }
                Invalidate();
            }

            // Cancel error copy if mouse moves too much
            if (pendingErrorCopy)
            {
                int dx = Math.Abs(e.X - mouseDownLocation.X);
                int dy = Math.Abs(e.Y - mouseDownLocation.Y);
                if (dx > 5 || dy > 5)
                {
                    pendingErrorCopy = false;
                }
            }

            // Handle horizontal scroll hover for plain text
            var hit = HitTestField(e.Location);
            FieldEntry newHovered = null;

            if (hit.type == HitType.PlainValue && hit.field != null && hit.field.NeedsHorizontalScroll)
            {
                newHovered = hit.field;
            }

            if (newHovered != currentHoveredField)
            {
                // Reset previous hovered field
                if (currentHoveredField != null)
                {
                    currentHoveredField.IsHovered = false;
                    currentHoveredField.HorizontalScrollOffset = 0;
                }

                currentHoveredField = newHovered;
                horizontalScrollForward = true;
                horizontalScrollPauseCounter = HorizontalScrollPauseAtEnd;

                if (currentHoveredField != null)
                {
                    currentHoveredField.IsHovered = true;
                    horizontalScrollTimer.Start();
                }
                else
                {
                    horizontalScrollTimer.Stop();
                }

                Invalidate();
            }

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

            // Handle error message copy
            if (pendingErrorCopy)
            {
                if (!errorMessageArea.IsEmpty && errorMessageArea.Contains(e.Location))
                {
                    CopyToClipboard(errorMessage);
                }
                pendingErrorCopy = false;
                return;
            }

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

            using (GraphicsPath cardPath = CreateRoundedRectanglePath(rect, cornerRadius))
            {
                using (SolidBrush bg = new SolidBrush(bgColor))
                    g.FillPath(bg, cardPath);

                using (Pen borderPen = new Pen(currentBorderColor, borderThickness))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPath(borderPen, cardPath);
                }

                // Set clip region to card bounds to prevent content overflow
                Region oldClip = g.Clip;
                g.SetClip(cardPath);

                DrawCheckbox(g);

                int y = CardTopPadding;
                int maxY = Height - CardBottomPadding;

                if (hasStatus && !isOk && !string.IsNullOrEmpty(errorMessage))
                {
                    errorMessageArea = new Rectangle(10, y, Width - 20, ErrorMessageMaxHeight);

                    using (SolidBrush errorBrush = new SolidBrush(Color.DarkRed))
                    {
                        if (errorMessageNeedsScroll)
                        {
                            // Clip to error area
                            Region errorClip = g.Clip;
                            g.SetClip(Rectangle.Intersect(errorMessageArea, new Rectangle(0, 0, Width, Height)));

                            if (errorMessageIsHovered)
                            {
                                // Draw full text with scroll offset when hovering
                                g.DrawString(
                                    errorMessage,
                                    errorFont,
                                    errorBrush,
                                    new RectangleF(
                                        errorMessageArea.X,
                                        errorMessageArea.Y - errorMessageScrollOffset,
                                        errorMessageArea.Width,
                                        errorMessageTextHeight + 10
                                    )
                                );
                            }
                            else
                            {
                                // Draw full text (will be clipped)
                                g.DrawString(
                                    errorMessage,
                                    errorFont,
                                    errorBrush,
                                    new RectangleF(
                                        errorMessageArea.X,
                                        errorMessageArea.Y,
                                        errorMessageArea.Width,
                                        errorMessageTextHeight + 10
                                    )
                                );
                            }

                            g.SetClip(errorClip, CombineMode.Replace);

                            // Draw gradient fade at the bottom when not hovering and there's more content
                            if (!errorMessageIsHovered)
                            {
                                Rectangle fadeRect = new Rectangle(
                                    errorMessageArea.X,
                                    errorMessageArea.Bottom - ErrorFadeHeight,
                                    errorMessageArea.Width,
                                    ErrorFadeHeight
                                );

                                using (LinearGradientBrush fadeBrush = new LinearGradientBrush(
                                    fadeRect,
                                    Color.FromArgb(0, statusBackColor),
                                    Color.FromArgb(255, statusBackColor),
                                    LinearGradientMode.Vertical))
                                {
                                    g.FillRectangle(fadeBrush, fadeRect);
                                }
                            }
                        }
                        else
                        {
                            // No scrolling needed, just draw normally
                            g.DrawString(errorMessage, errorFont, errorBrush, errorMessageArea);
                        }
                    }

                    y += ErrorMessageMaxHeight + 5;
                }
                else
                {
                    errorMessageArea = Rectangle.Empty;
                }

                foreach (var f in fields)
                {
                    // Stop drawing if we've exceeded the card bounds
                    if (y >= maxY) break;

                    SizeF labelSize = g.MeasureString(f.Label + ":", labelFont);
                    f.LabelArea = new Rectangle(10, y, (int)labelSize.Width, (int)labelSize.Height);

                    using (Brush labelBrush = new SolidBrush(Color.Black))
                        g.DrawString(f.Label + ":", labelFont, labelBrush, new PointF(10, y));

                    if (f.Type == FieldType.PlainText)
                    {
                        Rectangle valueRect = new Rectangle(130, y, Width - 140, PlainTextRowHeight);
                        f.Area = valueRect;

                        // Check if text needs horizontal scrolling
                        f.NeedsHorizontalScroll = f.TextWidth > valueRect.Width;

                        // Clip to the value area for horizontal scrolling
                        Region fieldClip = g.Clip;
                        g.SetClip(Rectangle.Intersect(valueRect, new Rectangle(0, 0, Width, Height)));

                        using (SolidBrush valueBrush = new SolidBrush(Color.DarkSlateGray))
                        {
                            float drawX = valueRect.X - f.HorizontalScrollOffset;

                            if (f.IsHovered && f.NeedsHorizontalScroll)
                            {
                                // Draw full text when hovering and scrolling
                                g.DrawString(f.Value, contentFont, valueBrush,
                                    new PointF(drawX, valueRect.Y));
                            }
                            else
                            {
                                // Draw with ellipsis when not hovering
                                using (StringFormat sf = new StringFormat())
                                {
                                    sf.Trimming = StringTrimming.EllipsisCharacter;
                                    sf.FormatFlags = StringFormatFlags.NoWrap;
                                    g.DrawString(f.Value, contentFont, valueBrush,
                                        new RectangleF(valueRect.X, valueRect.Y, valueRect.Width, valueRect.Height), sf);
                                }
                            }
                        }

                        g.SetClip(fieldClip, CombineMode.Replace);

                        y += PlainTextRowHeight;
                    }
                    else
                    {
                        // Calculate available height for multiline area
                        int availableHeight = maxY - (y + MultilineAreaSpacing) - 10;
                        if (availableHeight < 40) break; // Not enough space for multiline field

                        int multilineHeight = Math.Min(MultilineAreaHeight, availableHeight);
                        int contentWidth = GetMultilineContentWidth();
                        Rectangle rectArea = new Rectangle(ContentPadding, y + MultilineAreaSpacing, contentWidth, multilineHeight);
                        f.Area = rectArea;

                        Region fieldClip = g.Clip;
                        g.SetClip(Rectangle.Intersect(rectArea, new Rectangle(0, 0, Width, Height)));

                        using (SolidBrush textBrush = new SolidBrush(Color.DarkSlateGray))
                        {
                            g.DrawString(
                                f.Value,
                                contentFont,
                                textBrush,
                                new RectangleF(
                                    rectArea.X + 2,
                                    rectArea.Y + 2 - f.ScrollOffset,
                                    rectArea.Width - 4,
                                    Math.Max(f.TextHeight, rectArea.Height)
                                ),
                                StringFormat.GenericTypographic
                            );
                        }

                        g.SetClip(fieldClip, CombineMode.Replace);

                        using (Pen p = new Pen(Color.LightGray))
                            g.DrawRectangle(p, rectArea);

                        // Always draw scrollbar track for multiline fields
                        int trackX = rectArea.Right + 4;
                        int trackY = rectArea.Y;
                        int trackW = ScrollBarWidth;
                        int trackH = rectArea.Height;

                        // Draw track background
                        using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                            g.FillRectangle(trackBrush, trackX, trackY, trackW, trackH);

                        using (Pen trackBorderPen = new Pen(Color.FromArgb(210, 210, 210)))
                            g.DrawRectangle(trackBorderPen, trackX, trackY, trackW, trackH);

                        bool needsScrolling = f.TextHeight > rectArea.Height;
                        if (needsScrolling)
                        {
                            float ratio = (float)rectArea.Height / f.TextHeight;
                            int thumbH = Math.Max(24, (int)(trackH * ratio));

                            float scrollable = Math.Max(1, f.TextHeight - rectArea.Height);
                            float scrollRatio = (float)f.ScrollOffset / scrollable;
                            int thumbY = trackY + (int)((trackH - thumbH) * scrollRatio);

                            Rectangle thumb = new Rectangle(trackX + 1, thumbY, trackW - 2, thumbH);
                            f.ThumbRect = thumb;

                            using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                            {
                                using (GraphicsPath thumbPath = CreateRoundedRectanglePath(
                                    new RectangleF(thumb.X, thumb.Y, thumb.Width, thumb.Height), 4))
                                {
                                    g.FillPath(thumbBrush, thumbPath);
                                }
                            }
                        }
                        else
                        {
                            // No scrolling needed - show disabled thumb spanning full track
                            Rectangle thumb = new Rectangle(trackX + 1, trackY + 1, trackW - 2, trackH - 2);
                            f.ThumbRect = thumb;

                            using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(220, 220, 220)))
                            {
                                using (GraphicsPath thumbPath = CreateRoundedRectanglePath(
                                    new RectangleF(thumb.X, thumb.Y, thumb.Width, thumb.Height), 4))
                                {
                                    g.FillPath(thumbBrush, thumbPath);
                                }
                            }
                        }

                        y += multilineHeight + MultilineAreaSpacing + 10;
                    }
                }

                // Restore original clip
                g.SetClip(oldClip, CombineMode.Replace);
            }
        }

        private void DrawCheckbox(Graphics g)
        {
            if (!showCheckbox)
                return;

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

        private enum HitType { None, Thumb, MultilineArea, PlainValue, Label, ScrollTrack }

        private (HitType type, FieldEntry field) HitTestField(Point p)
        {
            foreach (var f in fields)
            {
                if (f.Type == FieldType.Multiline)
                {
                    // Check scrollbar track area
                    Rectangle trackRect = new Rectangle(f.Area.Right + 4, f.Area.Y, ScrollBarWidth, f.Area.Height);

                    if (f.ThumbRect != Rectangle.Empty && f.ThumbRect.Contains(p))
                        return (HitType.Thumb, f);

                    if (trackRect.Contains(p))
                        return (HitType.ScrollTrack, f);
                }

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
                errorFont?.Dispose();
                borderAnimationTimer?.Dispose();
                horizontalScrollTimer?.Dispose();
                errorScrollTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}