namespace CorsaCareer1991;

/// <summary>Un momento della gara che vale qualcosa oltre ai punti.</summary>
public sealed record RaceHighlight(
    string Id,
    string Title,
    string Story,
    int Fitness,
    int Influencer,
    int SportingPrestige,
    int Importance);

/// <summary>
/// Quello che una gara lascia, oltre al piazzamento.
///
/// Prima una gara muoveva punti, premio e reputazione, e basta: una rimonta di
/// dieci posizioni e un settimo posto tranquillo valevano identici. Ma le
/// carriere vere si ricordano per le gare storte finite bene e per quelle
/// vinte perse all'ultimo giro — ed è lì che si costruisce il seguito.
///
/// Qui la gara viene riletta cercando i fatti che la rendono una storia:
/// rimonte, esordi, ritiri che costano un campionato, giornate sotto la
/// pioggia. Ogni fatto sposta forma fisica e livello influencer, non solo il
/// budget: una rimonta consuma il pilota e riempie i social, un ritiro al
/// primo giro non stanca nessuno e spegne l'entusiasmo.
/// </summary>
public static class RaceHighlights
{
    /// <param name="primaInCategoria">Vero se è la prima gara con questa vettura.</param>
    /// <param name="decisivaPerIlTitolo">Vero se questa gara chiudeva un campionato ancora aperto.</param>
    public static List<RaceHighlight> For(RaceHistoryEntry gara, bool primaInCategoria, bool decisivaPerIlTitolo)
    {
        var trovati = new List<RaceHighlight>();
        if (gara == null) return trovati;

        var partenti = Math.Max(1, gara.Classification?.Count ?? 1);
        var partenza = gara.StartingPosition > 0 ? gara.StartingPosition : partenti;
        var guadagnate = gara.Dnf ? 0 : partenza - gara.Position;
        var bagnato = (gara.WeatherLabel ?? "").Contains("pioggia", StringComparison.OrdinalIgnoreCase)
                      || (gara.WeatherLabel ?? "").Contains("bagnat", StringComparison.OrdinalIgnoreCase);
        var vittoria = !gara.Dnf && gara.Position == 1;
        var podio = !gara.Dnf && gara.Position <= 3;

        // --- le rimonte: costano fatica e valgono pubblico
        if (guadagnate >= 10)
            trovati.Add(new("rimonta-epica", "Rimonta da non credere",
                $"Partito {partenza}°, arrivato {gara.Position}°: {guadagnate} posizioni recuperate. "
                + "Il video del sorpasso all'ultima curva gira più della classifica.",
                Fitness: -6, Influencer: +12, SportingPrestige: +6, Importance: 88));
        else if (guadagnate >= 5)
            trovati.Add(new("rimonta", "Una bella rimonta",
                $"Dal {partenza}° al {gara.Position}° posto: {guadagnate} posizioni guadagnate, "
                + "una gara passata quasi tutta a sorpassare.",
                Fitness: -4, Influencer: +6, SportingPrestige: +3, Importance: 68));

        // --- l'esordio che va bene subito
        if (primaInCategoria && vittoria)
            trovati.Add(new("vittoria-esordio", "Vittoria all'esordio",
                "Prima gara con questa macchina, e ha vinto. Nel paddock non parlano d'altro.",
                Fitness: -3, Influencer: +18, SportingPrestige: +10, Importance: 96));
        else if (primaInCategoria && podio)
            trovati.Add(new("podio-esordio", "Podio all'esordio",
                $"Prima gara nella nuova categoria e già {gara.Position}°: nessuno se lo aspettava così presto.",
                Fitness: -2, Influencer: +10, SportingPrestige: +6, Importance: 84));

        // --- il ritiro che pesa più di una sconfitta
        if (gara.Dnf && gara.Laps <= 1)
            trovati.Add(new("ritiro-primo-giro", "Fuori al primo giro",
                decisivaPerIlTitolo
                    ? "Fermo prima ancora di cominciare, con il campionato che se ne va lì. "
                      + "Non c'è niente da spiegare e nessuna fatica da raccontare: solo un weekend buttato."
                    : "Gara finita al primo giro. Un pomeriggio intero di viaggio per due curve.",
                Fitness: 0,
                Influencer: decisivaPerIlTitolo ? -6 : -3,
                SportingPrestige: decisivaPerIlTitolo ? -8 : -3,
                Importance: decisivaPerIlTitolo ? 92 : 58));
        else if (gara.Dnf && decisivaPerIlTitolo)
            trovati.Add(new("ritiro-titolo", "Il titolo perso ai box",
                "Il ritiro arriva quando il campionato era ancora aperto. "
                + "Di quella stagione resterà questa immagine, non le gare vinte.",
                Fitness: -2, Influencer: -2, SportingPrestige: -6, Importance: 90));

        // --- la pioggia: livella le macchine e mostra il pilota
        if (bagnato && podio)
            trovati.Add(new("acqua", "Giornata da pioggia",
                "Con l'acqua le differenze fra le macchine contano meno, e si vede chi sa guidare. "
                + "Oggi si è visto.",
                Fitness: -5, Influencer: +8, SportingPrestige: +6, Importance: 80));

        // --- dominare non fa notizia quanto soffrire, ma conta
        if (vittoria && partenza == 1 && guadagnate == 0)
            trovati.Add(new("dominio", "Weekend perfetto",
                "Primo in qualifica, primo al traguardo, mai una posizione persa. "
                + "Meno spettacolare di una rimonta, molto più difficile da ripetere.",
                Fitness: -2, Influencer: +4, SportingPrestige: +8, Importance: 82));

        // --- finire in fondo alla distanza, comunque
        if (!gara.Dnf && gara.Position >= partenti && partenti >= 6)
            trovati.Add(new("ultimo", "Ultimo, ma in fondo",
                "Nessuno ricorderà questa gara, tranne chi l'ha portata a termine.",
                Fitness: -3, Influencer: -1, SportingPrestige: 0, Importance: 40));

        return trovati;
    }

