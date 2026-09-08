using System.Text;
using System.Globalization;

namespace CorsaCareer;

public sealed record AudioScript(string Title, string Text, string SourceType);

public static class AudioCommentaryBuilder
{
    // Testi originali: tono da radiocronaca sportiva classica, senza imitare voci o frasi di persone reali.
    public static AudioScript ForEvent(CareerState career, CareerEventRecord story)
    {
        var race = IsRaceEvent(story.Type) ? ResolveRace(career, story) : null;
        var text = new StringBuilder();
        var date = (story.StoryDate == default ? career.StoryDate : story.StoryDate).ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
        var journalist = career.Journalist ?? new JournalistProfile();
        var commentator = string.IsNullOrWhiteSpace(journalist.ExpertCommentator) ? "Vittorio Sanna" : journalist.ExpertCommentator;
        var commentatorBio = string.IsNullOrWhiteSpace(journalist.ExpertCommentatorBio) ? "ex pilota di formule minori, oggi commentatore tecnico — personaggio inventato" : journalist.ExpertCommentatorBio;
        text.Append(BuildIntro(journalist, date, story.Track, commentator, commentatorBio));
        if (race == null)
        {
            text.Append($"{story.Headline} ");
            switch (story.Type.ToUpperInvariant())
            {
                case "SEASON_AWARD":
                    var season = career.SeasonArchive.OrderByDescending(x => x.CompletedUtc).FirstOrDefault();
                    text.Append(season == null
                        ? "Il dossier di stagione è stato annunciato, ma l'archivio non contiene ancora il riepilogo finale."
                        : $"Il dossier certifica la stagione {season.Season}: {career.Driver} ha chiuso con {season.Points} punti, {season.Wins} vittorie e posizione finale {season.FinalPosition}. Il premio archiviato ammonta a {season.Award:N0} euro.");
                    break;
                case "PROMOTION":
                    text.Append($"La promozione verso {career.Tier} è stata registrata dopo la stagione archiviata. Il prossimo sedile sarà deciso dai contenuti disponibili e dalle scelte di mercato, non da un risultato inventato.");
                    break;
                case "SPONSOR_OBJECTIVE":
                case "SPONSOR_REVIEW":
                    text.Append($"Il rapporto con {career.Sponsor} entra nel dossier economico: {career.SponsorQualifyingResults} risultati qualificanti, stato {career.SponsorObjectiveStatus}. Il team userà questo dato per la prossima trattativa.");
                    break;
                case "CONTRACT_SIGNING":
                case "TEAM_CHANGE":
                    text.Append($"Nel box {career.Team}, {career.Driver} prepara il prossimo capitolo con la {UiText.Car(career.Car)}. Il contratto e il cambio di sedile restano registrati nel paddock.");
                    break;
                case "CONTRACT_EXPIRED":
                    text.Append($"Il contratto con {career.Team} è terminato al termine della stagione archiviata. Il mercato è aperto: {career.Offers.Count} opportunità risultano disponibili e nessuna firma viene considerata conclusa prima della scelta del pilota.");
                    break;
                case "MONTHLY_RECAP":
                    text.Append($"Il riepilogo mensile conserva {career.Races} gare, {career.Points} punti e una reputazione di {career.Reputation} su 100. Il prossimo aggiornamento arriverà da un fatto registrato.");
                    break;
                case "PADDOCK_ROSTER":
                    var teammate = career.Drivers.FirstOrDefault(x => x.Relationship.Equals("compagno di squadra", StringComparison.OrdinalIgnoreCase));
                    var rivals = career.Drivers.Count(x => x.Relationship.Equals("avversario", StringComparison.OrdinalIgnoreCase));
                    text.Append($"Il progetto {career.Team} presenta il roster iniziale: {career.Drivers.Count} identità, il compagno {teammate?.Name ?? career.Teammate} e {rivals} avversari osservati. Sono profili di paddock senza risultati assegnati: il primo verdetto arriverà soltanto dalla pista reale.");
                    break;
                case "SCREENSHOT_CAPTURED":
                    text.Append($"Questa è una cattura reale della finestra di Assetto Corsa, archiviata come {story.PhotoView switch { "" or null => "visuale non dichiarata", var view => view }}. La redazione descrive soltanto ciò che l'immagine documenta: non è un risultato di gara e non sostituisce il referto ufficiale.");
                    break;
                case "PHOTO_ARCHIVE":
                    text.Append("La redazione presenta una foto importata nell'archivio della carriera. È un elemento visivo di contesto: non prova posizione, tempi o classifiche e non modifica il campionato.");
                    break;
                default:
                    text.Append($"Nel box {career.Team}, {career.Driver} prepara il prossimo capitolo con la {UiText.Car(career.Car)}. Questo è un briefing editoriale: il cronometro della gara non è ancora entrato nell'archivio, quindi non viene assegnato alcun risultato.");
                    break;
            }
        }
        else
        {
            text.Append($"La giornata racconta {career.Driver}, al volante della {UiText.Car(race.Car)}, partito dalla posizione {race.StartingPosition}. ");
            if (journalist.NarrativeStyle.Equals("Cronaca classica analitica", StringComparison.OrdinalIgnoreCase))
                text.Append("Seguiamo il referto giro dopo giro: prima il ritmo, poi i distacchi e infine la lettura del box. ");
            if (story.Type.Equals("FIRST_VICTORY", StringComparison.OrdinalIgnoreCase)) text.Append("È il primo successo ufficiale della carriera: un passaggio che entra nell'archivio e cambia il peso del pilota nel paddock. ");
            else if (story.Type.Equals("FIRST_PODIUM", StringComparison.OrdinalIgnoreCase)) text.Append("È il primo podio ufficiale della carriera: il risultato apre una nuova pagina nel dossier del pilota. ");
            else if (story.Type.Equals("MILESTONE_RACE", StringComparison.OrdinalIgnoreCase)) text.Append($"È il traguardo della gara {career.Races}: una tappa numerica verificabile, non un risultato aggiunto dalla redazione. ");
            if (race.QualificationPosition > 0) text.Append($"Il sabato aveva ottenuto la posizione {race.QualificationPosition}; il dato dà la misura del lavoro svolto prima del via. ");
            text.Append(race.Dnf ? "La corsa si è però chiusa con un ritiro: adesso il box dovrà separare il problema tecnico dall'errore di esecuzione e preparare la risposta. " : $"Dopo {race.Laps} giri arriva il verdetto: posizione {race.Position}, {race.Points} punti e un risultato che entra nella storia della stagione. ");
            if (race.BestLapMilliseconds > 0) text.Append($"Il miglior giro ufficiale è di {FormatLap(race.BestLapMilliseconds)}. ");
            text.Append(race.PitStops > 0 ? $"Il referto registra {race.PitStops} pit stop, un passaggio che il reparto tecnico userà per leggere il ritmo e la gestione delle gomme. " : "Il referto non registra pit stop. ");
            if (race.PenaltySeconds > 0) text.Append($"Sono inoltre registrati {race.PenaltySeconds:0.0} secondi di penalità, un dettaglio che pesa sulla lettura finale. ");
            if (race.GapMilliseconds > 0) text.Append($"Il distacco ufficiale è di {FormatGap(race.GapMilliseconds)}. ");
            if (race.TeammatePosition > 0) text.Append(race.Position < race.TeammatePosition ? $"Il confronto con il compagno {race.TeammateName} sorride a {career.Driver}. " : race.Position > race.TeammatePosition ? $"Il compagno {race.TeammateName} precede {career.Driver} in classifica. " : $"{career.Driver} e {race.TeammateName} chiudono appaiati nel confronto interno. ");
            text.Append($"Il risultato aggiorna il budget a {career.Cash:N0} euro, la reputazione a {career.Reputation} su 100 e il bottino stagionale a {career.Points} punti. ");
            text.Append($"Lo sponsor {career.Sponsor} registra un bonus di {race.SponsorBonus:N0} euro; il contratto con {career.Team} resta legato all'obiettivo: {career.ContractObjective}. ");
            var arc = career.StoryArcs.OrderByDescending(x => x.Importance).FirstOrDefault(x => x.Status == "In corso");
            if (arc != null) text.Append($"Nel racconto più ampio della carriera, il filo aperto è {arc.Title}: {arc.Summary} ");
            var offers = career.Offers.Count;
            text.Append(offers > 0 ? $"Nel paddock circolano già {offers} proposte: nessuna promessa viene data per chiusa, ma ogni referto modifica il mercato. " : "Il mercato osserva in silenzio: serviranno altri referti per aprire nuove trattative. ");
            text.Append(story.Headline);
        }
        text.Append(" Fine del servizio. Il prossimo capitolo arriverà soltanto dopo un nuovo test o una gara realmente conclusa in Assetto Corsa.");
        return new AudioScript($"{story.Type} — {story.Track}", text.ToString(), "FATTI IMPORTATI DA ASSETTO CORSA");
    }

