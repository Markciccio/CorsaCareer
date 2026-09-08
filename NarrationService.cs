using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Text.Json;

namespace CorsaCareer;

public static class NarrationService
{
    private static readonly object Gate = new();

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private const int SwMaximize = 3;

    /// <summary>
    /// L'articolo si apre nel browser predefinito con la finestra di sempre —
    /// piccola, in un angolo, spesso dietro le altre. Un capitolo che deve
    /// leggersi prima di tornare al portale merita lo schermo intero, non un
    /// riquadro qualunque. Cerca per titolo perché è l'unico aggancio stabile
    /// che resta dopo un avvio "spara e dimentica" (Process.Start non aspetta
    /// che la finestra esista davvero, tantomeno in browser a istanza unica
    /// che smistano l'apertura a un processo già in esecuzione).
    /// </summary>
    private static void MaximizeBrowserWindow(string exactTitle, bool blocking = false)
    {
        // Durante il percorso guidato iniziale (prima ancora che la finestra
        // principale esista) conviene aspettare per davvero: se si torna
        // subito al chiamante, il portale finisce di costruirsi e si mostra
        // per primo, rubando il fuoco al browser un istante dopo che lo ha
        // ottenuto — la sequenza sembra "saltare" il dossier anche se in
        // realtà è aperto, solo dietro. A portale già visibile, invece, il
        // polling gira su un thread suo per non bloccare l'interfaccia.
        if (blocking) { CercaEMassimizza(exactTitle); return; }
        Task.Run(() => CercaEMassimizza(exactTitle));
    }

    private static void CercaEMassimizza(string exactTitle)
    {
        try
        {
            for (var tentativo = 0; tentativo < 20; tentativo++)
            {
                var trovata = IntPtr.Zero;
                EnumWindows((hwnd, _) =>
                {
                    if (!IsWindowVisible(hwnd)) return true;
                    var buffer = new System.Text.StringBuilder(256);
                    GetWindowText(hwnd, buffer, buffer.Capacity);
                    if (buffer.ToString().Contains(exactTitle, StringComparison.Ordinal)) { trovata = hwnd; return false; }
                    return true;
                }, IntPtr.Zero);
                if (trovata != IntPtr.Zero)
                {
                    ShowWindow(trovata, SwMaximize);
                    SetForegroundWindow(trovata);
                    return;
                }
                Thread.Sleep(150);
            }
        }
        catch { }
    }

    /// <summary>
    /// Ridà il fuoco al dossier del pilota dopo che il portale è comparso.
    ///
    /// Il primo tentativo (bloccante, prima che il portale esistesse) non
    /// basta da solo: quando la finestra principale si mostra per la prima
    /// volta Windows la attiva comunque, e lo stesso fa il tutorial del
    /// budget al primo avvio. L'ultimo a chiedere il fuoco vince, quindi qui
    /// si ripete la richiesta apposta dopo che il portale si è stabilizzato.
    /// </summary>
    public static void RefocusLastGuidedStory()
    {
        Task.Run(() =>
        {
            Thread.Sleep(400);
            CercaEMassimizza("CorsaCareer — Inizia la storia");
        });
    }
    private static Process? active;

    public static bool IsSpeaking
    {
        get
        {
            lock (Gate) return active is { HasExited: false };
        }
    }

    public static void Speak(string text, string? preferredVoice = null)
    {
        if (preferredVoice?.StartsWith("Browser", StringComparison.OrdinalIgnoreCase) == true)
        {
            OpenBrowserNarration(text);
            return;
        }
        Start(text, null, true, preferredVoice);
    }

