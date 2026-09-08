using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>Quello che il giocatore ha chiesto di fare alle sue carriere.</summary>
public enum CareerManagerAction
{
    Niente,
    /// <summary>Comincia una carriera nuova, archiviando quella in corso.</summary>
    Nuova,
    /// <summary>Riprendi la carriera selezionata.</summary>
    Riprendi,
    /// <summary>Metti via la carriera in corso con un nome, senza chiuderla.</summary>
    Archivia,

    /// <summary>
    /// Butta via la carriera in corso e ricomincia.
    ///
    /// Mancava, e la mancanza si vedeva subito: l'elenco cancellabile conteneva
    /// solo le carriere archiviate, quindi chi voleva liberarsi di quella che
    /// stava giocando — il caso normale — non aveva nessun pulsante da premere
    /// e non capiva perché non riuscisse a selezionarla.
    /// </summary>
    EliminaCorrente
}

/// <summary>
/// La gestione delle carriere, in un posto solo e raggiungibile.
///
/// Le funzioni c'erano già tutte — nuova, carica, archivia, cancella — ma erano
/// sei pulsanti piccoli sparsi dentro un pannello secondario, in mezzo a
/// «tavole del manga» e «servizio del giorno». Il risultato pratico è che per
/// chi gioca non esistevano: si poteva ricominciare da capo senza sapere di
/// poterlo fare, e riprendere una carriera vecchia era impossibile da trovare.
///
/// Qui c'è l'elenco vero, con quello che serve per riconoscere una carriera a
/// colpo d'occhio — pilota, dove è arrivato, quante gare, quando ci si è giocato
/// l'ultima volta — e le azioni scritte grandi. Cancellare chiede conferma e non
/// tocca mai la carriera in corso.
/// </summary>
public sealed class CareerManagerDialog : CareerDialog
{
    private readonly string cartella;
    private readonly CareerState corrente;
    private readonly ListView elenco = new()
    {
        Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = true,
        BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, BorderStyle = BorderStyle.None,
        HideSelection = false, Font = UiTheme.Body
    };
    private readonly Label dettaglio = new()
    {
        Dock = DockStyle.Fill, ForeColor = UiTheme.TextSecondary, Font = UiTheme.Body,
        Padding = new Padding(14, 10, 14, 10)
    };

    /// <summary>Cosa ha deciso il giocatore.</summary>
    public CareerManagerAction Azione { get; private set; } = CareerManagerAction.Niente;

    /// <summary>La carriera da riprendere, valorizzata solo con <see cref="CareerManagerAction.Riprendi"/>.</summary>
    public CareerState? Scelta { get; private set; }

    /// <summary>Il nome con cui archiviare, valorizzato solo con <see cref="CareerManagerAction.Archivia"/>.</summary>
    public string NomeArchivio { get; private set; } = "";

