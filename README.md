# Drafty

Drafty è uno spazio essenziale per scrivere bozze di testo semplice, senza formattazione o sintassi Markdown.

## Funzionalità

- Editor a schermo intero senza bordi, con conteggio di parole e caratteri e orologio.
- Animazione del logo all'avvio.
- Stato discreto della bozza: indica se è pronta, salvata o contiene modifiche non salvate.
- Schemi colore selezionabili e modalità trasparente per scrivere sopra il desktop.
- Caratteri e dimensioni regolabili, ricerca nel testo e musica ambientale opzionale.
- Larghezza del testo regolabile con i preset Wide, Medium, Narrow, Narrow Left e Narrow Right.
- Obiettivo giornaliero di scrittura in caratteri inseriti, con progresso persistente in `%LOCALAPPDATA%\Drafty\writing-goal.json` e una notifica discreta al raggiungimento.
- Preferenze persistenti per schema colore, carattere, dimensione del testo e posizione della barra dati, salvate in `%LOCALAPPDATA%\Drafty\settings.json`.
- Salvataggio manuale e autosalvataggio dei documenti modificati.
- Salvataggio tramite file temporaneo, con sostituzione dell’originale solo dopo la scrittura.
- Avvisi per le modifiche non salvate prima di aprire un altro documento o uscire.
- Backup con data e ora, creato quando si apre un file. I backup sono in `%LOCALAPPDATA%\Drafty\Backups`.

## Requisiti

- Windows 10 o Windows 11.
- .NET 8 per Windows.

## Avvio

Scarica una release e avvia `ZenScribe.exe`. Per compilare il progetto dai sorgenti, apri `ZenScribe.sln` con Visual Studio oppure esegui:

```powershell
dotnet build ZenScribe.sln
```

## Comandi rapidi

| Tasti | Funzione |
| --- | --- |
| `F1` | Mostra l’aiuto |
| `Ctrl + S` | Salva il documento |
| `Ctrl + O` | Apre un documento |
| `Ctrl + F` | Cerca nel testo |
| `Ctrl + N` | Salva le modifiche e svuota l’editor |
| `Ctrl + M` | Cambia traccia o disattiva la musica |
| `Alt + C` | Scorre gli schemi colore e la trasparenza |
| `Alt + T` | Attiva o disattiva la trasparenza |
| `Alt + F` | Cambia carattere |
| `Alt + =` / `Alt + -` | Aumenta o riduce la dimensione del carattere |
| `Alt + W` | Scorre i preset di larghezza del testo |
| `Ctrl + Alt + B` | Sposta la barra dati in alto o in basso |
| `Invio` / `Tab` | Inserisce una nuova riga o un rientro |

## Licenza

Drafty è distribuito con licenza MIT. Consulta il file [LICENSE](./LICENSE) per il testo completo.

## Attribuzione delle musiche

Le tracce musicali incluse nel progetto provengono da [Free Music LoFi Tracks di Rusted Studio su itch.io](https://rustedstudio.itch.io/free-music-lofi-tracks).
