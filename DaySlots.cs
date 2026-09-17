namespace CorsaCareer;

/// <summary>
/// Una fascia della giornata: dalle, alle, e che cosa ci sta dentro.
///
/// La giornata era un monte ore — «otto ore libere» — e un monte ore non si
/// vive: si consuma. Una giornata vera è fatta di fasce, e ogni fascia è una
/// scelta sola. Sapere che alle sedici si è liberi e alle venti no cambia
/// completamente il modo in cui si decide.
///
/// Gli orari sono quelli di uno studente giapponese: scuola dalle otto alle
/// tre e mezza, pranzo compreso, e il pomeriggio che comincia alle quattro —
/// l'ora in cui i compagni vanno al club sportivo e lui va al kartodromo.
/// </summary>
public sealed record FasciaDelGiorno(int Dalle, int Alle, bool Fissa, string Nome)
{
    public int Ore => Alle - Dalle;
    public string Orario => $"{Dalle:00}:00–{Alle:00}:00";
}

/// <summary>
/// Come è divisa la giornata, per il pilota e per Haru.
///
/// Due colonne separate perché sono due persone: quello che fa Haru un
/// pomeriggio non toglie niente al pilota, e viceversa. Erano due monte ore
/// che si guardavano da schermate diverse.
/// </summary>
public static class DaySlots
{
    /// <summary>Scuola giapponese: 8:00–15:30, pranzo compreso. Si arrotonda alle 15.</summary>
    public const int ScuolaDalle = 8;
    public const int ScuolaAlle = 15;

    /// <summary>
    /// Vero se oggi c'e' scuola.
    ///
    /// Non basta guardare il giorno della settimana: in Giappone le feste
    /// nazionali sono sedici, e in quei giorni un ragazzo e' libero dalle otto
    /// del mattino esattamente come di sabato. Senza questo controllo il
    /// programma mandava a scuola il giorno di Capodanno.
    /// </summary>
    private static bool Feriale(DateTime giorno) =>
        giorno.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
        && CalendarioGiapponese.Festa(giorno) == null
        && !CalendarioGiapponese.PaeseFermo(giorno);

    /// <summary>
    /// Le fasce del pilota.
    ///
    /// Nei giorni di scuola il pomeriggio comincia alle sedici — l'ora del club
    /// sportivo — e ci stanno tre fasce da due ore. Nel fine settimana la
    /// giornata è tutta sua, e ci stanno anche le cose lunghe: un turno di
    /// lavoro da quattro ore in un pomeriggio di scuola non ci sta, ed è giusto
    /// che non ci stia.
    ///
    /// Chi ripete l'anno perde la prima fascia del pomeriggio: il recupero
    /// finisce alle diciotto, non alle sedici.
    /// </summary>
    public static IReadOnlyList<FasciaDelGiorno> Pilota(DateTime giorno, int eta, bool ripetente)
    {
        if (eta <= 17 && Feriale(giorno))
        {
            var fine = ripetente ? ScuolaAlle + 3 : ScuolaAlle;
            var fasce = new List<FasciaDelGiorno>
            {
                new(ScuolaDalle, fine, true, ripetente ? "Scuola e recupero" : "Scuola")
            };
            if (!ripetente) fasce.Add(new FasciaDelGiorno(16, 18, false, ""));
            fasce.Add(new FasciaDelGiorno(18, 20, false, ""));
            fasce.Add(new FasciaDelGiorno(20, 22, false, ""));
            return fasce;
        }

        // Fine settimana, oppure il pilota ha finito la scuola: la giornata è
        // sua e le cose lunghe diventano possibili.
        return
        [
            new FasciaDelGiorno(9, 13, false, ""),
            new FasciaDelGiorno(14, 17, false, ""),
            new FasciaDelGiorno(17, 19, false, ""),
            new FasciaDelGiorno(20, 22, false, "")
        ];
    }

    /// <summary>
    /// Le fasce di Haru.
    ///
    /// Haru va a scuola come il pilota: quello che gli resta è un pomeriggio
    /// corto e un'ora di sera davanti al computer, che è quando tiene in piedi
    /// il sito e le pagine. Nel fine settimana gira di più, perché è quando i
    /// negozianti sono in negozio.
    /// </summary>
    public static IReadOnlyList<FasciaDelGiorno> Haru(DateTime giorno)
    {
        if (Feriale(giorno))
            return
            [
                new FasciaDelGiorno(16, 18, false, ""),
                new FasciaDelGiorno(21, 22, false, "")
            ];

        return
        [
            new FasciaDelGiorno(9, 12, false, ""),
            new FasciaDelGiorno(15, 17, false, ""),
            new FasciaDelGiorno(21, 22, false, "")
        ];
    }
}
