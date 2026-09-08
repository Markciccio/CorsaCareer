using System.Net.Http;
using System.Security.Cryptography;

namespace CorsaCareer;

public static class ContentDownloadService
{
    public static bool ValidateRequest(string url, string sha256, out string error)
    {
        error = "";
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps) { error = "Inserisci un URL HTTPS verificato."; return false; }
        if (string.IsNullOrWhiteSpace(sha256) || sha256.Trim().Length != 64 || sha256.Trim().Any(c => !Uri.IsHexDigit(c))) { error = "Inserisci un hash SHA-256 di 64 caratteri esadecimali."; return false; }
        return true;
    }

    public static async Task<string> DownloadVerifiedAsync(string url, string sha256, string temporaryDirectory, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(url, sha256, out var error)) throw new InvalidDataException(error);
        Directory.CreateDirectory(temporaryDirectory);
        var destination = Path.Combine(temporaryDirectory, $"corsacareer-{Guid.NewGuid():N}.zip");
        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
            using var response = await client.GetAsync(url.Trim(), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400) throw new InvalidDataException("Il server ha richiesto un redirect: per sicurezza usa l’URL HTTPS finale verificato.");
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is > 268435456) throw new InvalidDataException("Il pacchetto supera il limite di 256 MB.");
            const long maxBytes = 268435456;
            await using (var input = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var output = File.Create(destination))
            {
                var buffer = new byte[81920]; long total = 0; int read;
                while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                {
                    total += read;
                    if (total > maxBytes) throw new InvalidDataException("Il pacchetto supera il limite di 256 MB.");
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
            }
            string actual;
            await using (var verification = File.OpenRead(destination)) actual = Convert.ToHexString(await SHA256.HashDataAsync(verification, cancellationToken));
            if (!actual.Equals(sha256.Trim(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"SHA-256 non corrispondente. Atteso {sha256.Trim()}, ricevuto {actual}.");
            return destination;
        }
        catch { try { if (File.Exists(destination)) File.Delete(destination); } catch { } throw; }
    }
}
