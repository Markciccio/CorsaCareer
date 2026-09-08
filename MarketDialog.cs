using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class MarketDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly ListBox offers = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, BorderStyle = BorderStyle.None, Font = UiTheme.Body, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 94 };
    private readonly RichTextBox detail = new() { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body, ScrollBars = RichTextBoxScrollBars.Vertical };
    private Image? teamLogoSheet;
    private Image? negotiationArt;
    private PictureBox? negotiationVisual;
    public bool SponsorChanged { get; private set; }

    /// <summary>
    /// Il sedile scelto, fissato nel momento del clic.
    ///
    /// Prima questa proprietà leggeva la selezione dalla ListBox, ma chi la
    /// interroga lo fa DOPO che la finestra si è chiusa: a quel punto il
    /// controllo nativo non esiste più e la selezione torna vuota. Il
    /// chiamante riceveva quindi «nessuna offerta scelta» e usciva senza fare
    /// niente — si sceglieva la squadra e non succedeva nulla, senza un
    /// messaggio che lo spiegasse.
    /// </summary>
    private TeamOffer? chosen;

    public TeamOffer? SelectedOffer => chosen ?? offers.SelectedItem as TeamOffer;

    public MarketDialog(CareerState career)
    {
        this.career = career; TeamInterestService.Refresh(career);
        Text = "CorsaCareer — mercato e scouting"; BackColor = UiTheme.Background; ForeColor = UiTheme.TextPrimary;
        var header = new Panel { Dock = DockStyle.Top, Height = 106, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 16, 38, 14) };
        header.Controls.Add(new Label { Text = "MERCATO E SCOUTING", Dock = DockStyle.Top, Height = 38, Font = UiTheme.Headline, ForeColor = UiTheme.Warning });
        header.Controls.Add(new Label { Text = "Non sei l'unico candidato. Ogni sedile ha un costo, una storia e qualcuno pronto a prendertelo.", Dock = DockStyle.Bottom, Height = 28, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary });
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 78, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 15, 38, 15) };
        var accept = UiTheme.PrimaryButton("FIRMA / ACCETTA IL SEDILE"); accept.Dock = DockStyle.Right; accept.Width = 290; accept.DialogResult = DialogResult.OK;
        accept.Click += (_, _) =>
        {
            chosen = offers.SelectedItem as TeamOffer;
            if (chosen != null) return;
            // Il rifiuto non deve essere muto: senza questo avviso il pulsante
            // sembrava semplicemente rotto.
            DialogResult = DialogResult.None;
            MessageBox.Show(this,
                offers.Items.Count == 0 || offers.Items[0] is not TeamOffer
                    ? "Non c'è nessun sedile da firmare: al momento nessuna squadra ha formalizzato un'offerta.\n\nUsa il radar e i contatti sponsor per farti notare, poi torna qui."
                    : "Seleziona prima un sedile dall'elenco a sinistra, poi premi «FIRMA / ACCETTA IL SEDILE».",
                "CorsaCareer — nessun sedile selezionato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        footer.Controls.Add(accept);
        // «Haru cerca sponsor» viveva anche qui, in basso a sinistra, mentre le
        // sponsorizzazioni hanno già il loro posto in Home: due porte per la
        // stessa stanza, in una pagina che parla di sedili.
        footer.Controls.Add(new Label { Text = $"Cassa personale: € {career.Cash:N0}  ·  sponsor: € {career.SponsorBudget:N0}  ·  supporto team: {career.TeamSupportPercent}%  ·  qui i sedili sono pagati, non stipendiati.", Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft });
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(32, 22, 32, 18), BackColor = UiTheme.Background };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        var offerCard = UiTheme.Card("SEDILI SUL TAVOLO · RADAR", out var offerInner, UiTheme.Warning); offerCard.Dock = DockStyle.Fill;
        var offerLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = UiTheme.Surface };
        offerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 56)); offerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
        offerLayout.Controls.Add(offers, 0, 0);
        var compactRadar = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextSecondary, Font = UiTheme.Small, Padding = new Padding(10) };
        compactRadar.Text = "RADAR DELLE SCUDERIE · INTERESSE ≠ OFFERTA\nLa percentuale è la probabilità di un prossimo contatto, non una promessa di contratto. Cresce con risultati, affidabilità e visibilità.\n\n" + string.Join("\n\n", career.TeamInterests.OrderByDescending(x => x.Value).Select(x => $"{x.Team.ToUpperInvariant()}  {x.Value}% · {x.Status}\n{x.Reason}")) + $"\n\nHARU SENDA · CONTATTI SPONSOR\nAzioni rimaste questa settimana: {career.SponsorActionsRemaining}\nContributo sponsor separato dalla cassa: € {career.SponsorBudget:N0}";
        offerLayout.Controls.Add(compactRadar, 0, 1); offerInner.Controls.Add(offerLayout); grid.Controls.Add(offerCard, 0, 0);
        var dossierCard = UiTheme.Card("DOSSIER DELLA TRATTATIVA", out var dossierInner, UiTheme.Accent); dossierCard.Dock = DockStyle.Fill;
        var dossierLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = UiTheme.Surface };
        dossierLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28)); dossierLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 72));
        negotiationVisual = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        dossierLayout.Controls.Add(negotiationVisual, 0, 0); dossierLayout.Controls.Add(detail, 0, 1); dossierInner.Controls.Add(dossierLayout); grid.Controls.Add(dossierCard, 1, 0);
        var logoPath = AssetPaths.File("team-logos-scouting.png");
        if (File.Exists(logoPath)) { using var source = Image.FromFile(logoPath); teamLogoSheet = new Bitmap(source); }
        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);
        foreach (var offer in career.Offers) offers.Items.Add(offer);
        if (offers.Items.Count == 0) offers.Items.Add("Nessun sedile offerto: osserva il radar e costruisci interesse.");
        offers.DrawItem += DrawOfferCard;
        Disposed += (_, _) => { teamLogoSheet?.Dispose(); negotiationArt?.Dispose(); };
        offers.SelectedIndexChanged += (_, _) => ShowOffer(); if (offers.Items.Count > 0) offers.SelectedIndex = 0;
    }

    private void ShowOffer()
    {
        if (SelectedOffer is not { } o) { detail.Text = "Nessuna offerta formale.\n\nLe squadre osservano i tuoi test: il radar mostra chi potrebbe chiamare per primo."; SetNegotiationArt(null); return; }
        SetNegotiationArt(o);
        var interest = career.TeamInterests.FirstOrDefault(x => x.Team.Equals(o.Team, StringComparison.OrdinalIgnoreCase));
        var economics = o.IsClientSeat
            ? $"QUOTA DELLA GARA CLIENTE\nFee standard: € {o.RaceFee:N0}\nSupporto del team: {o.TeamSupportPercent}% · € {o.RaceFee - o.NetRaceFee:N0}\nDa pagare prima di scendere in pista: € {o.NetRaceFee:N0}\n\nPREMI IN PISTA\nP1: € {o.PrizeP1:N0} · P2: € {o.PrizeP2:N0} · P3: € {o.PrizeP3:N0} · P4–P5: € {o.PrizeP4P5:N0}\nSe vai male non c'è stipendio: resta soltanto la quota bruciata."
            : $"CONDIZIONI ECONOMICHE\n€ {o.Salary:N0}/anno · contratto di {o.Years} anno/i\nLo stipendio è garantito dal team: premi e sponsor restano separati.";
        detail.Text = $"{o.Team.ToUpperInvariant()}\n\nAUTO ASSEGNATA\n{UiText.Car(o.Car)} · categoria {o.Category}\nLivrea: {(string.IsNullOrWhiteSpace(o.Livery) ? "da definire" : o.Livery)}\nLa categoria definisce livello degli avversari, calendario e salto di carriera.\n\n{economics}\n\nSTATO ECONOMICO\n{o.FundingLabel}\nCompagno osservato: {o.Teammate} · Sponsor: {o.Sponsor}\n\nPRESTIGIO DEL SEDILE\n{o.Prestige}/100\nPiù è alto, più aumenta visibilità e aspettative: un risultato mediocre pesa di più.\n\nPERCHÉ ORA\n{o.Origin}\n\nINTERESSE DEL TEAM\n{interest?.Value ?? 0}% · {interest?.Status ?? "nessun dato"}\n{interest?.Reason}\nLa percentuale non è un contratto: indica quanto sei vicino al prossimo passo.\n\nOBIETTIVO SPORTIVO\n{o.Objective}\n\nCONCORRENTI\nKenta Ogawa · passo costante\nSota Fujimoto · appoggio economico\nMei Kanzaki · reputazione nel paddock\n\nLa progressione è: cliente pagante → quota scontata → sedile finanziato → professionista pagato.";
    }

    private void ContactSponsor()
    {
        career.SponsorProspects ??= [];
        var available = career.SponsorProspects.Where(x => !x.Contacted).ToList();
        if (career.SponsorActionsRemaining <= 0 || available.Count == 0)
        {
            MessageBox.Show("Haru non ha più contatti disponibili questa settimana. Le occasioni si rinnovano dopo il prossimo appuntamento.", "Sponsor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var picker = new Form { Text = "Haru Senda · contatti sponsor", ClientSize = new Size(620, 430), StartPosition = FormStartPosition.CenterParent, BackColor = UiTheme.Background, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body };
        var intro = new Label { Dock = DockStyle.Top, Height = 72, Padding = new Padding(18, 14, 18, 8), Text = $"Haru Senda: «Abbiamo {career.SponsorActionsRemaining} contatti questa settimana. Scegline uno: l'interesse si può intuire, non garantire.»\nAzioni rimaste: {career.SponsorActionsRemaining}", ForeColor = UiTheme.TextSecondary };
        var list = new ListBox { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body, IntegralHeight = false };
        foreach (var p in available) list.Items.Add($"{p.Name} · {p.Sector} · {new string('★', p.PerceivedStars)} · contributo possibile € {p.Contribution:N0}");
        list.SelectedIndex = 0;
        var go = UiTheme.PrimaryButton("CONTATTA"); go.Dock = DockStyle.Bottom; go.Height = 44; go.Click += (_, _) => picker.DialogResult = DialogResult.OK;
        picker.Controls.Add(list); picker.Controls.Add(go); picker.Controls.Add(intro);
        if (picker.ShowDialog(this) != DialogResult.OK || list.SelectedIndex < 0) return;
        var prospect = available[list.SelectedIndex];
        prospect.Contacted = true; prospect.Status = "Contattato"; career.SponsorActionsRemaining--;
        var chance = Math.Clamp(prospect.BaseInterest
            + (career.Wins > 0 ? 15 : 0) + (career.Podiums > 0 ? 10 : 0)
            + (career.ReputationProfile?.SportingPrestige >= 40 ? 8 : 0)
            + (career.SponsorRelation >= 50 ? 5 : 0), 5, 95);
        var success = Random.Shared.Next(1, 101) <= chance;
        if (success)
        {
            prospect.Status = "Accettato"; career.SponsorBudget += prospect.Contribution; career.SponsorMoney += prospect.Contribution;
            career.Sponsor = prospect.Name; career.SponsorRelation = Math.Clamp(career.SponsorRelation + 8, 0, 100);
            MessageBox.Show($"Haru ha chiuso il contatto con {prospect.Name}.\n\nContributo destinato alla prossima gara: € {prospect.Contribution:N0}.\nNon è denaro personale: resta nel budget sponsor.", "Sponsor accettato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
            MessageBox.Show($"{prospect.Name} per ora rimanda la decisione.\n\nHaru: «Non è un no per sempre, ma questa settimana abbiamo bruciato il contatto.»", "Sponsor rifiutato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SponsorChanged = true; compactRadarRefresh();
    }

    private void compactRadarRefresh()
    {
        // Il pannello viene ricreato al prossimo accesso: qui aggiorniamo solo
        // il titolo della finestra per rendere immediato il consumo dell'azione.
        Text = $"CorsaCareer — mercato e scouting · contatti Haru rimasti {career.SponsorActionsRemaining}";
    }

    private void SetNegotiationArt(TeamOffer? offer)
    {
        if (negotiationVisual == null) return;
        negotiationArt?.Dispose(); negotiationArt = null; negotiationVisual.Image = null;
        // Il simbolo della squadra, non un'illustrazione generica scelta per
        // sottostringa del nome: una trattativa ha un interlocutore preciso e
        // riconoscerlo a colpo d'occhio è metà del senso di questa schermata.
        var path = TeamLogoCatalog.LogoPath(offer?.Team);
        if (path.Length == 0)
        {
            // Squadra fuori catalogo o logo non installato: si torna alla tavola
            // editoriale, che non pretende di essere l'identità del team.
            var fallback = AssetPaths.File(
                offer == null ? "calendar-rookie-rain.png" : "manga-sponsor-table.png");
            if (!File.Exists(fallback)) return;
            path = fallback;
        }
        using var source = Image.FromFile(path); negotiationArt = new Bitmap(source); negotiationVisual.Image = negotiationArt;
    }

    private void DrawOfferCard(object? sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        if (e.Index < 0 || e.Index >= offers.Items.Count) return;
        var selected = (e.State & DrawItemState.Selected) != 0;
        using var bg = new SolidBrush(selected ? Color.FromArgb(37, 85, 128) : UiTheme.SurfaceRaised);
        e.Graphics.FillRectangle(bg, e.Bounds);
        if (offers.Items[e.Index] is not TeamOffer offer) { TextRenderer.DrawText(e.Graphics, offers.Items[e.Index]?.ToString(), UiTheme.Body, e.Bounds, UiTheme.TextSecondary); return; }
        var logoRect = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top + 8, 76, 76);
        if (teamLogoSheet != null)
        {
            var cell = offer.Team.Contains("Kaido", StringComparison.OrdinalIgnoreCase) ? new Rectangle(teamLogoSheet.Width / 2, 0, teamLogoSheet.Width / 2, teamLogoSheet.Height / 2)
                : offer.Team.Contains("Apex", StringComparison.OrdinalIgnoreCase) ? new Rectangle(0, teamLogoSheet.Height / 2, teamLogoSheet.Width / 2, teamLogoSheet.Height / 2)
                : offer.Team.Contains("Hoshino", StringComparison.OrdinalIgnoreCase) ? new Rectangle(teamLogoSheet.Width / 2, teamLogoSheet.Height / 2, teamLogoSheet.Width / 2, teamLogoSheet.Height / 2)
                : new Rectangle(0, 0, teamLogoSheet.Width / 2, teamLogoSheet.Height / 2);
            e.Graphics.DrawImage(teamLogoSheet, logoRect, cell, GraphicsUnit.Pixel);
        }
        var x = logoRect.Right + 12;
        TextRenderer.DrawText(e.Graphics, offer.Team.ToUpperInvariant(), UiTheme.BodyStrong, new Rectangle(x, e.Bounds.Top + 11, e.Bounds.Width - x, 22), Color.White);
        TextRenderer.DrawText(e.Graphics, $"{offer.Category} · {UiText.Car(offer.Car)}", UiTheme.Small, new Rectangle(x, e.Bounds.Top + 35, e.Bounds.Width - x, 18), UiTheme.TextSecondary);
        var interest = career.TeamInterests.FirstOrDefault(t => t.Team.Equals(offer.Team, StringComparison.OrdinalIgnoreCase));
        var money = offer.IsClientSeat ? $"quota € {offer.NetRaceFee:N0} · premio P1 € {offer.PrizeP1:N0}" : $"€ {offer.Salary:N0}/anno";
        TextRenderer.DrawText(e.Graphics, $"{money}   ·   interesse {interest?.Value ?? 0}%", UiTheme.Small, new Rectangle(x, e.Bounds.Top + 57, e.Bounds.Width - x, 20), UiTheme.Warning);
        using var border = new Pen(selected ? UiTheme.Info : UiTheme.Border); e.Graphics.DrawRectangle(border, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
    }
}