    private static string BuildIntro(JournalistProfile journalist, string date, string track, string commentator, string commentatorBio)
    {
        var style = journalist.NarrativeStyle ?? "Cronaca classica analitica";
        if (style.Equals("Paddock e interviste", StringComparison.OrdinalIgnoreCase))
            return $"Aggiornamento dal paddock CorsaCareer. È {date}, siamo a {track}: servizio di {journalist.Name}, {journalist.Publication}. In cabina {commentator}, {commentatorBio}. ";
        if (style.Equals("Redazione moderna", StringComparison.OrdinalIgnoreCase))
            return $"Edizione digitale CorsaCareer, {date}. Dal circuito di {track}, la redazione di {journalist.Publication} apre il servizio firmato da {journalist.Name}; analisi tecnica di {commentator}, {commentatorBio}. ";
        return $"Edizione speciale del diario CorsaCareer. È {date}, e il nostro inviato è al circuito di {track}. Servizio di {journalist.Name}, {journalist.Publication}. Prima i tempi e i distacchi, poi la voce del box: commento tecnico di {commentator}, {commentatorBio}. ";
    }

    public static AudioScript TestBriefing(CareerState career, Round round)
        => new("Servizio test e briefing", $"È {career.StoryDate.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"))}. Dal paddock di {round.Track}, {career.Driver} prepara il weekend con {career.Team}. Il programma registra il test come briefing: nessun risultato viene assegnato finché una sessione reale non viene conclusa in Assetto Corsa.", "BRIEFING, NON RISULTATO");

    private static string FormatLap(int ms) => $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}";
    private static string FormatGap(int ms) => ms < 60000 ? $"{ms / 1000.0:0.000} secondi" : $"{ms / 60000:0} minuti e {ms / 1000 % 60:00} secondi";
    private static bool IsRaceEvent(string type) => type.Equals("RACE_FINISHED", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_OR_NEXT_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("MILESTONE_RACE", StringComparison.OrdinalIgnoreCase)
        || type.Equals("RETIREMENT", StringComparison.OrdinalIgnoreCase);

    private static RaceHistoryEntry? ResolveRace(CareerState career, CareerEventRecord story)
    {
        var candidates = career.RaceHistory.Where(x => x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase)).ToList();
        if (candidates.Count == 0) return null;
        if (story.DateUtc != default)
            return candidates.OrderBy(x => Math.Abs((x.DateUtc - story.DateUtc).Ticks)).First();
        return candidates[^1];
    }
}
