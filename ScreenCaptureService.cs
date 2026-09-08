using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace CorsaCareer;

public static class ScreenCaptureService
{
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(nint handle, StringBuilder text, int maxCount);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint handle, out RECT rect);
    private struct RECT { public int Left, Top, Right, Bottom; }

    public static bool TryCaptureAssettoCorsa(string destination, out string error)
    {
        error = "";
        var handle = GetForegroundWindow();
        if (handle == 0 || !GetWindowRect(handle, out var rect)) { error = "Nessuna finestra attiva catturabile."; return false; }
        var titleBuffer = new StringBuilder(256); GetWindowText(handle, titleBuffer, titleBuffer.Capacity);
        var title = titleBuffer.ToString();
        if (!title.Contains("Assetto Corsa", StringComparison.OrdinalIgnoreCase)) { error = "La finestra in primo piano non sembra Assetto Corsa. Porta il simulatore in primo piano e riprova."; return false; }
        var width = rect.Right - rect.Left; var height = rect.Bottom - rect.Top;
        if (width < 320 || height < 200) { error = "La finestra di Assetto Corsa è troppo piccola per una cattura utile."; return false; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap)) graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size);
            bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }
}
