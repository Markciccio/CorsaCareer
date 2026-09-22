namespace CorsaCareer;

/// <summary>
/// La scuola come vincolo della carriera, non come decorazione.
///
/// Un pilota di dodici anni ha due lavori e il tempo per uno solo. Il programma
/// lo diceva soltanto a giugno, con la bocciatura; nel frattempo la scuola non
/// si vedeva da nessuna parte e non toglieva niente a nessuno.
///
/// Adesso c'è un livello, e non sale da solo: ogni giorno di lezione il
/// programma va avanti e lascia indietro chi non lo segue, quindi il livello
/// scende. Studiare lo rialza. È l'unico parametro della carriera che cala
/// stando fermi, ed è giusto che sia questo.
///
/// Sotto la soglia del divieto non si scende in pista. Non è il gioco che
/// punisce: è che a un ragazzo di dodici anni con quel quadrimestre in mano
/// nessuno, in casa, firma l'iscrizione alla gara della domenica.
///
/// Finita la scuola il vincolo sparisce del tutto: il livello va al massimo e
/// non lo si guarda più. Un pilota di diciotto anni ha altri problemi.
/// </summary>
public static class Scuola
{
    /// <summary>Sotto questo, a giugno si ripete l'anno.</summary>
    public const int SogliaDiPromozione = 40;

    /// <summary>Sotto questo non si scende in pista finché non si risale.</summary>
    public const int SogliaDiDivieto = 25;

    /// <summary>
    /// L'età in cui la scuola dell'obbligo è finita e sparisce dalla carriera.
    ///
    /// In Giappone l'obbligo scolastico (gimu kyoiku) copre elementari e medie
    /// e si chiude a quindici anni, alla fine del terzo anno di chuugakkou. Il
    /// liceo esiste ma non è obbligatorio, e un ragazzo che a quindici anni ha
    /// un sedile sceglie il sedile.
    ///
    /// Da qui in avanti il livello non si mostra più e non vincola più niente:
    /// non è che il pilota diventa bravo a scuola, è che la scuola smette di
    /// essere un problema di questa carriera.
    /// </summary>
    public const int EtaDiFine = 16;

    /// <summary>
    /// Quanto costa ogni giorno di lezione, e quanto rende un pomeriggio sui
    /// libri. Sono i due numeri che decidono se la scuola e' un vincolo o un
    /// fastidio.
    ///
    /// Il conto: l'anno scolastico giapponese ha circa duecento giorni di
    /// lezione, quindi due punti al giorno fanno quaranta al mese. Un pomeriggio
    /// di studio ne rende dieci, cioe' servono quattro pomeriggi al mese su
    /// circa novanta fasce libere — il quattro per cento del tempo. Poco, ma
    /// non zero, ed e' esattamente il peso giusto: la scuola non deve essere il
    /// gioco, deve essere la cosa che non puoi ignorare del tutto.
    ///
    /// Con tre al giorno il livello crollava da settanta a zero in un mese e
    /// mezzo, e il pannello avrebbe proposto i libri tutti i santi giorni.
    /// </summary>
    public const int CaloGiornaliero = 1;
    public const int Studiare = 10;

    /// <summary>
    /// In che classe è il pilota, con il nome giapponese.
    ///
    /// Il sistema giapponese è elementari (shougakkou, 6 anni), medie
    /// (chuugakkou, 3 anni, dai dodici ai quindici) e liceo (koukou, 3 anni,
    /// non obbligatorio). Un ragazzo che comincia a correre a dodici anni è in
    /// prima media, ed è esattamente il punto in cui le due vite cominciano a
    /// litigare per gli stessi pomeriggi.
    ///
    /// L'anno scolastico giapponese comincia ad aprile, non a settembre: chi
    /// fa i conti con settembre sbaglia di sei mesi tutte le volte.
    /// </summary>
    public static string Classe(CareerState career)
    {
        var eta = Eta(career);
        // Da aprile in poi si è già nella classe dell'anno nuovo.
        var avanzato = career.StoryDate.Month >= 4;
        var anno = eta - 12 + (avanzato ? 1 : 0);
        return anno switch
        {
            <= 1 => "prima media (chuugaku 1)",
            2 => "seconda media (chuugaku 2)",
            3 => "terza media (chuugaku 3)",
            4 => "primo anno di liceo (koukou 1)",
            5 => "secondo anno di liceo (koukou 2)",
            _ => "terzo anno di liceo (koukou 3)"
        };
    }

