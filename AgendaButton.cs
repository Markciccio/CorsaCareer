using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Riga dell'agenda composta da un campo orario e da un campo descrizione.
/// Evita le ellissi dei Button standard e mantiene leggibili anche le voci di
/// Haru, che spesso hanno descrizioni più lunghe.
/// </summary>
internal sealed class AgendaButton : Button
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TimeText { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ActivityText { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; } = UiTheme.Info;

    public AgendaButton()
    {
        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
        Text = "";
        Height = 42;
        Padding = Padding.Empty;
        TabStop = false;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        UiTheme.RoundCorners(this, 8);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
        var disabled = !Enabled;
        var accent = disabled ? Color.FromArgb(120, UiTheme.TextMuted) : AccentColor;
        var text = disabled ? Color.FromArgb(142, UiTheme.TextMuted) : UiTheme.TextPrimary;
        using (var background = new SolidBrush(BackColor))
            e.Graphics.FillRoundedRectangle(background, bounds, 8);

        var timeWidth = Math.Min(112, Math.Max(96, Width / 3));
        var timeBounds = new Rectangle(1, 1, Math.Max(0, timeWidth - 1), Math.Max(0, Height - 2));
        using (var timeBackground = new SolidBrush(disabled
                   ? Color.FromArgb(28, 70, 93, 122)
                   : Color.FromArgb(118, 42, 105, 157)))
            e.Graphics.FillRoundedRectangle(timeBackground, timeBounds, 7);

        using (var divider = new Pen(Color.FromArgb(disabled ? 70 : 150, accent), 1f))
            e.Graphics.DrawLine(divider, timeWidth, 6, timeWidth, Math.Max(6, Height - 7));
        UiTheme.DrawRoundedBorder(e.Graphics, bounds, Color.FromArgb(disabled ? 70 : 180, accent), 1f, 8);

        var timeFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
        TextRenderer.DrawText(e.Graphics, TimeText, UiTheme.BodyStrong,
            new Rectangle(9, 0, Math.Max(10, timeWidth - 15), Height), accent, timeFlags);

        var activityFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                            TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(e.Graphics, ActivityText, UiTheme.BodyStrong,
            new Rectangle(timeWidth + 12, 0, Math.Max(10, Width - timeWidth - 18), Height),
            text, activityFlags);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Invalidate();
    }
}

internal static class AgendaGraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        using var path = RoundedPath(bounds, radius);
        graphics.FillPath(brush, path);
    }

    private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Max(2, radius * 2);
        var r = new Rectangle(bounds.X, bounds.Y, Math.Max(diameter, bounds.Width), Math.Max(diameter, bounds.Height));
        path.AddArc(r.X, r.Y, diameter, diameter, 180, 90);
        path.AddArc(r.Right - diameter, r.Y, diameter, diameter, 270, 90);
        path.AddArc(r.Right - diameter, r.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(r.X, r.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
