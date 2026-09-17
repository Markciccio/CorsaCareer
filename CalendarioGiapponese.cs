namespace CorsaCareer;

/// <summary>
/// Il calendario del paese in cui questa carriera si svolge.
///
/// Serviva a una cosa sola e importante: la prima prova della carriera cadeva
/// il primo gennaio. In Giappone il capodanno e' la festa piu' importante
/// dell'anno, i circuiti sono chiusi, le officine sono chiuse, e un ragazzo di
/// dodici anni e' a casa con la famiglia a mangiare l'osechi. Un programma che
/// fissa un test quel giorno perde credibilita' alla prima schermata, prima
/// ancora che qualcuno guidi.
///
/// Le feste nazionali sono sedici e non sono decorative: sono giorni in cui si
/// puo' correre, perche' non c'e' scuola. Contano quanto un sabato.
///
/// Dove la legge e' cambiata durante gli anni che una carriera attraversa, il
/// calendario cambia con lei: il compleanno dell'Imperatore era il 23 dicembre
/// fino all'abdicazione di Akihito e dal 2020 e' il 23 febbraio; la Festa
/// della montagna esiste solo dal 2016. Una carriera che comincia nel 2003 e
/// finisce nel 2030 li attraversa tutti e due.
/// </summary>
public static class CalendarioGiapponese
{
    /// <summary>
    /// I giorni in cui il paese si ferma davvero: non si corre e non si prova.
    ///
    /// Capodanno (oshogatsu) dal 29 dicembre al 3 gennaio, e l'Obon dal 13 al
    /// 16 agosto, quando si torna al paese d'origine per i defunti. Sono le due
    /// settimane in cui in Giappone non si fa niente che assomigli a lavoro.
    /// </summary>
    public static bool PaeseFermo(DateTime giorno)
    {
        var d = giorno.Date;
        if (d.Month == 12 && d.Day >= 29) return true;
        if (d.Month == 1 && d.Day <= 3) return true;
        if (d.Month == 8 && d.Day is >= 13 and <= 16) return true;
        return false;
    }

    /// <summary>Come si chiama la festa di oggi, se oggi e' una festa.</summary>
    public static string? Festa(DateTime giorno)
    {
        var d = giorno.Date;
        var anno = d.Year;

        if (d.Month == 1 && d.Day == 1) return "Capodanno";
        if (d.Month == 1 && d == LunediDelMese(anno, 1, 2)) return "Festa della maggiore età";
        if (d.Month == 2 && d.Day == 11) return "Fondazione dello Stato";
        if (anno >= 2020 && d.Month == 2 && d.Day == 23) return "Compleanno dell'Imperatore";
        if (d.Month == 3 && d.Day == EquinozioDiPrimavera(anno)) return "Equinozio di primavera";
        if (d.Month == 4 && d.Day == 29) return anno >= 2007 ? "Giorno dello Showa" : "Giorno del verde";
        if (d.Month == 5 && d.Day == 3) return "Festa della Costituzione";
        if (d.Month == 5 && d.Day == 4) return "Giorno del verde";
        if (d.Month == 5 && d.Day == 5) return "Festa dei bambini";
        if (d.Month == 7 && d == LunediDelMese(anno, 7, 3)) return "Festa del mare";
        if (anno >= 2016 && d.Month == 8 && d.Day == 11) return "Festa della montagna";
        if (d.Month == 9 && d == LunediDelMese(anno, 9, 3)) return "Rispetto per gli anziani";
        if (d.Month == 9 && d.Day == EquinozioDAutunno(anno)) return "Equinozio d'autunno";
        if (d.Month == 10 && d == LunediDelMese(anno, 10, 2)) return "Giornata dello sport";
        if (d.Month == 11 && d.Day == 3) return "Festa della cultura";
        if (d.Month == 11 && d.Day == 23) return "Festa del ringraziamento del lavoro";
        if (anno <= 2018 && d.Month == 12 && d.Day == 23) return "Compleanno dell'Imperatore";
        return null;
    }

    /// <summary>
    /// Vero se in questo giorno si puo' correre o provare.
    ///
    /// Sabato, domenica o festa nazionale — cioe' i giorni in cui uno studente
    /// e' libero dalle otto del mattino — e comunque mai dentro capodanno o
    /// l'Obon.
    /// </summary>
    public static bool GiornoLibero(DateTime giorno)
    {
        if (PaeseFermo(giorno)) return false;
        if (giorno.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return true;
        return Festa(giorno) != null;
    }

    /// <summary>
    /// Sposta una data in avanti fino al primo giorno in cui si puo' davvero
    /// scendere in pista.
    ///
    /// Sempre in avanti, mai indietro: un appuntamento non puo' spostarsi nel
    /// passato, e anticiparlo cambierebbe i conti gia' fatti da chi lo ha
    /// programmato. Il limite di ricerca e' largo: fra capodanno e i giorni
    /// feriali il salto piu' lungo possibile e' di una decina di giorni.
    /// </summary>
    public static DateTime ProssimoGiornoDaPista(DateTime giorno)
    {
        var d = giorno.Date;
        for (var passi = 0; passi < 30; passi++, d = d.AddDays(1))
            if (GiornoLibero(d)) return d;
        return giorno.Date;
    }

    // ------------------------------------------------------------- calcoli

    /// <summary>L'n-esimo lunedi' di un mese: e' cosi' che la legge fissa quattro feste.</summary>
    private static DateTime LunediDelMese(int anno, int mese, int quale)
    {
        var primo = new DateTime(anno, mese, 1);
        var scarto = ((int)DayOfWeek.Monday - (int)primo.DayOfWeek + 7) % 7;
        return primo.AddDays(scarto + 7 * (quale - 1));
    }

    /// <summary>
    /// Gli equinozi non hanno una data fissa: il Giappone li calcola ogni anno
    /// dall'osservazione astronomica e li pubblica l'anno prima. L'approssimazione
    /// qui usata e' quella comunemente adottata e sbaglia di un giorno solo ben
    /// oltre il secolo che una carriera puo' attraversare.
    /// </summary>
    private static int EquinozioDiPrimavera(int anno) =>
        (int)(20.8431 + 0.242194 * (anno - 1980) - (anno - 1980) / 4);

    private static int EquinozioDAutunno(int anno) =>
        (int)(23.2488 + 0.242194 * (anno - 1980) - (anno - 1980) / 4);
}