    public CareerManagerDialog(string cartellaCarriere, CareerState carrieraCorrente)
    {
        cartella = cartellaCarriere;
        corrente = carrieraCorrente;
        Text = "CorsaCareer — gestione carriere";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var header = new Panel { Dock = DockStyle.Top, Height = 118, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 18, 40, 14) };
        header.Controls.Add(new Label
        {
            Text = "GESTIONE CARRIERE", Dock = DockStyle.Top, Height = 46,
            Font = UiTheme.Headline, ForeColor = UiTheme.Warning
        });
        header.Controls.Add(new Label
        {
            Text = "Comincia da capo, riprendi una carriera messa da parte, o fai pulizia. "
                   + "La carriera in corso viene sempre archiviata prima di essere sostituita: non si perde niente per sbaglio.",
            Dock = DockStyle.Bottom, Height = 34, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        // --- la carriera in corso, sempre in evidenza
        var correnteCard = UiTheme.Card("LA CARRIERA IN CORSO", out var correnteInner, UiTheme.Positive);
        correnteCard.Dock = DockStyle.Top;
        correnteCard.Height = 104;
        correnteInner.Controls.Add(new Label
        {
            Text = Descrivi(corrente), Dock = DockStyle.Fill,
            Font = UiTheme.Body, ForeColor = UiTheme.TextPrimary, Padding = new Padding(12, 8, 12, 8)
        });

        // --- l'archivio
        elenco.Columns.Add("Carriera", 300);
        elenco.Columns.Add("Pilota", 180);
        elenco.Columns.Add("Dove è arrivato", 320);
        elenco.Columns.Add("Gare", 70, HorizontalAlignment.Right);
        elenco.Columns.Add("Vittorie", 80, HorizontalAlignment.Right);
        elenco.Columns.Add("Ultimo salvataggio", 190);
        elenco.SelectedIndexChanged += (_, _) => MostraDettaglio();

        var archivioCard = UiTheme.Card("CARRIERE MESSE DA PARTE", out var archivioInner, UiTheme.Info);
        archivioCard.Dock = DockStyle.Fill;
        archivioInner.Controls.Add(elenco);

        var dettaglioCard = UiTheme.Card("SCHEDA", out var dettaglioInner, UiTheme.TextMuted);
        dettaglioCard.Dock = DockStyle.Bottom;
        dettaglioCard.Height = 116;
        dettaglioInner.Controls.Add(dettaglio);

        var centro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(32, 16, 32, 12), BackColor = UiTheme.Background };
        centro.Controls.Add(archivioCard);
        centro.Controls.Add(dettaglioCard);
        centro.Controls.Add(correnteCard);

        // --- le azioni
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 92, BackColor = UiTheme.HeaderBackground, Padding = new Padding(32, 14, 32, 14) };
        var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };

        var nuova = UiTheme.PrimaryButton("NUOVA CARRIERA");
        nuova.Width = 230; nuova.Margin = new Padding(0, 0, 10, 0);
        nuova.Click += (_, _) => ChiediNuova();

        var riprendi = UiTheme.PrimaryButton("RIPRENDI SELEZIONATA");
        riprendi.Width = 250; riprendi.Margin = new Padding(0, 0, 10, 0);
        riprendi.Click += (_, _) => Riprendi();

        var archivia = UiTheme.SecondaryButton("ARCHIVIA LA CORRENTE");
        archivia.Width = 230; archivia.Height = 44; archivia.Margin = new Padding(0, 0, 10, 0);
        archivia.Click += (_, _) => Archivia();

        var cancella = UiTheme.SecondaryButton("CANCELLA DALL'ARCHIVIO");
        cancella.Width = 240; cancella.Height = 44; cancella.Margin = new Padding(0, 0, 10, 0);
        cancella.ForeColor = UiTheme.Accent;
        cancella.Click += (_, _) => Cancella();

        // Cancellare quella che si sta giocando e' il caso piu' frequente e non
        // c'era: l'elenco selezionabile contiene solo le carriere archiviate.
        var elimina = UiTheme.SecondaryButton("ELIMINA QUELLA IN CORSO");
        elimina.Width = 240; elimina.Height = 44; elimina.Margin = new Padding(0, 0, 10, 0);
        elimina.ForeColor = UiTheme.Accent;
        elimina.Click += (_, _) => EliminaCorrente();

        var chiudi = UiTheme.SecondaryButton("CHIUDI");
        chiudi.Width = 130; chiudi.Height = 44;
        chiudi.Click += (_, _) => { Azione = CareerManagerAction.Niente; DialogResult = DialogResult.Cancel; };

        barra.Controls.AddRange([nuova, riprendi, archivia, cancella, elimina, chiudi]);
        footer.Controls.Add(barra);

        Controls.Add(centro); Controls.Add(footer); Controls.Add(header);
        Carica();
    }

    private void Carica()
    {
        elenco.Items.Clear();
        if (!Directory.Exists(cartella)) { MostraDettaglio(); return; }
        foreach (var file in Directory.EnumerateFiles(cartella, "*.json").OrderByDescending(File.GetLastWriteTime))
        {
            var stato = Leggi(file);
            var voce = new ListViewItem(Path.GetFileNameWithoutExtension(file)) { Tag = file };
            if (stato == null)
            {
                voce.SubItems.AddRange(["—", "salvataggio non leggibile", "—", "—",
                    File.GetLastWriteTime(file).ToString("dd/MM/yyyy HH:mm")]);
                voce.ForeColor = UiTheme.TextMuted;
            }
            else
            {
                voce.SubItems.AddRange([
                    string.IsNullOrWhiteSpace(stato.Driver) ? "—" : stato.Driver,
                    Posizione(stato),
                    stato.Races.ToString(),
                    stato.Wins.ToString(),
                    File.GetLastWriteTime(file).ToString("dd/MM/yyyy HH:mm")
                ]);
            }
            elenco.Items.Add(voce);
        }
        MostraDettaglio();
    }

    private static string Posizione(CareerState stato)
    {
        var campionato = string.IsNullOrWhiteSpace(stato.Championship) ? "campionato non dichiarato" : stato.Championship;
        var categoria = string.IsNullOrWhiteSpace(stato.Tier) ? "" : $"{stato.Tier} · ";
        return $"{categoria}{campionato} · livello {ChampionshipLadder.Clamp(stato.ChampionshipLevel)} di {ChampionshipLadder.Levels}";
    }

    private string Descrivi(CareerState stato)
    {
        var titoli = (stato.SeasonArchive ?? []).Count(x => x.TitleWon);
        var squadra = string.IsNullOrWhiteSpace(stato.Team) ? "senza contratto" : stato.Team;
        return $"{(string.IsNullOrWhiteSpace(stato.Driver) ? "Pilota senza nome" : stato.Driver)} · {squadra}\n"
               + $"{Posizione(stato)}\n"
               + $"{stato.Races} gare · {stato.Wins} vittorie · {stato.Podiums} podi · "
               + $"{(stato.SeasonArchive ?? []).Count} stagioni concluse · {titoli} titoli · € {stato.Cash:N0} in cassa";
    }

    private void MostraDettaglio()
    {
        if (elenco.SelectedItems.Count != 1)
        {
            dettaglio.Text = elenco.Items.Count == 0
                ? "Non c'è nessuna carriera messa da parte. Quando ne comincerai una nuova, quella attuale finirà qui e potrai riprenderla quando vuoi."
                : "Seleziona una carriera per vederne la scheda. Con Ctrl o Maiusc puoi selezionarne più di una per cancellarle insieme.";
            return;
        }
        var stato = Leggi(elenco.SelectedItems[0].Tag as string ?? "");
        dettaglio.Text = stato == null
            ? "Questo salvataggio non è leggibile: può essere stato scritto da una versione precedente o essersi danneggiato."
            : Descrivi(stato);
    }

    private static CareerState? Leggi(string file)
    {
        try
        {
            if (!File.Exists(file)) return null;
            return JsonSerializer.Deserialize<CareerState>(File.ReadAllText(file),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    private void ChiediNuova()
    {
        var haStoria = corrente.Races > 0 || (corrente.RaceHistory?.Count ?? 0) > 0;
        var messaggio = haStoria
            ? $"Cominciare una carriera nuova?\n\nLa carriera di {corrente.Driver} ({corrente.Races} gare) verrà archiviata "
              + "e potrai riprenderla da questa schermata quando vuoi."
            : "Cominciare una carriera nuova?";
        if (CareerMessages.Ask(this, messaggio, "Nuova carriera", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes) return;
        Azione = CareerManagerAction.Nuova;
        DialogResult = DialogResult.OK;
    }

    private void Riprendi()
    {
        if (elenco.SelectedItems.Count != 1)
        {
            CareerMessages.Show(this, "Seleziona una sola carriera da riprendere.", "Gestione carriere");
            return;
        }
        var stato = Leggi(elenco.SelectedItems[0].Tag as string ?? "");
        if (stato == null)
        {
            CareerMessages.Show(this, "Questo salvataggio non è leggibile e non può essere ripreso.",
                "Gestione carriere", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var haStoria = corrente.Races > 0 || (corrente.RaceHistory?.Count ?? 0) > 0;
        if (haStoria && CareerMessages.Ask(this,
                $"Riprendere la carriera di {stato.Driver}?\n\nQuella in corso ({corrente.Driver}, {corrente.Races} gare) "
                + "verrà archiviata prima, quindi non la perdi.",
                "Riprendi carriera", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes) return;
        Scelta = stato;
        Azione = CareerManagerAction.Riprendi;
        DialogResult = DialogResult.OK;
    }

    private void Archivia()
    {
        var proposto = $"{(string.IsNullOrWhiteSpace(corrente.Driver) ? "carriera" : corrente.Driver)} · {corrente.Races} gare";
        var nome = ChiediNome(proposto);
        if (string.IsNullOrWhiteSpace(nome)) return;
        NomeArchivio = nome;
        Azione = CareerManagerAction.Archivia;
        DialogResult = DialogResult.OK;
    }

    /// <summary>Una richiesta di testo: WinForms non ne ha una, e serviva solo qui.</summary>
    private string ChiediNome(string proposto)
    {
        using var finestra = new Form
        {
            Text = "Nome della copia", ClientSize = new Size(520, 168),
            StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false,
            BackColor = UiTheme.Background, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body
        };
        var casella = new TextBox
        {
            Left = 20, Top = 62, Width = 480, Text = proposto,
            BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle
        };
        var ok = new Button
        {
            Text = "ARCHIVIA", Left = 340, Top = 108, Width = 160, Height = 38,
            DialogResult = DialogResult.OK, BackColor = UiTheme.Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        finestra.Controls.Add(new Label
        {
            Left = 20, Top = 20, Width = 480, Height = 36, ForeColor = UiTheme.TextSecondary,
            Text = "Con che nome vuoi ritrovarla? La carriera in corso resta aperta: questa è una copia."
        });
        finestra.Controls.Add(casella);
        finestra.Controls.Add(ok);
        finestra.AcceptButton = ok;
        return finestra.ShowDialog(this) == DialogResult.OK ? casella.Text.Trim() : "";
    }

    /// <summary>
    /// Butta via la carriera che si sta giocando. E' l'unica azione davvero
    /// distruttiva della schermata, quindi chiede due volte: la prima per
    /// sapere se archiviarla prima, la seconda per confermare.
    /// </summary>
    private void EliminaCorrente()
    {
        var haStoria = corrente.Races > 0 || (corrente.RaceHistory?.Count ?? 0) > 0;
        var chi = string.IsNullOrWhiteSpace(corrente.Driver) ? "questa carriera" : $"la carriera di {corrente.Driver}";
        var quanto = haStoria
            ? $" ({corrente.Races} gare, {corrente.Wins} vittorie, {(corrente.SeasonArchive ?? []).Count} stagioni concluse)"
            : "";

        if (haStoria)
        {
            var salva = CareerMessages.Ask(this,
                $"Vuoi conservarne una copia prima di eliminarla?\n\nStai per buttare via {chi}{quanto}.\n\n"
                + "Sì = archiviala, così puoi riprenderla quando vuoi\n"
                + "No = eliminala e basta",
                "Elimina la carriera in corso", MessageBoxButtons.YesNoCancel, DialogResult.Cancel);
            if (salva == DialogResult.Cancel) return;
            if (salva == DialogResult.Yes)
            {
                var nome = ChiediNome($"{(string.IsNullOrWhiteSpace(corrente.Driver) ? "carriera" : corrente.Driver)} · {corrente.Races} gare");
                if (string.IsNullOrWhiteSpace(nome)) return;
                NomeArchivio = nome;
            }
        }

        if (CareerMessages.Ask(this,
                $"Eliminare definitivamente {chi}{quanto}?\n\n"
                + (string.IsNullOrWhiteSpace(NomeArchivio)
                    ? "Non ne resterà nessuna copia e non si può annullare."
                    : $"Resterà la copia archiviata «{NomeArchivio}».")
                + "\n\nSubito dopo ti verrà chiesto di crearne una nuova.",
                "Elimina la carriera in corso", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
        {
            NomeArchivio = "";
            return;
        }

        Azione = CareerManagerAction.EliminaCorrente;
        DialogResult = DialogResult.OK;
    }

    private void Cancella()
    {
        var file = elenco.SelectedItems.Cast<ListViewItem>().Select(x => x.Tag as string ?? "").Where(File.Exists).ToList();
        if (file.Count == 0)
        {
            CareerMessages.Show(this, "Seleziona almeno una carriera da cancellare.", "Gestione carriere");
            return;
        }
        var nomi = string.Join("\n", file.Select(x => "· " + Path.GetFileNameWithoutExtension(x)));
        if (CareerMessages.Ask(this,
                $"Cancellare definitivamente {file.Count} carriera/e?\n\n{nomi}\n\n"
                + "La carriera in corso non viene toccata. L'operazione non si può annullare.",
                "Cancella carriere", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes) return;

        var falliti = new List<string>();
        foreach (var f in file)
        {
            try { File.Delete(f); }
            catch (Exception errore) { falliti.Add($"{Path.GetFileName(f)}: {errore.Message}"); }
        }
        Carica();
        CareerMessages.Show(this,
            falliti.Count == 0
                ? $"Cancellate {file.Count} carriera/e."
                : $"Cancellate {file.Count - falliti.Count} su {file.Count}.\n\nNon riuscite:\n{string.Join("\n", falliti)}",
            "Gestione carriere", MessageBoxButtons.OK,
            falliti.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }
}