    /// <summary>
    /// Le materie delle medie giapponesi, quelle vere: giapponese, matematica,
    /// scienze, studi sociali, inglese, piu' musica, arte, tecnica ed
    /// educazione fisica. E dotoku, l'educazione morale, che in Giappone e'
    /// una materia con il suo orario.
    /// </summary>
    public static readonly string[] Materie =
    [
        "giapponese", "matematica", "scienze", "studi sociali", "inglese",
        "educazione morale", "tecnica", "musica", "arte"
    ];

    public static string Materia(int seme) => Materie[Math.Abs(seme) % Materie.Length];

    public static int Eta(CareerState career) =>
        career.BirthYear <= 0 ? 12 : Math.Max(10, career.StoryDate.Year - career.BirthYear);

    /// <summary>Vero finché la scuola è un problema di questo pilota.</summary>
    public static bool Riguarda(CareerState career) => Eta(career) < EtaDiFine;

    /// <summary>
    /// Il livello, da 0 a 100. Chi ha finito la scuola sta al massimo: non è un
    /// premio, è il modo di dire che quel vincolo non esiste più.
    /// </summary>
    public static int Livello(CareerState career) =>
        Riguarda(career) ? Math.Clamp(career.SchoolPerformance, 0, 100) : 100;

    /// <summary>Vero se oggi la scuola impedisce di scendere in pista.</summary>
    public static bool Vieta(CareerState career) =>
        Riguarda(career) && Livello(career) < SogliaDiDivieto;

    /// <summary>Vero se il livello è abbastanza basso da dover correre ai ripari.</summary>
    public static bool ARischio(CareerState career) =>
        Riguarda(career) && Livello(career) < SogliaDiPromozione;

    /// <summary>
    /// Come si legge lo stato, in una riga. Vuota quando la scuola dell'obbligo
    /// è finita: non si mostra un parametro che non vincola più niente.
    /// </summary>
    public static string Etichetta(CareerState career)
    {
        if (!Riguarda(career)) return "";
        var livello = Livello(career);
        if (livello < SogliaDiDivieto) return $"Scuola {livello}/100 · SOSPESO DALLE GARE";
        if (livello < SogliaDiPromozione) return $"Scuola {livello}/100 · a rischio bocciatura";
        return $"Scuola {livello}/100 · in regola";
    }

    /// <summary>
    /// Perché oggi non si corre. Va detto per esteso: un divieto che non
    /// spiega se stesso è solo un pulsante che non funziona.
    /// </summary>
    public static string Divieto(CareerState career) =>
        $"{career.Driver} è a {Livello(career)} su 100 a scuola, sotto la soglia di {SogliaDiDivieto}.\n\n"
        + "In casa non firmano niente finché la situazione non migliora: niente prove, niente gare, niente giri liberi.\n\n"
        + $"Si risale studiando — ogni pomeriggio sui libri vale {Studiare} punti — e il pannello della giornata "
        + "te lo propone finché sei sotto. Oppure si salta l'appuntamento, e quello che costa è già scritto.";

    /// <summary>
    /// Il prezzo di una giornata di lezione, pagato una volta sola per giorno.
    ///
    /// Non è una punizione per essere andato a scuola: è il programma che
    /// avanza. Chi segue e fa i compiti resta in pari, chi passa tutti i
    /// pomeriggi in pista resta indietro — che è esattamente la scelta che
    /// questa carriera deve far pesare.
    /// </summary>
    /// <summary>
    /// L'inizio dell'anno scolastico, ad aprile.
    ///
    /// Si dice una volta l'anno in che classe si e' finiti: e' il modo in cui
    /// una carriera lunga fa sentire il tempo che passa anche fuori dalla
    /// pista. In Giappone l'anno comincia ad aprile, insieme ai ciliegi.
    /// </summary>
    public static void AnnunciaLAnnoScolastico(CareerState career)
    {
        if (!Riguarda(career)) return;
        var oggi = career.StoryDate.Date;
        if (oggi.Month != 4 || oggi.Day > 12) return;
        if (career.LastSchoolYearAnnounced >= oggi.Year) return;
        career.LastSchoolYearAnnounced = oggi.Year;
        var riga = $"Comincia l'anno scolastico: {career.Driver} entra in {Classe(career)}, "
                   + $"a {Eta(career)} anni, con {Livello(career)} su 100 di preparazione.";
        career.News.Add(riga);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = oggi, Type = "SCHOOL_YEAR",
            Headline = riga, Importance = 34
        });
    }

    public static void PassaUnGiornoDiLezione(CareerState career)
    {
        if (!Riguarda(career)) return;
        var oggi = career.StoryDate.Date;
        if (career.LastSchoolDayCharged.Date >= oggi) return;
        career.LastSchoolDayCharged = oggi;
        career.SchoolPerformance = Math.Clamp(career.SchoolPerformance - CaloGiornaliero, 0, 100);
    }
}
