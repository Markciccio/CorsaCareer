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


---

# Richiesta del 11 settembre 2026 — misurata, non stimata

Questo blocco nasce dal collaudo `--contenuti`, che conta davvero quali
combinazioni di disciplina e momento hanno una tavola e quali no:

```powershell
dotnet run --project tests\CareerSim\CareerSim.csproj -c Debug -- --contenuti
```

Rieseguirlo dopo aver aggiunto le tavole dice quante ne restano.

## 1. I due ritratti mancanti — PRIORITÀ ALTA (8 file)

Sono l'unica cosa che si **vede** come un difetto: dove manca il ritratto la
scena mostrava un rettangolo nero grande mezzo schermo. Adesso ripiega su una
tavola di gruppo, che è meglio ma non è un primo piano come hanno tutti gli
altri personaggi.

**Sae Kurihara** — compagna di classe del pilota, sedici anni, corre nella sua
stessa categoria. Diretta e pratica, non si commuove facilmente, tiene i
capelli corti perché il casco. Non è un interesse amoroso: è una collega.

| File | Espressione |
|---|---|
| `character-sae-kurihara.png` | neutra, sguardo dritto |
| `character-sae-kurihara-felice.png` | contenta ma composta, mezzo sorriso |
| `character-sae-kurihara-decisa.png` | concentrata prima di scendere in pista |
| `character-sae-kurihara-preoccupata.png` | preoccupata per l'amico, non per sé |

**Tooru Inagaki** — compagno di classe, non guida e non gli interessa: tiene i
conti. Occhiali, quaderno sempre in mano, impacciato con le persone e
implacabile con i numeri.

| File | Espressione |
|---|---|
| `character-tooru-inagaki.png` | neutro, quaderno in mano |
| `character-tooru-inagaki-sollevato.png` | sollievo dopo un sì |
| `character-tooru-inagaki-deluso.png` | dopo un rifiuto, sguardo basso |
| `character-tooru-inagaki-imbarazzato.png` | mentre dice una cifra che fa male |

**Formato ritratti**: come gli altri `character-*` già presenti — **1152 × 1152**
(oppure 1024 × 1536 verticale), mezzo busto, sfondo di contesto sfocato.

## 2. Le combinazioni scoperte — 21 tavole

Il collaudo le elenca per nome. Vanno taggate nel catalogo con **disciplina e
momento**, altrimenti il motore non le trova: è quello il criterio, non il nome
del file.

- **`scuola` in ogni disciplina (7 tavole)** — è il momento nuovo e non ha
  nemmeno una tavola: aula giapponese, cortile, corridoio, palestra della
  scuola. Il pilota in divisa scolastica, con Sae e Tooru.
- **`campionato` in formula-minore, formula-alta, formula-vertice, gt,
  endurance, turismo (6 tavole)** — la fine di un campionato: classifica sul
  muretto, squadra che conta i punti, premiazione.
- **`sconfitta` in formula-alta, formula-vertice, gt, endurance, turismo
  (5 tavole)** — la domenica storta in quella categoria.
- **`formula-vertice`: vittoria, trattativa, officina (3 tavole)** — il gradino
  più alto è quasi scoperto, e ora la carriera ci arriva davvero.

Priorità fra queste: prima le sette di `scuola` (momento nuovo, zero
copertura), poi le tre di `formula-vertice`.

## 3. La cittadina — OPZIONALE, valutare se ne vale la pena

La mappa del minigioco è **disegnata a codice** con GDI+ (`TownWalkDialog`):
strade, isolati, canale, ponti, passaggio a livello, alberi, insegne. Funziona
ed è leggibile, ma è geometria colorata in mezzo a un gioco fatto di tavole
disegnate a mano.

Per sostituirla servirebbero:

- **1 tileset** PNG a griglia 32×32 con: strada dritta / incrocio / curva,
  marciapiede, 4 varianti di edificio visto dall'alto, tetto, albero, prato,
  acqua, ponte, binari, passaggio a livello aperto e chiuso, insegna negozio —
  circa **24 caselle**.
- **1 spritesheet** del personaggio visto dall'alto, 4 direzioni × 3 fotogrammi
  = **12 pose**, 32×32 ciascuna.

Tutto il disegno sta in due metodi (`DisegnaCella` e `DisegnaPersona`): il
passaggio ai tile non tocca la logica. Ma è un tipo di asset diverso da tutti
gli altri, e la mappa così com'è non è un difetto — è solo meno bella del
resto. **Da fare per ultimo, se avanza tempo.**



---

# Stato al 11 settembre 2026 — dopo la consegna di Codex

Arrivati **50 file**. Non tutti utilizzabili: vale la pena leggere come e'
andata, perche' la prossima consegna puo' evitare gli stessi tre inciampi.

