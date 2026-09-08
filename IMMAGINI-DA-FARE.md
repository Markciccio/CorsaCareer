# Tavole da produrre — istruzioni per codex

Il motore è pronto e cerca **questi nomi esatti** in `assets/`.
Finché un file non c'è, la scena ripiega su una tavola presente e funziona lo
stesso: si possono aggiungere una alla volta, senza toccare il codice.

Verificato dal codice, non ipotizzato: l'elenco vive in `SceneArtwork.Expected()`
e un test (`SceneArtworkCheck`) controlla che resti allineato.

---

## Specifiche tecniche

- **Formato**: PNG, `1536 × 1024` (come tutte le tavole esistenti)
- **Cartella**: `assets/` — nomi in minuscolo, senza spazi, esattamente come sotto
- **Stile**: coerente con le tavole già presenti — manga/anime, tratto a china,
  colore desaturato, luce naturale. Guardare `manga-01-rookie-dawn.png` e
  `manga-sponsor-table.png` come riferimento.
- **Ambientazione**: Giappone di provincia. Il pilota parte da Suzuka.
- **Niente marchi reali**: le aziende sono inventate. Nessun logo esistente,
  nessun nome di azienda vera sulle insegne.

---

## 1. Le visite di Haru — priorità alta

Haru Senda è il procuratore: gira a cercare chi finanzia la stagione. Sono le
scene che si vedono più spesso, perché lavora tutti i giorni.

**Haru**: ragazzo giovane, camicia, cartellina sotto il braccio. Il riferimento
è `character-haru-senda.png`.

| file | scena da rappresentare |
|---|---|
| `haru-visita-officina.png` | Officina di quartiere. Il meccanico ascolta pulendosi le mani con uno straccio, senza smettere di lavorare. |
| `haru-visita-ramen.png` | Piccola trattoria di ramen. Il proprietario dietro il bancone, vapore, pochi coperti. |
| `haru-visita-sushi.png` | Locale di sushi o poke. Vetrina refrigerata, il titolare che prepara mentre ascolta. |
| `haru-visita-fruttivendolo.png` | Banco di frutta e verdura all'aperto. Il negoziante scettico, cassette impilate. |
| `haru-visita-ferramenta.png` | Ferramenta stretta, scaffali fino al soffitto. Il titolare misura le parole. |
| `haru-visita-gommista.png` | Gommista. Pile di pneumatici, ambiente che le corse le conosce già. |
| `haru-visita-assicurazioni.png` | Ufficio ordinato, scrivania grande, aria formale. Qui la risposta è quasi sempre no. |
| `haru-rifiuto-porta.png` | Haru sulla soglia, la porta che si sta chiudendo. Il no prima delle parole. |
| `haru-accordo-stretta-mano.png` | La stretta di mano che chiude l'accordo. Sollievo più che trionfo: sono poche centinaia di euro. |

## 2. Le attività del pilota — nessuna esiste

Otto attività, zero illustrazioni. Il pilota è il protagonista: stessa persona
delle tavole `manga-01-rookie-dawn.png` e `manga-08-financial-crisis.png`.

| file | scena da rappresentare |
|---|---|
| `attivita-palestra.png` | Palestra spartana. Lavoro su collo e core, non sollevamento pesi da culturista. |
| `attivita-corsa.png` | Corsa all'aperto la mattina presto, strada di provincia. |
| `attivita-massaggio.png` | Seduta dal fisioterapista. Lettino, mani sulla schiena. |
| `attivita-riposo.png` | A casa, a non fare niente. È anche il gesto di rinunciare a fare. |
| `attivita-tifosi.png` | Incontro con i tifosi: poche persone, qualche foto. Non una folla. |
| `attivita-social.png` | Il pilota che scrive un post al telefono, la sera, luce dello schermo sul viso. |
| `attivita-pr.png` | Relazioni pubbliche in un circolo locale. Strette di mano, poca sostanza. |
| `attivita-lavoro.png` | Giornata di lavoro in officina per mettere insieme qualche soldo. Non è la sua officina. |

## 3. Le decisioni

| file | scena da rappresentare |
|---|---|
| `decisione-corri-o-salta.png` | Il giorno della gara: il pilota davanti al kart con la busta dell'iscrizione in mano. Deve pesare. |
| `decisione-calendario.png` | Il pilota davanti al calendario, a fare i conti su cosa può permettersi. |

## 4. Squilibrio da colmare

Copertura attuale: **kart 17 · monoposto 9 · vetture chiuse 13**.
La monoposto è la più povera, e **della Formula 1 non c'è niente** — pur essendo
una delle tre vette raggiungibili nella carriera.

| file | scena da rappresentare |
|---|---|
| `manga-formula1-debutto-griglia.png` | Il debutto in Formula 1, la griglia vista dall'abitacolo. Monoposto moderna. |
| `manga-formula1-primo-podio.png` | Il podio in Formula 1. |
| `manga-formula2-ultimo-gradino.png` | Formula internazionale: l'anticamera. Venti piloti, due posti veri più in alto. |

---

## Come registrare una tavola nel catalogo

Dopo aver messo il file in `assets/`, aggiungere una voce in
`IllustrationCatalog.cs`, dentro il dizionario:

```csharp
, ["haru-visita-ramen.png"] = new(
    "Haru alla trattoria",                       // titolo
    "sponsor, trattativa, ramen, quartiere",     // tag
    "",                                          // MEZZI VISIBILI
    "Haru presenta il progetto al proprietario di una piccola trattoria.")
```

### Il campo che conta: **mezzi visibili** (terzo parametro)

È l'unico che il filtro legge per decidere dove una tavola può comparire.
Sbagliarlo rimette una monoposto sopra una gara di kart — che è esattamente il
difetto che questo campo esiste per impedire.

| cosa si vede nella tavola | cosa scrivere |
|---|---|
| nessun mezzo riconoscibile | **lasciare vuoto** `""` — la rende utilizzabile ovunque |
| un kart | `"kart da competizione"` |
| una monoposto | `"monoposto a ruote scoperte"` |
| una GT, turismo, prototipo | `"vettura GT3"` / `"vettura da turismo"` / `"prototipo da endurance"` |

**Regola**: dichiarare un mezzo solo se si vede davvero. Nel dubbio, lasciare
vuoto — una tavola neutra è utilizzabile ovunque, una classificata male finisce
dove non deve.

Per tutte le tavole di questo elenco **il campo va lasciato vuoto**, tranne:

- `attivita-lavoro.png` → `"kart"` solo se nell'immagine se ne vede uno
- `decisione-corri-o-salta.png` → `"kart da competizione"`
- le tre di Formula → `"monoposto di Formula 1"` / `"monoposto di formula internazionale"`

---

## Verifica

Dopo aver aggiunto file e voci:

```powershell
.\verifica.ps1
```

Il collaudo controlla che ogni tavola dichiarata esista, che i percorsi puntino
dentro `assets/`, e che nessuna categoria riceva l'illustrazione di un'altra.
