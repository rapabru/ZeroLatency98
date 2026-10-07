using System.Drawing.Drawing2D;

namespace DesktopPerformance.UI;

public static class RetroTheme
{
    // Windows 95/98/2000 Classic Color Palette
    public static readonly Color BackgroundColor = Color.FromArgb(192, 192, 192); // Classic Win98 ButtonFace #C0C0C0
    public static readonly Color HighlightLight = Color.FromArgb(255, 255, 255);  // #FFFFFF
    public static readonly Color LightShadow = Color.FromArgb(223, 223, 223);     // #DFDFDF
    public static readonly Color DarkShadow = Color.FromArgb(128, 128, 128);      // #808080
    public static readonly Color Black = Color.FromArgb(0, 0, 0);                 // #000000
    public static readonly Color WindowBackground = Color.FromArgb(255, 255, 255);
    public static readonly Color TitleGradientStart = Color.FromArgb(0, 0, 128);  // Navy #000080
    public static readonly Color TitleGradientEnd = Color.FromArgb(16, 132, 208); // Cyan-Blue #1084D0
    public static readonly Color TitleTextColor = Color.FromArgb(255, 255, 255);

    public static readonly Color ButtonHoverColor = Color.FromArgb(222, 222, 222);
    public static readonly Color ButtonPressedColor = Color.FromArgb(176, 176, 176);

    public static Font DefaultFont { get; }
    public static Font BoldFont { get; }
    public static Font TitleFont { get; }
    public static Font MonospaceFont { get; }

    static RetroTheme()
    {
        // Standard Windows 98/2000 system typography
        const string fontName = "Tahoma";
        DefaultFont = new Font(fontName, 8.25f, FontStyle.Regular);
        BoldFont = new Font(fontName, 8.25f, FontStyle.Bold);
        TitleFont = new Font(fontName, 9.0f, FontStyle.Bold);
        MonospaceFont = new Font("Lucida Console", 8.25f, FontStyle.Regular);
    }

    public static void Draw3DRaisedBorder(Graphics g, Rectangle rect)
    {
        ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Raised);
    }

    public static void Draw3DSunkenBorder(Graphics g, Rectangle rect)
    {
        ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
    }

    public static void DrawTitleBar(Graphics g, Rectangle rect, string title)
    {
        using (var brush = new LinearGradientBrush(rect, TitleGradientStart, TitleGradientEnd, LinearGradientMode.Horizontal))
        {
            g.FillRectangle(brush, rect);
        }

        using (var brush = new SolidBrush(TitleTextColor))
        {
            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            var textRect = new Rectangle(rect.X + 6, rect.Y, rect.Width - 12, rect.Height);
            g.DrawString(title, TitleFont, brush, textRect, format);
        }
    }
}
