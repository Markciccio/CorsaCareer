namespace CorsaCareer;

/// <summary>Una scena che il gioco avrebbe mostrato al giocatore.</summary>
public sealed record ScenaRegistrata(
    DateTime Quando, int Stagione, int Gara, string Tipo, string Titolo, IReadOnlyList<string> Righe);

/// <summary>
/// Il copione: che cosa avrebbe visto il giocatore, e in che ordine.
///
/// Il banco gira senza nessuno davanti allo schermo, quindi tutte le finestre
/// vengono saltate: <c>if (CareerMessages.Unattended) return;</c> compare in
/// una dozzina di posti. Ottimo per misurare la carriera, inutile per sapere
/// se le scene scattano al momento giusto — ed e' proprio la cosa che non
/// avevamo mai verificato: le venti scene scritte, il verdetto di stagione, la
/// chiamata da sopra, l'epilogo erano agganciati e mai visti partire in
/// sequenza durante una partita vera.
///
/// Qui le scene, invece di essere buttate via, vengono annotate. Alla fine si
/// legge la carriera come un copione: quali momenti sono scattati, quando, in
/// che ordine, e soprattutto <b>quante finestre di fila</b> — che e' il difetto
/// che abbiamo gia' dovuto correggere una volta.
///
/// Non sostituisce il giocare davvero: non prova la tastiera nella camminata
/// ne' i clic sulle schede della trattativa. Prova che la regia funzioni.
/// </summary>
public static class SceneRecorder
{
    /// <summary>Acceso solo dal banco: in partita non deve costare niente.</summary>
    public static bool Attivo { get; set; }

    private static readonly List<ScenaRegistrata> scene = [];

    public static IReadOnlyList<ScenaRegistrata> Scene => scene;

    public static void Azzera() => scene.Clear();

    /// <summary>Annota una scena. Chiamata dove il gioco la mostrerebbe.</summary>
    public static void Registra(CareerState? carriera, string tipo, string titolo, IEnumerable<string>? righe = null)
    {
        if (!Attivo) return;
        scene.Add(new ScenaRegistrata(
            carriera?.StoryDate ?? default,
            carriera?.Season ?? 0,
            carriera?.Races ?? 0,
            tipo, titolo,
            righe?.ToList() ?? []));
    }

    /// <summary>Annota una scena parlata, con chi dice cosa.</summary>
    public static void Registra(CareerState? carriera, string tipo, string titolo,
        IEnumerable<AnimeDialogueLine> battute) =>
        Registra(carriera, tipo, titolo, battute.Select(x => $"{x.Speaker}: {x.Text}"));

    /// <summary>
    /// Le sequenze troppo lunghe: piu' di questo numero di finestre di fila
    /// senza che il giocatore faccia niente in mezzo e la scena smette di
    /// essere un momento e diventa una coda da sbrigare.
    /// </summary>
    public const int FinestreDiFilaAccettabili = 4;

    /// <summary>
    /// Raggruppa le scene che sarebbero comparse una dopo l'altra senza che il
    /// giocatore potesse fare altro: stessa data e stesso numero di gare.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<ScenaRegistrata>> Raffiche()
    {
        var gruppi = new List<List<ScenaRegistrata>>();
        foreach (var s in scene)
        {
            var ultimo = gruppi.Count > 0 ? gruppi[^1] : null;
            if (ultimo != null && ultimo[^1].Quando == s.Quando && ultimo[^1].Gara == s.Gara)
                ultimo.Add(s);
            else
                gruppi.Add([s]);
        }
        return gruppi;
    }
}
