using System.Diagnostics;

namespace CorsaCareer;

public static class ContentManagerLaunch
{
    public static ProcessStartInfo Build(string contentManagerPath, string presetPath)
    {
        // Il passaggio del file .cmpreset come argomento apre la finestra di
        // conferma di Content Manager ("Apply preset" / "Just Go"). La URI
        // race/quick è invece il percorso di esecuzione diretto: CM legge il
        // preset locale e chiama QuickDrive.RunAsync senza mostrare la scelta.
        var encodedPreset = Uri.EscapeDataString(Path.GetFullPath(presetPath));
        return new ProcessStartInfo
        {
            FileName = contentManagerPath,
            Arguments = $"\"acmanager://race/quick?presetFile={encodedPreset}\"",
            UseShellExecute = true
        };
    }
}
