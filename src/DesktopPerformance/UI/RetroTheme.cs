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

    public static Font DefaultFont { get; }
    public static Font BoldFont { get; }
    public static Font TitleFont { get; }
    public static Font MonospaceFont { get; }

    static RetroTheme()
    {
        // Try Tahoma or MS Sans Serif
        string fontName = "Tahoma";
        using (var testFont = new Font("MS Sans Serif", 8.25f))
        {
            if (testFont.Name == "MS Sans Serif")
                fontName = "MS Sans Serif";
        }

        DefaultFont = new Font(fontName, 8.25f, FontStyle.Regular);
        BoldFont = new Font(fontName, 8.25f, FontStyle.Bold);
        TitleFont = new Font(fontName, 9.0f, FontStyle.Bold);
        MonospaceFont = new Font("Lucida Console", 8.25f, FontStyle.Regular);
    }

    public static void Draw3DRaisedBorder(Graphics g, Rectangle rect)
    {
        using var penWhite = new Pen(HighlightLight);
        using var penDark = new Pen(DarkShadow);
        using var penBlack = new Pen(Black);

        // Top and Left outer
        g.DrawLine(penWhite, rect.Left, rect.Top, rect.Right - 1, rect.Top);
        g.DrawLine(penWhite, rect.Left, rect.Top, rect.Left, rect.Bottom - 1);

        // Bottom and Right outer
        g.DrawLine(penBlack, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
        g.DrawLine(penBlack, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);

        // Bottom and Right inner
        g.DrawLine(penDark, rect.Left + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
        g.DrawLine(penDark, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);
    }

    public static void Draw3DSunkenBorder(Graphics g, Rectangle rect)
    {
        using var penWhite = new Pen(HighlightLight);
        using var penDark = new Pen(DarkShadow);
        using var penBlack = new Pen(Black);

        // Top and Left outer
        g.DrawLine(penDark, rect.Left, rect.Top, rect.Right - 1, rect.Top);
        g.DrawLine(penDark, rect.Left, rect.Top, rect.Left, rect.Bottom - 1);

        // Top and Left inner
        g.DrawLine(penBlack, rect.Left + 1, rect.Top + 1, rect.Right - 2, rect.Top + 1);
        g.DrawLine(penBlack, rect.Left + 1, rect.Top + 1, rect.Left + 1, rect.Bottom - 2);

        // Bottom and Right outer
        g.DrawLine(penWhite, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
        g.DrawLine(penWhite, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
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