**Cosa e' stato tenuto: 27 file.** Convertiti in JPEG come tutto il resto della
cartella (erano PNG da 3 MB l'uno: 138 MB diventati 15).

**Cosa e' stato ripristinato: 23 file.** Codex ha ridisegnato tavole che
esistevano gia' e funzionavano, sovrascrivendole con soggetti diversi e in un
caso sbagliati: `attivita-palestra` era un pilota che si scalda in palestra fra
i bilancieri ed e' diventata un ragazzo in camera sua davanti alle bollette —
che e' la scena della cassa vuota, non dell'allenamento. Le vecchie sono state
rimesse da git. **Non ridisegnare file gia' presenti** senza che siano stati
chiesti.

**Cosa e' stato rinominato: 7 file.** Le sette tavole `scuola-*` non mostrano
la scuola: sono paddock, tende, gare sotto la pioggia. Codex ha letto «scuola»
come scuola di pilotaggio. Sono belle e utili, quindi sono diventate
`paddock-<disciplina>` e restano in rotazione come tavole di categoria — ma il
momento «scuola» resta scoperto.

**Il catalogo non le trovava.** Il classificatore non conosceva le parole
`scuola`, `sconfitta`, `formula-alta` e `formula-vertice`: un file chiamato
`campionato-formula-vertice` finiva etichettato *formula-minore*, cioe' la
tavola della Formula 1 sarebbe comparsa sopra una Formula 4. Corretto.

Risultato: combinazioni scoperte da **21 a 9**, e i due personaggi hanno il
loro ritratto — ottimi, e coerenti con lo stile della cartella.

## Cosa resta da disegnare

### 1. Le sette scene scolastiche (priorita' alta)

Sono il momento nuovo e sono ancora a zero. **Ambientazione: dentro la
scuola** — aula giapponese con i banchi, corridoio con gli armadietti,
cortile, tetto dell'edificio. Il pilota in **divisa scolastica**, non in tuta.
Sae e Tooru con lui.

| File | Cosa mostra |
|---|---|
| `scuola-kart.jpg` | aula, lunedi' mattina: il pilota assonnato al banco, Sae che lo sveglia |
| `scuola-formula-minore.jpg` | cortile all'intervallo, si parla della gara di domenica |
| `scuola-formula-alta.jpg` | corridoio: i compagni lo riconoscono, lui e' a disagio |
| `scuola-formula-vertice.jpg` | l'aula vuota: ormai ci torna di rado |
| `scuola-gt.jpg` | tetto della scuola, Sae e il pilota mangiano il bento |
| `scuola-endurance.jpg` | biblioteca o aula studio, Tooru che rifa' i conti |
| `scuola-turismo.jpg` | uscita di scuola, cancello, biciclette |

### 2. Due campionati (priorita' alta)

| File | Cosa mostra |
|---|---|
| `campionato-endurance.jpg` | fine di un campionato endurance: la squadra al muretto che conta i punti, l'alba dopo l'ultima gara |
| `campionato-turismo.jpg` | fine di un campionato turismo: premiazione, classifica, vetture nel parco chiuso |

### 3. Il pilota non ha una faccia (priorita' alta)

Quando e' il pilota a parlare in una scena della giornata, il gioco ripiegava
sul ritratto di Genji: il protagonista parlava con la faccia del suo meccanico.
Adesso il codice usa prima il ritratto importato dal giocatore, e in mancanza
cerca questi due:

| File | Cosa mostra |
|---|---|
| `character-pilota.jpg` | ritratto neutro del protagonista, mezzo busto, tuta slacciata, sfondo paddock sfocato. **Volto di tre quarti o in parte in ombra**: deve valere per qualunque nome scelga il giocatore |
| `character-pilota-casco.jpg` | stesso soggetto col casco in mano, visiera che copre parte del viso |

1152 x 1152 come gli altri ritratti.

### 4. Sae e Tooru in divisa (priorita' media)

I ritratti consegnati sono ottimi ma hanno lo sfondo del circuito: Sae in tuta
al kartodromo, Tooru in giacca a vento nel paddock. Nelle scene scolastiche il
dialogo e' in aula, e il contrasto si vede.

| File | Cosa mostra |
|---|---|
| `character-sae-kurihara-scuola.jpg` | in divisa scolastica, aula sullo sfondo |
| `character-tooru-inagaki-scuola.jpg` | in divisa, quaderno in mano, corridoio |

### 5. La cittadina — resta opzionale

Tileset 32x32 da ~24 caselle piu' spritesheet del personaggio (4 direzioni x 3
fotogrammi). La mappa disegnata a codice funziona: e' solo meno bella del
resto.

## Tre regole per la prossima consegna

1. **JPEG, qualita' 85-90.** Non PNG: cinquanta PNG da soli pesavano 138 MB
   contro i 15 degli stessi file in JPEG, e la cartella e' gia' a 250 MB.
2. **Non toccare i file che esistono gia'.** Se un nome e' occupato, il file
   c'e' e funziona: aggiungere, non sostituire.
3. **Il nome decide dove finisce la tavola.** Il motore ricava i tag dal nome
   del file, quindi `sconfitta-gt.jpg` funziona e `gt-brutta-giornata.jpg` no.
   E il soggetto deve corrispondere al nome: una tavola chiamata `scuola-kart`
   deve mostrare una scuola.

Il collaudo dice sempre che cosa manca:

```powershell
dotnet run --project tests/CareerSim/CareerSim.csproj -c Debug -- --contenuti
```
