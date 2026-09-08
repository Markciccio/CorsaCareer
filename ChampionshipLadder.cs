namespace CorsaCareer1991;

/// <summary>
/// La scala dei campionati: dove si corre, e quanto in alto.
///
/// Prima il campionato era solo un nome derivato dalla categoria della vettura
/// («Campionato regionale»), senza un'idea di altezza: non si capiva se fosse
/// un punto di partenza o un traguardo, e non esisteva una regola che dicesse
/// come si sale. Qui i livelli sono cinque, dichiarati, e la promozione ha una
/// condizione sola e leggibile.
///
/// È una cosa diversa dalla categoria della vettura (kart, formula, GT), che
/// resta in <see cref="CareerLadder"/>: si può correre in kart a livello
/// locale o nazionale, e sono due carriere molto diverse.
/// </summary>
public static class ChampionshipLadder
{
    /// <summary>Quanti livelli ha la scala. Il livello più basso è 1.</summary>
    public const int Levels = 5;

    /// <summary>Piazzamento finale entro cui si viene promossi al livello successivo.</summary>
    public const int PromotionPosition = 3;

    private static readonly (string Name, string Scope)[] Steps =
    [
        ("Campionato di zona",      "il circuito di casa e poco altro"),
        ("Campionato regionale",    "le squadre della regione"),
        ("Campionato nazionale",    "i migliori del paese"),
        ("Campionato internazionale", "trasferte fuori dal Giappone"),
        ("Campionato mondiale",     "il vertice: si corre per il titolo iridato")
    ];

    public static int Clamp(int level) => Math.Clamp(level <= 0 ? 1 : level, 1, Levels);

    /// <summary>Nome del campionato a quel livello.</summary>
    public static string Name(int level) => Steps[Clamp(level) - 1].Name;

    /// <summary>Contro chi si corre, detto in una riga.</summary>
    public static string Scope(int level) => Steps[Clamp(level) - 1].Scope;

    public static bool IsTop(int level) => Clamp(level) >= Levels;

    /// <summary>
    /// Il livello più alto che una categoria può sostenere.
    ///
    /// Le due scale sono diverse ma non indipendenti: un kart da noleggio non
    /// corre un mondiale, e una Formula 3 non corre il campionato del
    /// kartodromo di casa. Senza questo legame il banco di prova produceva un
    /// «Campionato mondiale» corso con un kart 125 dopo due anni — il nome più
    /// altisonante della scala su una carriera appena cominciata, che toglie
    /// senso a tutto quello che viene dopo.
    ///
    /// Il gradino della vettura è quindi anche il tetto del campionato: per
    /// salire di livello bisogna prima salire di categoria, ed è lì che sta la
    /// fatica.
    /// </summary>
    public static int MaxLevelForStep(int ladderStep) => Math.Clamp(ladderStep, 1, Levels);

    /// <summary>
    /// Il livello dopo una stagione conclusa. Arrivare nei primi tre porta al
    /// gradino successivo; qualunque altro piazzamento lascia dove si è, senza
    /// retrocessioni: si resta a correre lo stesso campionato.
    /// </summary>
    public static int AfterSeason(int level, int finalPosition)
    {
        var current = Clamp(level);
        if (finalPosition <= 0 || finalPosition > PromotionPosition) return current;
        return Clamp(current + 1);
    }

    /// <summary>
    /// L'etichetta della testata: dove si corre, su cosa, e quanto in alto — su
    /// entrambe le scale.
    ///
    /// Le altezze sono due e vanno lette insieme. La <b>categoria</b> dice su
    /// che mezzo si corre (1 = kart da noleggio, 7 = il vertice); il
    /// <b>campionato</b> dice contro chi (1 = di zona, 5 = mondiale). Si può
    /// essere in cima a una e in fondo all'altra: un kart 125 in un campionato
    /// nazionale è categoria 3 e livello 3, e sono due cose diverse.
    ///
    /// Prima qui compariva solo il livello di campionato, e mancava la metà del
    /// quadro: non si capiva quanto restasse da salire come categoria.
    /// </summary>
    /// <param name="categoryName">Il nome del gradino di categoria, per esteso.</param>
    /// <param name="categoryStep">Il numero del gradino di categoria.</param>
    /// <param name="categorySteps">Quanti gradini ha la scala delle categorie.</param>
    public static string Header(string categoryName, int categoryStep, int categorySteps, int level)
    {
        var livello = Clamp(level);
        var parti = new List<string>();
        if (!string.IsNullOrWhiteSpace(categoryName)) parti.Add(categoryName.Trim().ToUpperInvariant());
        if (categoryStep > 0 && categorySteps > 0) parti.Add($"CATEGORIA {categoryStep} DI {categorySteps}");
        parti.Add(Name(livello).ToUpperInvariant());
        parti.Add($"LIVELLO {livello} DI {Levels}");
        return string.Join("  ·  ", parti);
    }

    /// <summary>Cosa serve per salire, detto al giocatore.</summary>
    public static string PromotionRule(int level) => IsTop(level)
        ? "Sei al vertice: da qui non si sale, si difende."
        : $"Chiudi la stagione nei primi {PromotionPosition} per salire a «{Name(Clamp(level) + 1)}».";
}