    /// <summary>Riproduce una narrazione già registrata, mantenendo lo stesso comando Stop delle voci sintetiche.</summary>
    public static bool PlayAudioFile(string path)
    {
        // Come per la colonna sonora: nei collaudi automatici nessun player va
        // avviato, altrimenti il processo di rendering resta appeso al termine
        // della traccia invece di chiudersi.
        if (string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal)) return false;
        if (!File.Exists(path)) return false;
        var player = FindFfplay();
        if (player == null) return false;
        Stop();
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = player,
            // 100 è il livello naturale di ffplay: oltre amplifica e distorce.
            // La voce deve stare davanti alla musica alzando il proprio livello
            // solo fin qui, e abbassando il sottofondo — non gridando.
            Arguments = $"-nodisp -autoexit -loglevel quiet -volume 100 \"{path}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true
        });
        // Anche la voce deve morire con l'applicazione: una narrazione lunga
        // sopravviveva a un arresto anomalo continuando a parlare senza finestra.
        SoundtrackService.BindToApplicationLifetime(process);
        lock (Gate) active = process;
        return process != null;
    }

    /// <summary>
    /// Indirizzo dell'endpoint locale che riporta il fuoco alla finestra
    /// principale. Impostato da MainForm all'avvio; vuoto se l'endpoint non è
    /// disponibile, e in quel caso le pagine non mostrano il pulsante.
    /// </summary>
    public static string PortalFocusUrl { get; set; } = "";


    public static void OpenBrowserPortal(BrowserPortalContent portal)
        => OpenBrowserPortal(portal, "rubrica");

    private static void OpenBrowserPortal(BrowserPortalContent portal, string fileStem)
    {
        if (portal == null || string.IsNullOrWhiteSpace(portal.AudioText)) return;
        var safeStem = string.Concat(fileStem.Where(char.IsLetterOrDigit));
        var path = Path.Combine(Path.GetTempPath(), $"corsacareer-{safeStem}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.html");
        var encoded = WebUtility.HtmlEncode(portal.ArticleHtml).Replace("\r\n", "\n").Replace("\n\n", "<br><br>").Replace("\n", "<br>");
        var title = WebUtility.HtmlEncode(portal.Title);
        var standfirst = WebUtility.HtmlEncode(portal.Standfirst);
        var headline = WebUtility.HtmlEncode(portal.Headlines.FirstOrDefault() ?? "Ultime notizie dal paddock");
        var html = "<!doctype html><html lang='it'><meta charset='utf-8'><title>CorsaCareer Motorsport — Edizione digitale</title>" +
            "<style>body{margin:0;background:#f3f4f6;color:#171b22;font:15px Segoe UI,Arial}.top{height:7px;background:#d7193f}.back{position:fixed;top:14px;right:16px;z-index:9;background:#d7193f;color:#fff;border:0;border-radius:3px;padding:10px 16px;font:600 13px Segoe UI,Arial;cursor:pointer;box-shadow:0 2px 10px rgba(0,0,0,.25)}.back:hover{background:#b0142f}.wrap{max-width:1180px;margin:auto;background:#fff;min-height:100vh}.mast{display:flex;align-items:center;justify-content:space-between;padding:28px 36px 22px;border-bottom:1px solid #ddd}.logo{font-size:38px;font-weight:900;letter-spacing:-2px}.logo b{color:#d7193f}.edition{color:#777;text-transform:uppercase;font-size:12px;letter-spacing:1px}.nav{background:#d7193f;color:#fff;padding:12px 36px;font-weight:700;word-spacing:18px}.ticker{background:#252b34;color:#fff;padding:10px 36px}.grid{display:grid;grid-template-columns:minmax(0,2fr) minmax(260px,1fr);gap:28px;padding:28px 36px}.hero{background:linear-gradient(135deg,#303846,#11151b);min-height:180px;padding:30px;color:#fff;display:flex;flex-direction:column;justify-content:end}.tag{color:#ffd34d;font-weight:800;text-transform:uppercase}.hero h1{font-size:34px;line-height:1.05;margin:10px 0}.side{border-top:5px solid #d7193f}.side h2{margin:12px 0;font-size:22px}.side article{border-bottom:1px solid #ddd;padding:13px 0}.side strong{display:block}.story{background:#fff;border-left:5px solid #f2b400;padding:24px;line-height:1.75;font-size:18px}.controls{background:#20252d;color:#fff;padding:18px;margin-top:18px}.controls button{background:#d7193f;color:#fff;border:0;padding:10px 16px;margin:8px 8px 8px 0;font-weight:700;cursor:pointer}.controls select{padding:9px;min-width:290px}.note{color:#aeb6c2;font-size:12px;margin-top:8px}.footer{border-top:1px solid #ddd;margin:18px 36px;padding:18px 0;color:#777;font-size:12px}@media(max-width:760px){.grid{grid-template-columns:1fr;padding:18px}.mast{padding:20px}.nav,.ticker{padding-left:20px}.hero h1{font-size:26px}}</style>" +
            "<div class='top'></div>" + PortalFocusServer.ButtonHtml(PortalFocusUrl) + "<div class='wrap'><header class='mast'><div><div class='logo'><b>CORSA</b> CAREER</div><div class='edition'>Motorsport · carriera · paddock · archivio</div></div><div class='edition'>EDIZIONE DIGITALE<br>LIVE PADDOCK</div></header><nav class='nav'>HOME FORMULA GT TURISMO ENDURANCE MERCATO MEDIA</nav><div class='ticker'>ULTIM'ORA &nbsp; · &nbsp; " + headline + "</div><main class='grid'><section><div class='hero'><div class='tag'>Servizio speciale · Rubrica TV</div><h1>" + title + "</h1><div>" + standfirst + "</div></div><div class='controls'><b>ASCOLTA IL SERVIZIO</b><br><select id='v'></select><br><button onclick='speechSynthesis.cancel()'>■ FERMA</button><button onclick='play()'>▶ AVVIA / RIPRENDI</button><button onclick='preview()'>♬ PROVA VOCE</button><div class='note'>La pagina non avvia l'audio da sola. Voce predefinita: Google italiano (it-IT), se il browser la espone; in mancanza viene scelta la migliore voce it-IT disponibile. Velocit&agrave; di lettura " + NarrationVoiceCatalog.BrowserRateLiteral + "&times; la naturale.</div></div><article class='story' id='t'>" + encoded + "</article></section><aside class='side'><h2>ULTIME NOTIZIE</h2><article><strong>PADDOCK</strong>Il team prepara il prossimo test e aggiorna il dossier tecnico.</article><article><strong>MERCATO PILOTI</strong>Le prestazioni della pista influenzano le trattative.</article><article><strong>DATI E CLASSIFICHE</strong>Risultati importati soltanto da gare realmente concluse.</article><h2>IN EVIDENZA</h2><article><strong>ARCHIVIO CARRIERA</strong>Ogni articolo resta collegato alla data narrativa e agli eventi salvati.</article></aside></main><footer class='footer'>CorsaCareer Motorsport è una pubblicazione fittizia locale. Grafica e testi originali, senza affiliazione a testate reali.</footer></div><script>" + PortalFocusServer.ButtonScript(PortalFocusUrl) + "const t=document.getElementById('t').innerText,v=document.getElementById('v');let u=[];function rank(x){const it=x.lang.toLowerCase()==='it-it',g=/google/i.test(x.name);return g&&it?0:it?1:g?2:3}function load(){u=speechSynthesis.getVoices().filter(x=>/^it(-|_)/i.test(x.lang));u.sort((a,b)=>rank(a)-rank(b)||a.name.localeCompare(b.name));v.innerHTML=u.length?u.map((x,i)=>`<option value='${i}'>${x.name} — ${x.lang}</option>`).join(''):'<option>Nessuna voce italiana esposta</option>'}function play(){speechSynthesis.cancel();const a=new SpeechSynthesisUtterance(t),x=u[Number(v.value)];if(x)a.voice=x;a.lang='it-IT';a.rate=" + NarrationVoiceCatalog.BrowserRateLiteral + ";a.pitch=1;speechSynthesis.speak(a)}function preview(){speechSynthesis.cancel();const a=new SpeechSynthesisUtterance('Questa è una prova della voce italiana selezionata.');const x=u[Number(v.value)];if(x)a.voice=x;a.lang='it-IT';a.rate=" + NarrationVoiceCatalog.BrowserRateLiteral + ";speechSynthesis.speak(a)}load();speechSynthesis.onvoiceschanged=load;</script></html>";
        File.WriteAllText(path, html, Encoding.UTF8);
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        MaximizeBrowserWindow("CorsaCareer Motorsport — Edizione digitale");
    }

    private static void OpenBrowserNarration(string text)
    {
        OpenBrowserPortal(new BrowserPortalContent("Servizio speciale CorsaCareer", "Edizione digitale dal paddock", text, text, ["Ultime notizie dal paddock"]));
    }

    public static void OpenCareerLaunchStories(CareerState career, bool replay = false)
    {
        if (career == null || (career.LaunchStoriesOpened && !replay)) return;
        var date = career.StoryDate.ToString("d MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
        var car = string.IsNullOrWhiteSpace(career.Car) ? "la vettura di valutazione disponibile" : career.Car;
        var pilot = career.Driver;
        var stories = new[]
        {
            new BrowserPortalContent(
                $"Chi è {pilot}? Il paddock apre il dossier del nuovo rookie",
                $"Edizione inaugurale · {date} · presentazione del pilota",
                $"Il nome di {pilot} entra oggi nel notiziario CorsaCareer. Non arriva con un titolo già scritto né con un contratto garantito: arriva con una reputazione da costruire. Gli osservatori lo hanno seguito nelle categorie minori, dove il talento si misura quando la macchina non è la migliore e ogni errore pesa sul budget.\n\nIl primo capitolo è quello di un pilota quasi sconosciuto, chiamato a trasformare velocità, pazienza e professionalità in una prova concreta. La redazione seguirà ogni passo: test, colloqui, offerte e risultati reali.",
                $"È {date}. Il paddock presenta {pilot}, un rookie senza contratto che deve costruire da zero la propria reputazione. Il talento mostrato nelle categorie minori ha attirato gli osservatori, ma ora servono fatti e risultati.", ["DOSSIER PILOTA · La nuova firma del paddock"]),
            new BrowserPortalContent(
                $"Dalle piste minori al grande salto: la strada ancora da percorrere per {pilot}",
                "Retroscena · kart, formule propedeutiche e occasioni che non si possono sprecare",
                $"Prima di questa chiamata ci sono state piste corte, trasferte pagate contando ogni gomma e gare in cui {pilot} ha dovuto imparare a fare la differenza nei dettagli. Il paddock ricorda i sorpassi preparati con pazienza, i giri veloci arrivati quando la pista si raffreddava e la capacità di restare lucido anche dopo un weekend difficile.\n\nMa il passato non vale un sedile: vale soltanto un invito. La scuderia vuole capire se quel talento può diventare ritmo, costanza e lavoro con gli ingegneri. Il prossimo test sarà il primo vero esame pubblico.",
                $"La redazione ricostruisce il percorso minore di {pilot}: talento, sacrifici e poche occasioni. Il passato apre una porta, ma non assegna punti né contratti.", ["CATEGORIE MINORI · Il talento deve diventare mestiere"]),
            new BrowserPortalContent(
                $"Primo test per {pilot}: ecco cosa deve dimostrare",
                $"Rookie evaluation · {car} · obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}",
                $"Il primo test è stato preparato su {car}. Il riferimento indicato dal team è {FormatLap(career.EvaluationTargetMilliseconds)}: non è una promessa di prestazione, ma una soglia tecnica da affrontare sulla pista reale.\n\n{pilot} dovrà dimostrare velocità sul giro, costanza nelle run, rispetto della vettura e capacità di riferire agli ingegneri ciò che accade. Un tempo sufficiente può aprire un secondo test o una gara minore; una prova incompleta non cancella il progetto, ma rimanda il verdetto. Il risultato verrà acquisito soltanto dal referto realmente prodotto da Assetto Corsa.",
                $"Primo esame per {pilot}: {car}, obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}. Servono velocità, costanza e feedback tecnico. Il verdetto arriverà solo dal test reale in Assetto Corsa.", ["ROOKIE EVALUATION · La prima soglia del percorso"])
        };
        OpenGuidedStory(stories, career);
    }

    private static void OpenGuidedStory(IReadOnlyList<BrowserPortalContent> stories, CareerState career)
    {
        if (stories.Count == 0) return;
        var path = Path.Combine(Path.GetTempPath(), $"corsacareer-storia-{DateTime.Now:yyyyMMdd-HHmmss-fff}.html");
        var chapters = stories.Select((story, index) => new
        {
            number = index + 1,
            title = story.Title,
            standfirst = story.Standfirst,
            body = story.ArticleHtml,
            audio = story.AudioText,
            art = ToDataUri(LaunchArtwork(career, index)),
            kicker = index switch { 0 => "IL PILOTA", 1 => "LE ORIGINI", _ => "IL PRIMO ESAME" }
        }).ToArray();
        var json = JsonSerializer.Serialize(chapters).Replace("</script", "<\\/script", StringComparison.OrdinalIgnoreCase);
        var driver = WebUtility.HtmlEncode(career.Driver);
        var html = "<!doctype html><html lang='it'><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>CorsaCareer — Inizia la storia</title>" +
            "<style>*{box-sizing:border-box}body{margin:0;background:#0d1016;color:#eef1f6;font:16px 'Segoe UI',Arial}.top{height:8px;background:linear-gradient(90deg,#e71946,#f4b63e,#e71946)}.back{position:fixed;top:14px;right:16px;z-index:9;background:#e71946;color:#fff;border:0;border-radius:4px;padding:11px 17px;font:700 13px 'Segoe UI',Arial;cursor:pointer;box-shadow:0 4px 16px #0008}.back:hover{background:#ff3158}.mast{background:#151a23;border-bottom:1px solid #343b49;padding:25px 5vw;display:flex;justify-content:space-between;align-items:center}.logo{font-size:34px;font-weight:900;letter-spacing:-1.5px}.logo b{color:#e71946}.sub{color:#aab3c2;font-size:12px;text-transform:uppercase;letter-spacing:1px}.progress{height:5px;background:#252c38}.bar{height:100%;width:33%;background:linear-gradient(90deg,#e71946,#f4b63e);transition:width .35s}.layout{max-width:1320px;margin:30px auto;display:grid;grid-template-columns:260px minmax(0,1fr);gap:24px;padding:0 18px}.rail,.page{background:#151a23;border:1px solid #343b49;border-radius:6px;box-shadow:0 8px 35px #0005}.rail{padding:22px;height:max-content;position:sticky;top:18px}.rail h3{margin:0 0 16px;color:#f4b63e;letter-spacing:1px}.step{display:block;width:100%;text-align:left;border:0;border-left:3px solid #3b4350;background:transparent;padding:13px;margin:0 0 7px;color:#aab3c2;cursor:pointer}.step.on{border-left-color:#e71946;background:#202735;color:#fff;font-weight:700}.page{min-height:690px;overflow:hidden}.hero{background:linear-gradient(135deg,#273246,#10141b);color:#fff;padding:45px 50px}.kicker{color:#f4b63e;font-weight:800;letter-spacing:1.5px}.hero h1{font:700 42px/1.06 Georgia,serif;margin:13px 0}.stand{font:19px/1.5 Georgia,serif;color:#d6dbe3}.cover{height:310px;background:#080b10;overflow:hidden}.cover img{width:100%;height:100%;object-fit:cover;display:block;filter:saturate(.9) contrast(1.05)}.article{padding:36px 50px;font:19px/1.8 Georgia,serif;color:#eef1f6}.article p:first-child:first-letter{float:left;font-size:60px;line-height:.85;padding:7px 8px 0 0;color:#e71946;font-weight:700}.audio{margin:0 50px 28px;background:#202735;color:#fff;padding:17px;border-left:4px solid #f4b63e}.audio select,.audio button{padding:9px 12px;margin:6px 6px 0 0}.audio button{border:0;background:#e71946;color:#fff;font-weight:700;cursor:pointer}.nav{display:flex;justify-content:space-between;align-items:center;padding:0 50px 35px}.nav button{border:0;padding:13px 20px;font-weight:800;cursor:pointer}.prev{background:#303846;color:#fff}.next{background:#e71946;color:#fff}.counter{color:#aab3c2}.hint{background:#332c17;border-left:4px solid #f4b63e;padding:13px 16px;margin:0 50px 24px;color:#f4d98a}@media(max-width:780px){.layout{grid-template-columns:1fr}.rail{position:static}.hero,.article{padding:28px}.audio,.hint{margin:0 28px 22px}.nav{padding:0 28px 28px}.hero h1{font-size:31px}}</style>" +
            "<div class='top'></div>" + PortalFocusServer.ButtonHtml(PortalFocusUrl) + "<header class='mast'><div><div class='logo'><b>CORSA</b> CAREER</div><div class='sub'>La storia di " + driver + "</div></div><div class='sub'>NUMERO ZERO · PERCORSO GUIDATO</div></header><div class='progress'><div class='bar' id='bar'></div></div><main class='layout'><aside class='rail'><h3>IL TUO PERCORSO</h3><div id='steps'></div><p class='sub'>Procedi un capitolo alla volta. Puoi tornare indietro quando vuoi.</p></aside><section class='page'><div class='cover'><img id='art' alt='Illustrazione narrativa del capitolo'></div><div class='hero'><div class='kicker' id='kicker'></div><h1 id='title'></h1><div class='stand' id='stand'></div></div><div class='article' id='article'></div><div class='hint' id='hint'></div><div class='audio'><b>ASCOLTA QUESTO CAPITOLO</b><br><select id='voice'></select><br><button onclick='speak()'>▶ ASCOLTA</button><button onclick='speechSynthesis.cancel()'>■ FERMA</button></div><div class='nav'><button class='prev' id='prev' onclick='move(-1)'>← INDIETRO</button><span class='counter' id='counter'></span><button class='next' id='next' onclick='move(1)'>AVANTI →</button></div></section></main>" +
            "<script>" + PortalFocusServer.ButtonScript(PortalFocusUrl) + "const chapters=" + json + ";let current=0,voices=[];const q=id=>document.getElementById(id);function render(){const c=chapters[current];q('kicker').textContent=c.kicker;q('title').textContent=c.title;q('stand').textContent=c.standfirst;q('art').src=c.art;q('article').innerHTML='';c.body.split(/\\n\\n+/).forEach(x=>{const p=document.createElement('p');p.textContent=x;q('article').appendChild(p)});q('counter').textContent=`CAPITOLO ${current+1} DI ${chapters.length}`;q('bar').style.width=((current+1)/chapters.length*100)+'%';q('prev').disabled=current===0;q('next').textContent=current===chapters.length-1?'ENTRA NELLA CARRIERA →':'AVANTI →';q('hint').textContent=current===chapters.length-1?'Ultimo capitolo: premi BARRA SPAZIATRICE per entrare nella carriera.':'BARRA SPAZIATRICE per il capitolo successivo · frecce per muoverti · o scegli un capitolo dall\\'elenco a sinistra.';document.querySelectorAll('.step').forEach((x,i)=>x.classList.toggle('on',i===current));scrollTo({top:0,behavior:'smooth'});speechSynthesis.cancel()}function entra(){backToPortal()}function move(d){if(current===chapters.length-1&&d>0){entra();return}current=Math.max(0,Math.min(chapters.length-1,current+d));render()}function buildSteps(){q('steps').innerHTML='';chapters.forEach((c,i)=>{const b=document.createElement('button');b.className='step';b.textContent=`${i+1}. ${c.kicker}`;b.onclick=()=>{current=i;render()};q('steps').appendChild(b)})}function rank(x){const it=x.lang.toLowerCase()==='it-it',g=/google/i.test(x.name);return g&&it?0:it?1:g?2:3}function loadVoices(){voices=speechSynthesis.getVoices().filter(x=>/^it(-|_)/i.test(x.lang));voices.sort((a,b)=>rank(a)-rank(b)||a.name.localeCompare(b.name));q('voice').innerHTML=voices.length?voices.map((x,i)=>`<option value='${i}'>${x.name} — ${x.lang}</option>`).join(''):'<option>Nessuna voce italiana</option>'}function speak(){speechSynthesis.cancel();const u=new SpeechSynthesisUtterance(chapters[current].audio),v=voices[Number(q('voice').value)];if(v)u.voice=v;u.lang='it-IT';u.rate=" + NarrationVoiceCatalog.BrowserRateLiteral + ";speechSynthesis.speak(u)}document.addEventListener('keydown',function(e){var t=(e.target&&e.target.tagName||'').toUpperCase();if(t==='SELECT'||t==='INPUT'||t==='TEXTAREA')return;if(e.code==='Space'||e.key===' '||e.key==='Enter'){e.preventDefault();move(1)}else if(e.key==='ArrowRight'){e.preventDefault();move(1)}else if(e.key==='ArrowLeft'){e.preventDefault();move(-1)}});buildSteps();loadVoices();speechSynthesis.onvoiceschanged=loadVoices;render()</script></html>";
        File.WriteAllText(path, html, Encoding.UTF8);
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        // Bloccante: questa chiamata avviene prima che la finestra principale
        // sia mai stata mostrata, quindi non c'è nulla di visibile da bloccare
        // — e aspettare per davvero è l'unico modo per evitare che il portale,
        // mostrandosi un istante dopo, rubi il fuoco al browser appena aperto.
        MaximizeBrowserWindow("CorsaCareer — Inizia la storia", blocking: true);
    }

    private static string LaunchArtwork(CareerState career, int chapter)
    {
        // L'introduzione usa tavole principali coerenti con la fase narrativa:
        // niente rotazione casuale e nessun catalogo parallelo.
        var file = chapter switch
        {
            0 => "manga-kart-scrapyard-four-stroke-generator-first-test.png",
            1 => "manga-kart-first-rain-test-repaired-chassis.png",
            _ => "manga-kart-first-team-choice-rei-nobu-genji-paddock.png"
        };
        var selected = AssetPaths.File(file);
        if (File.Exists(selected)) return selected;
        return AssetPaths.File("manga-02-first-test.png");
    }

    private static string ToDataUri(string path)
    {
        try { return File.Exists(path) ? "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(path)) : ""; }
        catch { return ""; }
    }

    private static string FormatLap(int milliseconds) => $"{milliseconds / 60000:00}:{milliseconds / 1000 % 60:00}.{milliseconds % 1000:000}";

    public static void Stop()
    {
        lock (Gate)
        {
            try { if (active is { HasExited: false }) active.Kill(true); } catch { }
            active = null;
        }
    }

    public static void ExportWav(string text, string outputPath, string? preferredVoice = null)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(outputPath)) return;
        Start(text, outputPath, false, preferredVoice);
    }

    private static void Start(string text, string? outputPath, bool trackForStop, string? preferredVoice)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Stop();
        static string Ps(string value) => "'" + value.Replace("'", "''") + "'";
        var output = string.IsNullOrWhiteSpace(outputPath) ? "" : $"$s.SetOutputToWaveFile({Ps(outputPath)});";
        var preferred = string.IsNullOrWhiteSpace(preferredVoice) || preferredVoice.Equals("Automatica", StringComparison.OrdinalIgnoreCase) ? "" : preferredVoice.Trim();
        var command = $"Add-Type -AssemblyName System.Speech; $s=New-Object System.Speech.Synthesis.SpeechSynthesizer; $voices=$s.GetInstalledVoices() | Where-Object {{ $_.VoiceInfo.Culture.Name -like 'it-*' }}; $preferred={Ps(preferred)}; $voice=if ($preferred) {{ $voices | Where-Object {{ $_.VoiceInfo.Name -like ('*' + $preferred + '*') }} | Select-Object -First 1 }} else {{ $voices | Sort-Object {{ if ($_.VoiceInfo.Name -like '*Natural*' -or $_.VoiceInfo.Name -like '*Neural*' -or $_.VoiceInfo.Name -like '*Online*') {{ 0 }} elseif ($_.VoiceInfo.Name -like '*Cosimo*') {{ 1 }} elseif ($_.VoiceInfo.Name -like '*Elsa*Desktop*') {{ 2 }} else {{ 3 }} }} | Select-Object -First 1 }}; if ($voice) {{ $s.SelectVoice($voice.VoiceInfo.Name) }}; $s.Rate=-2; $s.Volume=100; $raw={Ps(text)}; $escaped=[System.Security.SecurityElement]::Escape($raw); $escaped=$escaped -replace '\\r?\\n\\r?\\n','<break time=\"520ms\"/>'; $escaped=$escaped -replace '\\r?\\n','<break time=\"280ms\"/>'; $escaped=$escaped -replace '([.!?])\\s+','$1<break time=\"180ms\"/> '; $ssml=\"<speak version='1.0' xml:lang='it-IT'><prosody rate='{NarrationVoiceCatalog.WindowsProsodyRate}' pitch='-2%'>$escaped</prosody></speak>\"; {output}try {{ $s.SpeakSsml($ssml) }} catch {{ $s.Speak($raw) }}; $s.Dispose()";
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
        var process = Process.Start(new ProcessStartInfo { FileName = "powershell.exe", Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}", UseShellExecute = false, CreateNoWindow = true });
        if (trackForStop) lock (Gate) active = process;
    }

    private static string? FindFfplay()
    {
        var fromPath = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator)
            .Select(x => Path.Combine(x, "ffplay.exe")).FirstOrDefault(File.Exists);
        if (fromPath != null) return fromPath;
        return File.Exists(@"C:\ffmpeg\bin\ffplay.exe") ? @"C:\ffmpeg\bin\ffplay.exe" : null;
    }
}
