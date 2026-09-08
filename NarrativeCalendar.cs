using System.Globalization;

namespace CorsaCareer;

/// <summary>
/// Date del calendario di campionato.
///
/// Prima le date dei round venivano calcolate a partire da <c>StoryDate</c>, che
/// però avanza a ogni gara registrata: l'intero calendario — comprese le date dei
/// round già disputati — si spostava in avanti dopo ogni risultato. L'ancora è
/// ora la data d'inizio stagione, che cambia solo al passaggio di stagione.
/// </summary>
public static class NarrativeCalendar
{
    public const int DaysBetweenRounds = 21;

    /// <summary>
    /// Anno d'inizio predefinito della carriera. Prima era il 1991, che però è
    /// anteriore alla quasi totalità dei contenuti installati: una carriera che
    /// parte lì racconta stagioni con vetture che non esistevano ancora. Il 2023
    /// lascia spazio a una carriera pluriennale e resta coerente con le auto
    /// moderne, mentre una vettura che dichiara il proprio anno continua a
    /// spostare l'inizio sull'epoca giusta.
    /// </summary>
    /// <summary>
    /// L'anno in cui comincia una carriera quando i contenuti non dicono niente.
    ///
    /// Normalmente non serve: la data di partenza la decide la vettura
    /// d'ingresso, cioe' l'anno in cui quel campionato esisteva davvero. Un kart
    /// del 1998 apre una carriera nel 1998, e da li' in poi tutte le vetture che
    /// si incontrano sono vetture vere di quell'epoca — senza bisogno di
    /// controllare niente, perche' e' l'inizio a essere al posto giusto.
    ///
    /// Questo numero e' solo il ripiego per i contenuti che l'anno non lo
    /// dichiarano: duemilacinque, l'epoca di Capeta, cosi' una carriera lunga
    /// vent'anni ci sta dentro tutta senza finire nel futuro.
    /// </summary>
    public const int DefaultStartYear = 2005;

    /// <summary>Prima domenica utile di marzo: apertura di stagione credibile.</summary>
    public static readonly DateTime DefaultSeasonStart = new(DefaultStartYear, 3, 13);

    /// <summary>
    /// Quanto passa fra la firma di un contratto e il primo round.
    ///
    /// Prima la stagione partiva il giorno stesso della firma: si accettava un
    /// sedile e il giorno dopo si era in griglia. Una squadra che ingaggia un
    /// pilota gli fa fare sedile, test e preparazione prima di schierarlo, e il
    /// calendario di un campionato è pubblicato con settimane di anticipo.
    /// </summary>
    public const int DaysFromSigningToFirstRound = 28;

    /// <summary>
    /// La domenica di gara: sposta in avanti una data qualunque fino al primo
    /// fine settimana utile. Le gare si corrono di domenica, non nel giorno in
    /// cui è stato premuto un pulsante.
    /// </summary>
    public static DateTime RaceWeekend(DateTime date)
    {
        var day = date.Date;
        var toSunday = ((int)DayOfWeek.Sunday - (int)day.DayOfWeek + 7) % 7;
        return day.AddDays(toSunday);
    }

    /// <summary>
    /// Inizio stagione a partire dal giorno della firma, arrotondato in avanti
    /// a scadenze regolari invece di cadere in un giorno qualsiasi.
    /// </summary>
    public static DateTime SeasonStartAfterSigning(DateTime signingDate)
    {
        var firma = signingDate == default ? DefaultSeasonStart : signingDate.Date;
        var primo = RaceWeekend(firma.AddDays(DaysFromSigningToFirstRound));
        // Un campionato non comincia a stagione finita.
        //
        // Chi firmava a settembre si vedeva generare un calendario che partiva
        // subito e finiva a gennaio o febbraio, con round in pieno inverno e
        // una «stagione» a cavallo di due anni. Se non c'è più spazio per
        // correre, il calendario è quello della primavera successiva: è la
        // pausa in cui, nella realtà, i contratti si firmano.
        // Serve spazio per un campionato, non per due gare: sotto le sei
        // domeniche utili non è una stagione, è una coda d'anno che finirebbe
        // comunque in inverno.
        var chiusura = LastRaceSundayOfSeason(primo.Year);
        var spazioMinimo = primo.AddDays(14 * 6);
        if (spazioMinimo <= chiusura) return primo;
        return RaceWeekend(new DateTime(primo.Year + 1, SeasonFirstMonth, 10));
    }

    /// <summary>Primo mese in cui si corre: la stagione apre in primavera.</summary>
    public const int SeasonFirstMonth = 3;

    /// <summary>Ultimo mese in cui si corre: dopo novembre il paddock chiude.</summary>
    public const int SeasonLastMonth = 11;

