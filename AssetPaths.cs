namespace CorsaCareer1991;

/// <summary>
/// Dove stanno le illustrazioni, in un posto solo.
///
/// Prima ogni schermata componeva il percorso a mano come
/// <c>AppContext.BaseDirectory + "assets"</c>, e il progetto copiava tutta la
/// cartella nell'output a ogni configurazione di compilazione: milleottocento
/// megabyte duplicati in <c>bin\Debug</c> e altrettanti in <c>bin\Release</c>,
/// cioè quasi quattro gigabyte di copie della stessa cosa.
///
/// Qui la cartella viene cercata una volta e poi ricordata: prima accanto
/// all'eseguibile — è il caso di un'installazione vera, dove le immagini sono
/// distribuite insieme al programma — e altrimenti risalendo le cartelle fino
/// a trovare quella del progetto. Così in sviluppo si legge direttamente dagli
/// originali, senza copiarne nemmeno uno.
///
/// C'è anche un ripiego sull'estensione: un'immagine convertita da PNG a JPEG
/// per occupare dieci volte meno continua a essere trovata con il vecchio nome,
/// che è scritto in decine di posti e nel catalogo delle tavole.
/// </summary>
public static class AssetPaths
{
    private static string? radice;

    /// <summary>
    /// La cartella delle illustrazioni. Si può forzare con la variabile
    /// d'ambiente <c>CORSACAREER_ASSETS</c>, che serve a chi tiene le immagini
    /// su un disco diverso da quello del programma.
    /// </summary>
    public static string Root => radice ??= Trova();

    private static string Trova()
    {
        var forzata = Environment.GetEnvironmentVariable("CORSACAREER_ASSETS");
        if (!string.IsNullOrWhiteSpace(forzata) && Directory.Exists(forzata)) return forzata;

        // Accanto all'eseguibile: è così in un'installazione distribuita.
        var accanto = Path.Combine(AppContext.BaseDirectory, "assets");
        if (Directory.Exists(accanto)) return accanto;

        // In sviluppo l'eseguibile sta in bin\Debug\net9.0-windows: si risale
        // finché non si trova la cartella del progetto. Il limite di risalita
        // evita di andare a spasso per il disco se qualcosa non torna.
        var cartella = new DirectoryInfo(AppContext.BaseDirectory);
        for (var passi = 0; passi < 6 && cartella != null; passi++, cartella = cartella.Parent)
        {
            var candidata = Path.Combine(cartella.FullName, "assets");
            if (Directory.Exists(candidata)) return candidata;
        }

        // Non trovata: si restituisce comunque il percorso accanto
        // all'eseguibile, così i controlli di esistenza falliscono in modo
        // pulito invece di sollevare eccezioni ovunque.
        return accanto;
    }

    /// <summary>
    /// Il percorso completo di un'immagine, con il ripiego sull'estensione.
    ///
    /// Restituisce sempre una stringa: chi chiama continua a verificare
    /// l'esistenza come faceva prima.
    /// </summary>
    public static string File(params string[] parti)
    {
        var pieno = Path.Combine([Root, .. parti]);
        if (System.IO.File.Exists(pieno)) return pieno;

        // Stesso nome, formato diverso: serve dopo la conversione in JPEG delle
        // tavole, che nel codice e nel catalogo sono citate ancora come .png.
        var senzaEstensione = Path.Combine(Path.GetDirectoryName(pieno) ?? Root, Path.GetFileNameWithoutExtension(pieno));
        foreach (var estensione in new[] { ".jpg", ".jpeg", ".png", ".webp" })
        {
            var alternativa = senzaEstensione + estensione;
            if (System.IO.File.Exists(alternativa)) return alternativa;
        }
        return pieno;
    }

    /// <summary>Vero se l'immagine esiste, in qualunque dei formati ammessi.</summary>
    public static bool Exists(params string[] parti) => System.IO.File.Exists(File(parti));
}
