using System.Drawing.Drawing2D;

namespace DesktopPerformance.UI;

public class RetroButton : Button
{
    private bool _isPressed = false;

    public RetroButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = RetroTheme.BackgroundColor;
        ForeColor = RetroTheme.Black;
        Font = RetroTheme.DefaultFont;
        Cursor = Cursors.Default;
        Padding = new Padding(4);
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

        using (var brush = new SolidBrush(BackColor))
        {
            g.FillRectangle(brush, rect);
        }

        if (_isPressed)
        {
            RetroTheme.Draw3DSunkenBorder(g, rect);
        }
        else
        {
            RetroTheme.Draw3DRaisedBorder(g, rect);
        }

        // Draw Text
        var textRect = new Rectangle(
            rect.X + (_isPressed ? 1 : 0),
            rect.Y + (_isPressed ? 1 : 0),
            rect.Width,
            rect.Height);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        using (var brush = new SolidBrush(Enabled ? ForeColor : RetroTheme.DarkShadow))
        {
            g.DrawString(Text, Font, brush, textRect, format);
        }

        // Focus rectangle
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 7, rect.Height - 7);
            ControlPaint.DrawFocusRectangle(g, focusRect);
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
