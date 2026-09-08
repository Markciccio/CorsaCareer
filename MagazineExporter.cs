using System.Net;
using System.Text;

namespace CorsaCareer;

public static class MagazineExporter
{
    public static void Export(CareerState career, string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            foreach (var (story, index) in career.Events.Select((item, index) => (item, index)))
            {
                var date = story.StoryDate == default ? story.DateUtc.ToLocalTime() : story.StoryDate;
                // L'archivio veniva riscritto INTERO a ogni salvataggio, cioè
                // dopo ogni azione del giocatore: per ogni evento una copia di
                // immagine, un articolo ricomposto e un file riscritto. Il
                // costo cresceva con la carriera, ed era la causa dei blocchi
                // sempre più lunghi. Un evento già archiviato non cambia più:
                // qui si scrive solo ciò che manca.
                var destinazione = Path.Combine(directory, $"{date:yyyyMMdd}-{index + 1:000}-{Sanitize(story.Type)}.html");
                if (File.Exists(destinazione)) continue;
                var title = string.IsNullOrWhiteSpace(story.Headline) ? "Diario dal paddock" : story.Headline;
                var realPhoto = !string.IsNullOrWhiteSpace(story.PhotoPath) && File.Exists(story.PhotoPath) ? story.PhotoPath : "";
                var illustration = EditorialAssets.ForEvent(story);
                var sourcePhoto = realPhoto.Length > 0 ? realPhoto : File.Exists(illustration) ? illustration : "";
                // Le tavole non si duplicano: si collegano.
                //
                // Ogni articolo copiava accanto a sé la propria illustrazione,
                // che è un file del programma da quattro megabyte e mezzo. Con
                // cinquecento eventi l'archivio di UNA carriera arrivava a un
                // gigabyte e duecento di immagini identiche a quelle già
                // installate, e cresceva a ogni apertura del portale. Le foto
                // vere — gli screenshot presi in pista — restano invece copiate:
                // quelle appartengono alla carriera e non esistono altrove.
                var imageSource = "";
                if (realPhoto.Length > 0)
                {
                    var imageName = $"{date:yyyyMMdd}-{index + 1:000}-photo{Path.GetExtension(realPhoto).ToLowerInvariant()}";
                    File.Copy(realPhoto, Path.Combine(directory, imageName), true);
                    imageSource = imageName;
                }
                else if (sourcePhoto.Length > 0)
                {
                    imageSource = new Uri(Path.GetFullPath(sourcePhoto)).AbsoluteUri;
                }
                var image = imageSource.Length > 0
                    ? $"<figure><img src='{WebUtility.HtmlEncode(imageSource)}' alt='Immagine editoriale' /><figcaption class='caption'>{WebUtility.HtmlEncode(PhotoSource.Caption(story.PhotoPath, story.Type))}{(realPhoto.Length > 0 && story.Type.Equals("SCREENSHOT_CAPTURED", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(story.PhotoView) ? $" · visuale {WebUtility.HtmlEncode(story.PhotoView)}" : "")}</figcaption></figure>"
                    : "<p class='caption'>Nessuna immagine disponibile per questo evento.</p>";
                var race = NewspaperDialog.ResolveRace(career, story);
                var arc = career.StoryArcs.OrderByDescending(x => x.Importance).FirstOrDefault(x => x.Status == "In corso");
                var article = NewspaperDialog.BuildArticle(career, story, race, arc).Replace("\r\n", "\n");
                var paragraphs = string.Join("", article.Split("\n\n", StringSplitOptions.RemoveEmptyEntries).Select(p => $"<p>{WebUtility.HtmlEncode(p).Replace("\n", "<br>")}</p>"));
                var html = $"<!doctype html><html lang='it'><meta charset='utf-8'><title>{WebUtility.HtmlEncode(title)}</title><style>body{{background:#151923;color:#eee;font-family:Segoe UI,Arial;margin:0;padding:40px}}main{{max-width:820px;margin:auto;background:#252c3b;padding:42px;border-top:5px solid #c01b43}}h1{{color:#f5be41;font-size:34px}}.date{{color:#b8c0cf;text-transform:uppercase;letter-spacing:2px}}p{{line-height:1.65}}figure{{margin:24px 0}}figure img{{display:block;max-width:100%;max-height:420px;margin:auto;object-fit:contain}}.caption{{color:#cbd2df;font-size:13px;border-left:3px solid #c01b43;padding-left:12px}}</style><main><div class='date'>{date:dddd d MMMM yyyy} · {WebUtility.HtmlEncode(story.Type)}</div><h1>{WebUtility.HtmlEncode(title)}</h1><p>{WebUtility.HtmlEncode(career.Driver)} · {WebUtility.HtmlEncode(career.Team)} · {WebUtility.HtmlEncode(story.Track)}</p>{image}{paragraphs}</main></html>";
                File.WriteAllText(destinazione, html, Encoding.UTF8);
            }
        }
        catch { }
    }

    private static string Sanitize(string value) => string.Join("_", (value ?? "evento").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
}
