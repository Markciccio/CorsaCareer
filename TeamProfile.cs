namespace CorsaCareer;

/// <summary>
/// Che tipo di squadra è.
///
/// Prima due offerte differivano solo per la quota: sceglierne una era una
/// scelta aritmetica, e vinceva sempre la più economica. Una squadra però non è
/// solo un prezzo. Ce n'è una che costa il doppio ma ti mette in vetrina, e
/// vincere con lei vale il triplo in visibilità e quindi in sponsor; ce n'è
/// un'altra che costa poco e non ti fa vedere da nessuno, ma i meccanici sono
/// bravi e la macchina va; ce n'è una che paga premi grossi e non le importa
/// niente di te.
///
/// Sono i moltiplicatori a rendere la scelta interessante: non esiste la
/// squadra giusta, esiste quella giusta per quello che ti serve adesso — soldi,
/// visibilità, o una macchina che non si rompe.
/// </summary>
public sealed record TeamProfile(
    string Id,
    string Nome,
    string Descrizione,
    double CostoWeekend,
    double Premi,
    double Visibilita,
    double SupportoTecnico,
    string Compromesso)
{
    /// <summary>Nessun tratto: la squadra media, che non eccelle e non penalizza in niente.</summary>
    public static readonly TeamProfile Neutro = new(
        "neutro", "Squadra ordinaria",
        "Una squadra come tante: nessun difetto evidente e nessun motivo per parlarne.",
        1.0, 1.0, 1.0, 1.0,
        "Niente di eccezionale in nessuna direzione.");

    public static readonly IReadOnlyList<TeamProfile> Tutti =
    [
        new("vetrina", "Squadra da vetrina",
            "Sponsor importanti, fotografi al box, presenza sui social curata da loro. Ti mettono in mostra perché conviene a entrambi.",
            CostoWeekend: 1.45, Premi: 1.0, Visibilita: 2.0, SupportoTecnico: 0.9,
            Compromesso: "Costa quasi il doppio, ma qui una vittoria si vede. Il seguito che costruisci vale più di quello che spendi — se vinci."),

        new("officina", "Officina di paese",
            "Due meccanici, un furgone e trent'anni di mestiere. Nessuno saprà che hai corso, ma la macchina è preparata bene.",
            CostoWeekend: 0.7, Premi: 0.9, Visibilita: 0.5, SupportoTecnico: 1.35,
            Compromesso: "La più economica e la più solida. Ma potresti vincere tutto senza che nessuno se ne accorga."),

        new("tecnica", "Squadra tecnica",
            "Telemetria, ingegneri, dati dopo ogni turno. Non ti coccolano: ti misurano.",
            CostoWeekend: 1.15, Premi: 1.0, Visibilita: 0.9, SupportoTecnico: 1.5,
            Compromesso: "Qui si impara a guidare davvero. La visibilità è un problema tuo."),

        new("premi", "Squadra da montepremi",
            "Corrono nei campionati dove si vince denaro e non gloria. Del pilota gli interessa che porti a casa la macchina.",
            CostoWeekend: 1.25, Premi: 1.9, Visibilita: 0.7, SupportoTecnico: 1.0,
            Compromesso: "Se vai forte ti ripaghi la stagione. Se vai piano, hai buttato più soldi che altrove."),

        new("giovani", "Programma giovani",
            "Ti prendono perché credono di poterti rivendere. Coprono parte dei costi e in cambio pretendono risultati.",
            CostoWeekend: 0.55, Premi: 1.0, Visibilita: 1.3, SupportoTecnico: 1.15,
            Compromesso: "L'occasione migliore che ci sia — ma se non rendi, il posto lo danno a un altro."),

        new("famiglia", "Squadra di famiglia",
            "Gente che corre per passione da una generazione. Ti trattano bene e non ti chiedono molto.",
            CostoWeekend: 0.8, Premi: 0.85, Visibilita: 0.8, SupportoTecnico: 1.2,
            Compromesso: "Un posto tranquillo dove crescere. Nessuno però ti spingerà a fare il salto.")
    ];

    public static TeamProfile ById(string? id) =>
        Tutti.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)) ?? Neutro;

    /// <summary>
    /// I profili di un gruppo di offerte, tutti diversi fra loro.
    ///
    /// Ricevere tre proposte con lo stesso carattere non e' una scelta: la
    /// selezione parte da un punto stabile del catalogo e prosegue in ordine,
    /// cosi' le tre proposte sono sempre di tre tipi diversi e la stessa
    /// carriera vede sempre le stesse.
    /// </summary>
    public static IReadOnlyList<TeamProfile> Assortimento(int quanti, int seme)
    {
        var scelti = new List<TeamProfile>();
        if (quanti <= 0 || Tutti.Count == 0) return scelti;
        var partenza = Math.Abs(seme) % Tutti.Count;
        for (var i = 0; i < quanti; i++) scelti.Add(Tutti[(partenza + i) % Tutti.Count]);
        return scelti;
    }
}
