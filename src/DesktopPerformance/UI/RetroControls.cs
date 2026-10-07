using System.Drawing.Drawing2D;

namespace DesktopPerformance.UI;

public class RetroButton : Button
{
    private bool _isPressed = false;
    private bool _isHovered = false;

    [System.ComponentModel.DefaultValue(false)]
    public bool IsPrimary { get; set; } = false;

    public RetroButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);

        BackColor = RetroTheme.BackgroundColor;
        ForeColor = RetroTheme.Black;
        Font = RetroTheme.DefaultFont;
        Cursor = Cursors.Hand;
        Padding = new Padding(6, 4, 6, 4);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        if (mevent.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var rect = ClientRectangle;

        if (rect.Width <= 0 || rect.Height <= 0) return;

        // Background color calculation
        Color bgColor;
        if (!Enabled)
        {
            bgColor = RetroTheme.BackgroundColor;
        }
        else if (_isPressed)
        {
            bgColor = RetroTheme.ButtonPressedColor;
        }
        else if (_isHovered)
        {
            bgColor = RetroTheme.ButtonHoverColor;
        }
        else
        {
            bgColor = BackColor;
        }

        using (var brush = new SolidBrush(bgColor))
        {
            g.FillRectangle(brush, rect);
        }

        // Primary (Default Button) outline + 3D Border
        var workRect = rect;
        bool drawDefaultBorder = (IsPrimary || (IsDefault && ShowFocusCues)) && Enabled;

        if (drawDefaultBorder)
        {
            using var penBlack = new Pen(Color.Black);
            g.DrawRectangle(penBlack, rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
            workRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
        }

        if (_isPressed)
        {
            ControlPaint.DrawBorder3D(g, workRect, Border3DStyle.Sunken);
        }
        else
        {
            ControlPaint.DrawBorder3D(g, workRect, Border3DStyle.Raised);
        }

        // Text & Mnemonic rendering
        int offset = _isPressed ? 1 : 0;
        var textRect = new Rectangle(
            workRect.Left + offset,
            workRect.Top + offset,
            workRect.Width,
            workRect.Height);

        var flags = TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.WordEllipsis;

        if (!ShowKeyboardCues)
        {
            flags |= TextFormatFlags.HidePrefix;
        }

        if (Enabled)
        {
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, flags);
        }
        else
        {
            // Authentic Windows 95/98 disabled embossed text
            var highlightRect = new Rectangle(textRect.X + 1, textRect.Y + 1, textRect.Width, textRect.Height);
            TextRenderer.DrawText(g, Text, Font, highlightRect, Color.White, flags);
            TextRenderer.DrawText(g, Text, Font, textRect, RetroTheme.DarkShadow, flags);
        }

        // Focus cues
        if (Focused && ShowFocusCues && Enabled)
        {
            var focusRect = new Rectangle(
                workRect.Left + 3 + offset,
                workRect.Top + 3 + offset,
                Math.Max(0, workRect.Width - 7),
                Math.Max(0, workRect.Height - 7));

            if (focusRect.Width > 0 && focusRect.Height > 0)
            {
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }
        }
    }
}

public class RetroPanel : Panel
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    [System.ComponentModel.DefaultValue(true)]
    public bool IsSunken { get; set; } = true;

    public RetroPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = RetroTheme.BackgroundColor;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (IsSunken)
        {
            RetroTheme.Draw3DSunkenBorder(e.Graphics, ClientRectangle);
        }
        else
        {
            RetroTheme.Draw3DRaisedBorder(e.Graphics, ClientRectangle);
        }
    }
}
