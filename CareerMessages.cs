using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Gli avvisi del programma, in un punto solo.
///
/// Una finestra modale ferma il programma finché qualcuno non clicca. Va bene
/// quando c'è una persona davanti allo schermo; non va bene quando il
/// programma viene percorso da un banco di prova, dove nessuno può cliccare e
/// tutto resta appeso per sempre.
///
/// La guardia esisteva già, ma solo in alcuni punti: sessantaquattro avvisi
/// sparsi nel codice, e bastava che uno solo non l'avesse per rendere
/// impossibile qualunque verifica automatica del percorso di carriera.
///
/// Qui il divieto sta in un posto solo. Ogni avviso nuovo lo eredita senza che
/// nessuno debba ricordarsene.
/// </summary>
public static class CareerMessages
{
    /// <summary>
    /// Vero quando il programma viene percorso automaticamente: nessuno può
    /// rispondere a una finestra modale.
    /// </summary>
    public static bool Unattended =>
        string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal);

    /// <summary>
    /// Gli avvisi mostrati mentre il programma girava senza nessuno davanti.
    /// Non vanno persi: sono quello che il giocatore avrebbe letto, ed è
    /// l'informazione più utile per capire cosa succede in un percorso.
    /// </summary>
    public static List<string> Suppressed { get; } = [];

    /// <summary>
    /// Mostra un avviso. Senza nessuno davanti lo registra invece di fermare
    /// tutto, e restituisce la risposta predefinita indicata.
    /// </summary>
    public static DialogResult Show(IWin32Window? owner, string text, string caption,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.Information,
        DialogResult unattendedAnswer = DialogResult.OK)
    {
        if (Unattended)
        {
            Suppressed.Add($"[{caption}] {Compact(text)}");
            return unattendedAnswer;
        }
        var finestra = owner ?? FinestraInPrimoPiano();
        return finestra == null
            ? MessageBox.Show(text, caption, buttons, icon)
            : MessageBox.Show(finestra, text, caption, buttons, icon);
    }

    /// <summary>
    /// La finestra a cui agganciare un avviso quando chi chiama non ne indica una.
    ///
    /// Un MessageBox senza proprietario non appartiene a nessuna finestra: con
    /// l'applicazione a tutto schermo puo' comparire DIETRO, e siccome e' modale
    /// blocca tutto. Da fuori si vede un pulsante che non fa niente e un
    /// programma che non risponde piu' — ed e' proprio il difetto che continuava
    /// a saltare fuori: «accetto la firma e non succede nulla». Il messaggio
    /// c'era, era solo nascosto sotto la schermata principale.
    /// </summary>
    private static IWin32Window? FinestraInPrimoPiano()
    {
        try
        {
            if (Form.ActiveForm != null) return Form.ActiveForm;
            for (var i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                var form = Application.OpenForms[i];
                if (form is { IsDisposed: false, Visible: true }) return form;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Una domanda con risposta. Senza nessuno davanti risponde con il valore
    /// indicato: la scelta va dichiarata da chi chiama, non indovinata qui.
    /// </summary>
    public static DialogResult Ask(IWin32Window? owner, string text, string caption,
        MessageBoxButtons buttons, DialogResult unattendedAnswer)
        => Show(owner, text, caption, buttons, MessageBoxIcon.Question, unattendedAnswer);

    /// <summary>Il testo su una riga sola, per il registro.</summary>
    private static string Compact(string text)
    {
        var flat = (text ?? "").Replace("\r", " ").Replace("\n", " ");
        while (flat.Contains("  ")) flat = flat.Replace("  ", " ");
        flat = flat.Trim();
        return flat.Length <= 160 ? flat : flat[..160] + "…";
    }
}
