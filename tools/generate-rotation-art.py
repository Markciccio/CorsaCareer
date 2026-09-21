from pathlib import Path
import csv, hashlib, random
from PIL import Image, ImageEnhance, ImageOps, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "assets"
OUT = ASSETS / "variants"
OUT.mkdir(parents=True, exist_ok=True)

# Le varianti sono derivate da tavole già coerenti con il mondo di CorsaCareer:
# cambiano inquadratura, luce e trattamento manga, ma non introducono soggetti
# casuali. Sono volutamente piccole (320x180, JPEG progressivo) per il pannello.
groups = {
    "sponsor": (40, ["anime-haru-sponsor-sushi-poke-incontro.jpg", "anime-haru-sponsor-fruttivendolo-proposta.jpg", "anime-haru-sponsor-ferramenta-visita.jpg", "anime-haru-telefonata-sponsor-stazione.jpg", "haru-visita-assicurazioni.jpg", "haru-accordo-stretta-mano-nuovo.jpg"]),
    "school": (30, ["kart-215-rookie-studying-map.jpg", "character-sae-kurihara-scuola.jpg", "character-tooru-inagaki-scuola.jpg", "scuola-kart.jpg", "anime-calendario-campionato-garage.jpg"]),
    "training": (60, ["kart-096-allenamento-team.jpg", "activity-pilota-palestra.jpg", "activity-pilota-corsa-aperto.jpg", "anime-kart-controllo-gomme-tecnico.jpg", "anime-kart-qualifica-estate.jpg", "kart-014-dati-telemetria.jpg"]),
    "social": (25, ["activity-pilota-social.jpg", "activity-pilota-pr.jpg", "anime-pilota-attivita-social-post.jpg", "anime-pilota-attivita-relazioni-pubbliche.jpg", "anime-stampa-dopo-vittoria.jpg"]),
    "recovery": (20, ["activity-pilota-riposo-casa.jpg", "activity-pilota-massaggio-fisioterapia.jpg", "anime-pilota-attivita-fisioterapia.jpg", "activity-pilota-lavoro.jpg"]),
    "race": (25, ["anime-kart-gara-pioggia.jpg", "anime-kart-duello-rivale-curva.jpg", "anime-kart-vittoria-checkered-flag.jpg", "anime-formula4-rookie-test-suzuka.jpg", "anime-gt-test-pioggia.jpg", "anime-touring-car-fuji-gara.jpg"]),
}

def resolve(name: str) -> Path | None:
    p = ASSETS / name
    if p.exists(): return p
    alt = p.with_suffix(".jpg")
    return alt if alt.exists() else None

def make_variant(src: Path, seed: int) -> Image.Image:
    rng = random.Random(seed)
    with Image.open(src) as im:
        im = im.convert("RGB")
        # Crop diversi ma sempre leggibili nel rapporto del pannello.
        x = rng.uniform(0.25, 0.75)
        y = rng.uniform(0.38, 0.62)
        out = ImageOps.fit(im, (320, 180), method=Image.Resampling.LANCZOS,
                           centering=(x, y))
        if rng.random() < .28:
            out = ImageOps.mirror(out)
        out = ImageEnhance.Brightness(out).enhance(rng.uniform(.91, 1.08))
        out = ImageEnhance.Contrast(out).enhance(rng.uniform(.94, 1.12))
        if rng.random() < .35:
            out = ImageEnhance.Color(out).enhance(rng.uniform(.82, 1.12))
        if rng.random() < .22:
            out = out.filter(ImageFilter.UnsharpMask(radius=.45, percent=90, threshold=3))
        return out

rows = []
for category, (count, names) in groups.items():
    sources = [resolve(n) for n in names]
    sources = [p for p in sources if p]
    if not sources:
        raise SystemExit(f"No source for {category}")
    for i in range(1, count + 1):
        src = sources[(i - 1) % len(sources)]
        seed = int(hashlib.sha256(f"{category}:{i}".encode()).hexdigest()[:12], 16)
        name = f"{category}-{i:03d}.jpg"
        dst = OUT / name
        make_variant(src, seed).save(dst, "JPEG", quality=62, optimize=True, progressive=True)
        rows.append((f"variants/{name}", category, src.name, "rotazione;320x180;compact"))

with (ASSETS / "variants-catalog.csv").open("w", newline="", encoding="utf-8") as f:
    w = csv.writer(f, delimiter=";")
    w.writerow(["file", "sezione", "sorgente", "tag"])
    w.writerows(rows)
print(f"generated={len(rows)} bytes={sum(p.stat().st_size for p in OUT.glob('*.jpg'))}")
