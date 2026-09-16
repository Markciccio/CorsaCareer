using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace CorsaCareerInstaller;

internal static class Program
{
    private const string AppName = "CorsaCareer";

    [STAThread]
    private static void Main()
    {
        try
        {
            var target = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName);
            ExtractPackage(target);

            var installedExe = Path.Combine(target, "CorsaCareer.exe");
            CreateDesktopShortcut(installedExe, target);
            Process.Start(new ProcessStartInfo(installedExe) { WorkingDirectory = target, UseShellExecute = true });

            MessageBox.Show(
                "Installazione completata. Il collegamento Corsa Career è stato creato sul Desktop.",
                "CorsaCareer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "Installazione di CorsaCareer", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void ExtractPackage(string target)
    {
        var resourceName = $"{typeof(Program).Namespace}.payload.zip";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Il pacchetto di CorsaCareer è assente dall'installer.");
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var root = Path.GetFullPath(target + Path.DirectorySeparatorChar);

        Directory.CreateDirectory(target);
        foreach (var entry in archive.Entries)
        {
            var destination = Path.GetFullPath(Path.Combine(target, entry.FullName));
            if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Il pacchetto contiene un percorso non valido.");

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destination);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, true);
        }
    }

    private static void CreateDesktopShortcut(string targetExe, string workingDirectory)
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var shortcutPath = Path.Combine(desktop, "Corsa Career.lnk");
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Impossibile creare il collegamento sul Desktop.");
        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Impossibile creare il collegamento sul Desktop.");
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetExe;
        shortcut.WorkingDirectory = workingDirectory;
        shortcut.Description = "CorsaCareer — manager di carriera per Assetto Corsa";
        shortcut.IconLocation = targetExe + ",0";
        shortcut.Save();
    }
}
