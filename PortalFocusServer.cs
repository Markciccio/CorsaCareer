using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CorsaCareer1991;

/// <summary>
/// Piccolo endpoint locale che permette alle pagine generate (portale browser e
/// storia guidata) di riportare il fuoco alla finestra principale.
///
/// Una pagina aperta nel browser non può dare il fuoco a un'altra applicazione:
/// serve qualcosa che ascolti in locale. Sono usati un <see cref="TcpListener"/>
/// su <c>127.0.0.1</c> e una porta effimera — non <c>HttpListener</c>, che su
/// Windows richiede una prenotazione URL con privilegi di amministratore.
///
/// Vincoli: ascolta solo su loopback, accetta un unico percorso senza parametri,
/// non legge né restituisce dati della carriera, e vive quanto la finestra.
/// </summary>
public sealed class PortalFocusServer : IDisposable
{
    public const string FocusPath = "/focus";

    private readonly TcpListener listener;
    private readonly Action onFocusRequested;
    private readonly CancellationTokenSource cancellation = new();
    private bool disposed;

    private PortalFocusServer(TcpListener listener, Action onFocusRequested)
    {
        this.listener = listener;
        this.onFocusRequested = onFocusRequested;
    }

    /// <summary>Indirizzo da inserire nelle pagine generate. Vuoto se l'avvio è fallito.</summary>
    public string Url { get; private set; } = "";

    /// <summary>
    /// Avvia l'endpoint. Se la porta non è disponibile restituisce null senza
    /// propagare l'errore: le pagine funzionano comunque, solo senza il pulsante.
    /// </summary>
    public static PortalFocusServer? TryStart(Action onFocusRequested)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var server = new PortalFocusServer(listener, onFocusRequested)
            {
                Url = $"http://127.0.0.1:{port}{FocusPath}"
            };
            _ = Task.Run(server.AcceptLoop);
            CareerLog.Info("portale", $"endpoint di ritorno al portale attivo su {server.Url}");
            return server;
        }
        catch (Exception error)
        {
            CareerLog.Warn("portale", $"endpoint di ritorno non avviato: {error.Message}");
            return null;
        }
    }

    private async Task AcceptLoop()
    {
        while (!cancellation.IsCancellationRequested)
        {
            TcpClient client;
            try { client = await listener.AcceptTcpClientAsync(cancellation.Token); }
            catch (OperationCanceledException) { return; }
            catch (ObjectDisposedException) { return; }
            catch (Exception error) { CareerLog.Warn("portale", $"connessione rifiutata: {error.Message}"); continue; }
            _ = Task.Run(() => Handle(client));
        }
    }

    private void Handle(TcpClient client)
    {
        try
        {
            using (client)
            {
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;
                using var stream = client.GetStream();
                var buffer = new byte[1024];
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) return;
                var requestLine = Encoding.ASCII.GetString(buffer, 0, read).Split('\r')[0];
                var focus = requestLine.StartsWith($"GET {FocusPath}", StringComparison.Ordinal);
                var body = focus ? "ok" : "not found";
                var status = focus ? "200 OK" : "404 Not Found";
                var response = Encoding.UTF8.GetBytes(
                    $"HTTP/1.1 {status}\r\n" +
                    "Content-Type: text/plain; charset=utf-8\r\n" +
                    "Access-Control-Allow-Origin: *\r\n" +
                    "Cache-Control: no-store\r\n" +
                    $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
                    "Connection: close\r\n\r\n" + body);
                stream.Write(response, 0, response.Length);
                stream.Flush();
                if (focus) onFocusRequested();
            }
        }
        catch (Exception error) { CareerLog.Warn("portale", $"richiesta di ritorno non gestita: {error.Message}"); }
    }

    /// <summary>Frammento JavaScript del pulsante di ritorno, o stringa vuota se l'endpoint non è attivo.</summary>
    public static string ButtonHtml(string focusUrl) => string.IsNullOrWhiteSpace(focusUrl)
        ? ""
        : "<button class='back' onclick=\"backToPortal()\">&#8617; TORNA AL PORTALE</button>";

    public static string ButtonScript(string focusUrl) => string.IsNullOrWhiteSpace(focusUrl)
        ? "function backToPortal(){}"
        // Il fetch riporta il fuoco all'applicazione; la chiusura della schedaresta
        // un tentativo, perché i browser la consentono solo per finestre aperte da script.
        : "function backToPortal(){fetch('" + focusUrl + "',{mode:'no-cors',cache:'no-store'}).catch(function(){});setTimeout(function(){try{window.close()}catch(e){}},220)}";

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { cancellation.Cancel(); } catch { }
        try { listener.Stop(); } catch { }
        cancellation.Dispose();
    }
}