    /// <summary>
    /// La data di un round, con il calendario che sta dentro la propria stagione.
    ///
    /// I round erano distanziati di ventuno giorni fissi: un campionato da
    /// dodici appuntamenti finiva a gennaio dell'anno dopo, e nel banco di
    /// prova si correvano round il 14 dicembre e il 25 gennaio, sotto la neve,
    /// mentre la «stagione 1» durava diciassette mesi. Un campionato vero
    /// comincia in primavera e si chiude entro novembre: il passo fra un round
    /// e l'altro si adatta a quanti sono, fra due e tre settimane.
    /// </summary>
    public static DateTime RoundDate(int index, DateTime seasonStart, int roundsInSeason)
    {
        var start = seasonStart == default ? DefaultSeasonStart : seasonStart.Date;
        var rounds = Math.Max(1, roundsInSeason);
        if (rounds == 1) return RaceWeekend(start);

        // L'ultima domenica utile della stagione, nell'anno in cui si è aperta.
        var chiusura = LastRaceSundayOfSeason(start.Year);
        if (chiusura <= start) chiusura = start.AddDays(DaysBetweenRounds * (rounds - 1));
        // Sotto le due settimane si arriva a correre due domeniche di fila: è il
        // limite oltre il quale un calendario smette di essere credibile. Se
        // nemmeno così la stagione ci sta, l'ultimo round non va comunque oltre
        // la chiusura — meglio due gare ravvicinate che una gara a dicembre.
        var passo = Math.Clamp((int)((chiusura - start).TotalDays / (rounds - 1)), 14, DaysBetweenRounds);
        var data = RaceWeekend(start.AddDays(Math.Max(0, index) * passo));
        return data > chiusura ? chiusura : data;
    }

    /// <summary>
    /// Vecchia forma, per i punti che non sanno quanti round ha la stagione:
    /// mantiene il passo di tre settimane.
    /// </summary>
    public static DateTime RoundDate(int index, DateTime seasonStart)
    {
        var start = seasonStart == default ? DefaultSeasonStart : seasonStart.Date;
        return start.AddDays(Math.Max(0, index) * DaysBetweenRounds);
    }

    /// <summary>L'ultima domenica di gara di una stagione.</summary>
    public static DateTime LastRaceSundayOfSeason(int year)
    {
        var ultimo = new DateTime(year, SeasonLastMonth, DateTime.DaysInMonth(year, SeasonLastMonth));
        var indietro = ((int)ultimo.DayOfWeek - (int)DayOfWeek.Sunday + 7) % 7;
        return ultimo.AddDays(-indietro);
    }

    public static string Format(DateTime date) => date.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));

    public static string DateForRound(int index, DateTime seasonStart) => Format(RoundDate(index, seasonStart));

    public static string DateForRound(int index) => RoundDate(index, DefaultSeasonStart).ToString("dd MMMM", CultureInfo.GetCultureInfo("it-IT"));

    /// <summary>
    /// Inizio della stagione successiva: un anno dopo, ma sempre dopo l'ultima
    /// gara realmente disputata, così un calendario lungo non genera date che
    /// tornano indietro nel tempo.
    /// </summary>
    public static DateTime NextSeasonStart(DateTime currentSeasonStart, int roundsInSeason)
    {
        var start = currentSeasonStart == default ? DefaultSeasonStart : currentSeasonStart.Date;
        var lastRound = RoundDate(Math.Max(0, roundsInSeason - 1), start, roundsInSeason);
        // La stagione nuova apre in primavera, non «un anno dopo l'ultima
        // firma»: era il motivo per cui il campionato scivolava di qualche
        // settimana a ogni passaggio e dopo cinque stagioni si correva in
        // estate piena o a dicembre.
        var apertura = RaceWeekend(new DateTime(lastRound.Year, SeasonFirstMonth, 10));
        while (apertura <= lastRound.AddDays(28)) apertura = RaceWeekend(new DateTime(apertura.Year + 1, SeasonFirstMonth, 10));
        return apertura;
    }

    /// <summary>
    /// Data narrativa del diario dopo un weekend concluso: il giorno successivo
    /// alla gara, quando la redazione pubblica il servizio.
    /// </summary>
    public static DateTime DayAfterRound(int index, DateTime seasonStart) => RoundDate(index, seasonStart).AddDays(1);

    /// <summary>
    /// Ricava l'inizio stagione per una carriera salvata prima dell'introduzione
    /// dell'ancora esplicita, senza spostare gare già archiviate.
    /// </summary>
    public static DateTime InferSeasonStart(IEnumerable<DateTime> seasonRaceDates, DateTime storyDate)
    {
        var dates = seasonRaceDates.Where(x => x != default).OrderBy(x => x).ToList();
        if (dates.Count > 0) return dates[0].Date;
        return storyDate == default ? DefaultSeasonStart : storyDate.Date;
    }
}
