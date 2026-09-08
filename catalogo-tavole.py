# -*- coding: utf-8 -*-
"""
Strumento per catalogare le tavole d'archivio, un blocco per volta.

Le tavole "anime-archive-NNN" hanno nomi che non dicono niente. Il programma le
distribuisce fra le fasi in modo provvisorio, cosi nessuna resta inutilizzata,
ma la scheda vera si scrive solo guardando l'immagine.

Questo strumento prepara i fogli di contatto da guardare e sa dove il lavoro si
e' interrotto, cosi si riprende senza rifare due volte lo stesso blocco.

    python catalogo-tavole.py stato          quante sono catalogate, quante no
    python catalogo-tavole.py fogli 8 40     prepara i fogli per le tavole 8..40
    python catalogo-tavole.py scheda 8 kart gara "Titolo" "descrizione"

Le schede scritte con "scheda" finiscono in IllustrationCatalog.cs e da quel
momento valgono piu della collocazione automatica.
"""
import io, os, re, sys, glob

RADICE = os.path.dirname(os.path.abspath(__file__))
CATALOGO = os.path.join(RADICE, 'IllustrationCatalog.cs')
TAVOLE = os.path.join(RADICE, 'assets')
FOGLI = os.path.join(RADICE, 'fogli-catalogazione')

DISCIPLINE = {
    'kart': 'kart da gara',
    'formula-minore': 'monoposto junior a ruote scoperte',
    'formula-alta': 'monoposto di categoria superiore a ruote scoperte',
    'formula-vertice': 'monoposto di vertice a ruote scoperte',
    'turismo': 'vettura turismo da gara',
    'gt': 'GT da competizione',
    'endurance': 'prototipo o GT endurance',
    'paddock': 'nessun mezzo obbligatorio',
}
MOMENTI = ['gara', 'test', 'vittoria', 'podio', 'sconfitta', 'officina',
           'trattativa', 'contratto', 'budget', 'rivale', 'attivita-pilota',
           'trasferta', 'tifosi', 'dati']


def catalogate():
    testo = io.open(CATALOGO, encoding='utf-8').read()
    return set(m.group(1).lower() for m in re.finditer(r'\["([^"]+\.png)"\]', testo))


def archivio():
    return sorted(os.path.basename(p) for p in glob.glob(os.path.join(TAVOLE, 'anime-archive-*.png')))


def stato():
    fatte = catalogate()
    tutte = archivio()
    manca = [t for t in tutte if t.lower() not in fatte]
    print('tavole d\'archivio: %d' % len(tutte))
    print('con scheda scritta a mano: %d' % (len(tutte) - len(manca)))
    print('da guardare: %d' % len(manca))
    if manca:
        print('prossima da guardare: %s' % manca[0])
        gruppi = []
        for t in manca:
            n = int(t[14:17])
            if gruppi and n == gruppi[-1][1] + 1:
                gruppi[-1][1] = n
            else:
                gruppi.append([n, n])
        print('intervalli mancanti: ' + ', '.join(
            ('%d' % a) if a == b else ('%d-%d' % (a, b)) for a, b in gruppi))


def fogli(da, a):
    try:
        from PIL import Image
    except ImportError:
        print('serve PIL: pip install pillow')
        return
    os.makedirs(FOGLI, exist_ok=True)
    scelte = [t for t in archivio() if da <= int(t[14:17]) <= a]
    if not scelte:
        print('nessuna tavola in quell\'intervallo')
        return
    # Quattro per foglio, ognuna 320x213: foglio 640x426, ben dentro i limiti
    # di lettura, e il dettaglio resta sufficiente per riconoscere la scena.
    L, A, C = 320, 213, 2
    fatti = 0
    for b in range(0, len(scelte), 4):
        lotto = scelte[b:b + 4]
        foglio = Image.new('RGB', (L * C, A * 2), 'white')
        for k, nome in enumerate(lotto):
            im = Image.open(os.path.join(TAVOLE, nome)).convert('RGB').resize((L, A))
            foglio.paste(im, ((k % C) * L, (k // C) * A))
        primo = int(lotto[0][14:17])
        percorso = os.path.join(FOGLI, 'tavole-%03d.jpg' % primo)
        foglio.save(percorso, quality=84)
        fatti += 1
        print('%s  ->  %s' % (percorso, ', '.join(x[14:17] for x in lotto)))
    print('%d fogli pronti in %s' % (fatti, FOGLI))
    print('ordine nel foglio: alto-sinistra, alto-destra, basso-sinistra, basso-destra')


def scheda(numero, disciplina, momento, titolo, descrizione):
    if disciplina not in DISCIPLINE:
        print('disciplina sconosciuta. Ammesse: %s' % ', '.join(DISCIPLINE))
        return
    if momento not in MOMENTI:
        print('momento sconosciuto. Ammessi: %s' % ', '.join(MOMENTI))
        return
    nome = 'anime-archive-%03d.png' % int(numero)
    if not os.path.exists(os.path.join(TAVOLE, nome)):
        print('la tavola %s non esiste' % nome)
        return
    if nome.lower() in catalogate():
        print('%s ha gia una scheda: modificala a mano nel catalogo' % nome)
        return

    tags = 'manga, tavola-narrativa, carriera, giappone, %s, %s' % (disciplina, momento)
    riga = ('        , ["%s"] = new("%s", "%s", "%s", "%s")\n'
            % (nome, titolo, tags, DISCIPLINE[disciplina], descrizione))

    testo = io.open(CATALOGO, encoding='utf-8').read()
    # Si inserisce in coda alla biblioteca, prima della sua chiusura.
    inizio = testo.index('Library = new Dictionary')
    chiusura = testo.index('\n    };', inizio)
    testo = testo[:chiusura] + '\n' + riga.rstrip('\n') + testo[chiusura:]
    io.open(CATALOGO, 'w', encoding='utf-8', newline='').write(testo)
    print('scheda scritta per %s: %s / %s' % (nome, disciplina, momento))


if __name__ == '__main__':
    if len(sys.argv) < 2 or sys.argv[1] == 'stato':
        stato()
    elif sys.argv[1] == 'fogli':
        fogli(int(sys.argv[2]), int(sys.argv[3]) if len(sys.argv) > 3 else int(sys.argv[2]) + 23)
    elif sys.argv[1] == 'scheda':
        scheda(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6])
    else:
        print(__doc__)
