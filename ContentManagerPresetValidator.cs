using System.Text.Json;

namespace CorsaCareer;

public static class ContentManagerPresetValidator
{
    public static bool TryReadIdentity(string path, out string car, out string track, out string error)
    {
        car = ""; track = ""; error = "";
        try
        {
            if (!File.Exists(path)) { error = "preset non trovato"; return false; }
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            car = root.TryGetProperty("CarId", out var carValue) ? carValue.GetString() ?? "" : "";
            track = root.TryGetProperty("TrackId", out var trackValue) ? trackValue.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(car) || string.IsNullOrWhiteSpace(track)) { error = "auto o circuito assenti"; return false; }
            return true;
        }
        catch (Exception ex) { error = $"JSON non valido: {ex.Message}"; return false; }
    }

    public static bool TryValidate(string path, out string error, IEnumerable<string>? availableCars = null, IEnumerable<string>? availableTracks = null)
    {
        error = "";
        try
        {
            if (!File.Exists(path)) { error = "preset non trovato"; return false; }
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            var mode = root.TryGetProperty("Mode", out var modeValue) ? modeValue.GetString() : "";
            var car = root.TryGetProperty("CarId", out var carValue) ? carValue.GetString() : "";
            var track = root.TryGetProperty("TrackId", out var trackValue) ? trackValue.GetString() : "";
            if (string.IsNullOrWhiteSpace(mode) || !mode.Contains("QuickDrive", StringComparison.OrdinalIgnoreCase)) { error = "modalità Quick Drive assente"; return false; }
            if (string.IsNullOrWhiteSpace(car) || string.IsNullOrWhiteSpace(track)) { error = "auto o circuito assenti"; return false; }
            if (!root.TryGetProperty("ModeData", out var modeDataValue) || modeDataValue.ValueKind != JsonValueKind.String) { error = "ModeData assente"; return false; }
            using var modeData = JsonDocument.Parse(modeDataValue.GetString() ?? "");
            // I test usano il modo Practice nativo di CM: non devono avere una
            // RaceGridSerialized né un avversario, altrimenti CM li trasforma
            // in un weekend di gara e mostra l'errore "set at least one opponent".
            if (mode.Contains("QuickDrive_Practice", StringComparison.OrdinalIgnoreCase))
            {
                if (!modeData.RootElement.TryGetProperty("StartType", out _)) { error = "configurazione Practice assente"; return false; }
                if (availableCars != null && !availableCars.Contains(car, StringComparer.OrdinalIgnoreCase)) { error = $"auto non più installata: {car}"; return false; }
                if (availableTracks != null && !availableTracks.Contains(track, StringComparer.OrdinalIgnoreCase)) { error = $"circuito non più installato: {track}"; return false; }
                return true;
            }
            if (!modeData.RootElement.TryGetProperty("RaceGridSerialized", out var gridValue) || gridValue.ValueKind != JsonValueKind.String) { error = "griglia assente"; return false; }
            using var grid = JsonDocument.Parse(gridValue.GetString() ?? "");
            var gridRoot = grid.RootElement;
            if (!gridRoot.TryGetProperty("CarIds", out var cars) || cars.ValueKind != JsonValueKind.Array || cars.GetArrayLength() == 0) { error = "nessuna auto in griglia"; return false; }
            if (!cars.EnumerateArray().Any(x => string.Equals(x.GetString(), car, StringComparison.OrdinalIgnoreCase))) { error = "l’auto del pilota non è nella griglia"; return false; }
            var opponents = gridRoot.TryGetProperty("OpponentsNumber", out var opponentsValue) ? opponentsValue.GetInt32() : -1;
            if (opponents < 0 || opponents > cars.GetArrayLength() - 1) { error = "numero avversari incoerente"; return false; }
            if (availableCars != null && !availableCars.Contains(car, StringComparer.OrdinalIgnoreCase)) { error = $"auto non più installata: {car}"; return false; }
            if (availableTracks != null && !availableTracks.Contains(track, StringComparer.OrdinalIgnoreCase)) { error = $"circuito non più installato: {track}"; return false; }
            return true;
        }
        catch (Exception ex) { error = $"JSON non valido: {ex.Message}"; return false; }
    }
}