    /// <summary>
    /// Applica i momenti alla carriera. La forma fisica non scende mai sotto
    /// zero e il livello influencer resta nella sua scala: un momento
    /// eccezionale non deve poter rompere i limiti del gioco.
    /// </summary>
    public static void Apply(CareerState career, IEnumerable<RaceHighlight> momenti)
    {
        var profilo = career.ReputationProfile ??= new ReputationProfile();
        // Quanto vale un risultato dipende anche da CHI ti fa correre.
        //
        // La stessa vittoria in una squadra da vetrina finisce su tutti i
        // social del team e vale il doppio in seguito; nell'officina di paese
        // la vedono i presenti. E' il motivo per cui la squadra piu' cara puo'
        // convenire: costa di piu' a weekend, ma una vittoria li' ripaga in
        // visibilita' — e la visibilita' e' quello che apre le porte.
        var visibilita = TeamProfile.ById(career.TeamProfileId).Visibilita;
        foreach (var m in momenti)
        {
            career.Fitness = Math.Clamp(career.Fitness + m.Fitness, 0, 100);
            var seguito = m.Influencer > 0
                ? (int)Math.Round(m.Influencer * visibilita)
                : m.Influencer;
            profilo.PublicPopularity = Math.Clamp(profilo.PublicPopularity + seguito, 0, 100);
            profilo.SportingPrestige = Math.Clamp(profilo.SportingPrestige + m.SportingPrestige, 0, 100);

            var voci = new List<string>();
            if (m.Fitness != 0) voci.Add($"forma {m.Fitness:+#;-#;0}");
            if (m.Influencer != 0) voci.Add($"livello influencer {m.Influencer:+#;-#;0}");
            if (m.SportingPrestige != 0) voci.Add($"prestigio sportivo {m.SportingPrestige:+#;-#;0}");
            var effetti = voci.Count == 0 ? "" : $" ({string.Join(" · ", voci)})";

            career.News ??= [];
            career.News.Add($"{m.Title}: {m.Story}{effetti}");
            career.Events ??= [];
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow,
                StoryDate = career.StoryDate,
                Type = "RACE_HIGHLIGHT",
                Headline = $"{m.Title} — {m.Story}{effetti}",
                Track = "",
                Importance = m.Importance
            });
        }
    }
}
