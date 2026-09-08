namespace CorsaCareer1991;

/// <summary>
/// Le gare di un sedile cliente nel calendario.
///
/// Il difetto: firmando un sedile cliente le gare venivano create come
/// «opportunità», e nello stesso momento il calendario veniva svuotato. Il
/// giocatore firmava, andava a cercare la prima gara nel calendario — che è
/// dove si guarda — non trovava niente, e la carriera sembrava bloccata.
///
/// Le due liste non sono in concorrenza: l'opportunità è la decisione da
/// prendere (quale weekend pagare), l'appuntamento nel calendario è il fatto che
/// quelle gare esistono. Servono entrambe, e devono raccontare la stessa cosa.
/// </summary>
public sealed partial class MainForm
{
    /// <summary>
    /// Copia nel calendario le gare aperte da un sedile cliente. Ogni voce
    /// dichiara di essere ancora da confermare: finché la quota non è pagata
    /// non è un impegno preso.
    /// </summary>
    private void PublishClientRacesToCalendar(TeamOffer offer)
    {
        career.Schedule ??= [];
        career.Opportunities ??= [];

        var prefix = $"client-race-{career.Season}-{offer.Team}";
        var races = career.Opportunities
            .Where(x => x.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && x.IsOpen)
            .OrderBy(x => x.Date)
            .ToList();

        if (races.Count == 0)
        {
            CareerLog.Warn("agenda", $"firma con {offer.Team}: nessuna gara da pubblicare nel calendario.");
            return;
        }

        var round = 0;
        foreach (var race in races)
        {
            round++;
            var scheduled = new ScheduledEvent
            {
                // L'identificativo ricalca quello dell'opportunità: pagando la
                // quota le due voci restano riconducibili l'una all'altra.
                Id = $"cal-{race.Id}",
                // Una gara comprata non è un round di campionato.
                //
                // Erano registrate come round: entravano nel calendario del
                // campionato, ne falsavano il conteggio e facevano ripartire la
                // numerazione da «round 1» ogni volta che si firmava un sedile
                // cliente. Nel banco di prova una stagione arrivava a undici
                // round di cui tre ripetuti, e non si chiudeva mai. Un pilota
                // cliente compra weekend: sono inviti, e vanno letti così.
                Kind = ScheduledEventKind.Invitation,
                Date = race.Date,
                TrackId = race.TrackId,
                TrackName = string.IsNullOrWhiteSpace(race.TrackName) ? race.TrackId : race.TrackName,
                Season = career.Season,
                Status = CareerScheduler.StatusPlanned,
                GeneratedBy = $"Sedile cliente con {offer.Team}: gara da confermare pagando la quota.",
                Objective = string.IsNullOrWhiteSpace(offer.Objective) ? race.Title : offer.Objective,
                EntryFee = race.NetCost,
                EntryFeePaid = false,
                ProposedBy = offer.Team
            };
            CareerScheduler.Append(career.Schedule, scheduled);
        }

        rounds = ChampionshipRoundsView();
        CareerLog.Info("agenda", $"firma con {offer.Team}: {races.Count} gare pubblicate nel calendario.");
    }
}
