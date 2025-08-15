# H1 – Løsningsforslag i Grundlæggende Programmering

## Indholdsfortegnelse
- [Introduktion](#introduktion)
- [Opgavebeskrivelse](#opgavebeskrivelse)
- [Teknisk Gennemgang](#teknisk-gennemgang)
  - [Udviklingsmiljø](#udviklingsmiljø)
  - [Teknologivalg](#teknologivalg)
  - [Versionsstyring](#versionsstyring)
  - [Arkitektur og Design](#arkitektur-og-design)
- [Datamodeller](#datamodeller)
  - [Page](#page)
  - [LeftRightMenuPage](#leftrightmenupage)
  - [Task](#task)
  - [PageArgument](#pageargument)
- [Application Livscyklus](#application-livscyklus)
  - [Start og Initialisering](#start-og-initialisering)
  - [Run-loop i PageManager](#run-loop-i-pagemanager)
  - [Rendering og Buffer](#rendering-og-buffer)
- [Raw Input](#raw-input)
  - [Design](#design)
- [Task Manager](#task-manager)
  - [Formål](#formål)
  - [Data og Struktur](#data-og-struktur)
  - [Kernefunktioner](#kernefunktioner)
  - [Persistens](#persistens)
  - [UI-integration](#ui-integration)
- [Ping Pong](#ping-pong)
  - [Formål og Placering](#formål-og-placering)
  - [Styring og Inputkilder](#styring-og-inputkilder)
  - [Livscyklus](#livscyklus)
  - [Game-loop og Fysik](#game-loop-og-fysik)
  - [Rendering](#rendering)
  - [Robusthed](#robusthed)

---

## Introduktion

## Opgavebeskrivelse
Til vores grundlæggende programmering projekt har vi fået til opgave at programmere en console-applikation i C#.  
Krav:
- Tilføj opgaver (maks. 5 ad gangen).
- Vis alle opgaver (færdige og ikke færdige).
- Markér opgave som færdig.
- Afslut programmet pænt.
- Gem status i array/liste.
- Strukturér kode med metoder, loops, betingelser og switch-case.
- Console I/O med tydelige beskeder.
- Versionsstyring med Git og upload til public GitHub repository med meningsfulde commits.

Ekstra funktioner:
- Slet opgaver.
- Gem/indlæs fra tekstfil.
- Sortér færdige opgaver nederst.
- Brug farver til statusvisning.

---

## Teknisk Gennemgang

### Udviklingsmiljø
- **IDE**: Visual Studio Professional Edition v17.14.11

### Teknologivalg
- **Sprog/Framework**: C# .NET Core 9  
  Ikke en LTS-udgivelse, men valgt pga. adgang til nye funktioner.

### Versionsstyring
- **Værktøj**: Git
- **Hosting**: GitHub (valgt for enkel deling og tilgængelighed).

### Arkitektur og Design
- Konsolbaseret applikation med **side-baseret navigationssystem**.
- Abstrakt baseklasse `Page` til alle sider.
- Fordele:
  - Titler og beskrivelser pr. side.
  - Argumenter med visning.
  - Inputsystem til metodekald.
  - Høj konfigurerbarhed.

---

## Datamodeller

### Page
- Abstrakt baseklasse for alle sider.
- Håndterer:
  - Aktivering/Deaktivering (`Activate()`, `DeActivate()`).
  - Rendering (AutoRender, ClearAtRender).
  - Event-baseret input (`OnKeyPressed`).
  - Opdateringslogik (`HasUpdate()`).
  - Layoutinformation (Window Width/Height).
- Konkrete sider implementerer `Render(Rendere)`.

### LeftRightMenuPage
- Abstrakt sideskabelon med venstre menu + højre ASCII-kunst.
- Funktioner:
  - Titel, beskrivelse, højrepanel (Title, Description, ArtFile).
  - Menuargumenter (`PageArgument`-liste).
  - Indbygget tastestyring (tal, piletaster, Enter).
  - Rendering opdelt i venstre/højre panel.
  - Hjælpefunktion til at mappe tastkoder til menuvalg.

### Task
- Uforanderlig datamodel for en opgave.
- **Egenskaber**:
  - `Name`
  - `IsFinished`
- **Konstruktor**:
  - Opretter opgave med initial status ([JsonConstructor]).
- **Metoder**:
  - `Finish(bool finished)`
  - `ToString()`

### PageArgument
- Uforanderlig container for et menupunkt.
- **Egenskaber**:
  - `Name`
  - `Action`
- Bruges i `LeftRightMenuPage` til at udføre menuhandlinger.

---

## Application Livscyklus

### Start og Initialisering
- Singleton i `H1BasicProgrammingFinalProject`.
- Loader ASCII-art.
- Loader eller opretter opgavefil via `TaskManager`.
- Initialiserer `PageManager` og sætter `MainMenuPage` aktiv.

### Run-loop i PageManager
- Tjekker skærmkrav (størrelse/fyldskærm).
- Renderer aktiv side via `RenderPage()`.

### Rendering og Buffer
- Tjek for autoopdatering og rydning af skærm.
- Kalder `Render(Rendere)`.
- `Rendere` skriver til buffer og viser i loop.

---

## Raw Input

### Design
- Direkte buffertegning → ingen standard .NET key events.
- Tastaturdata læses via Win32 `ReadConsoleInput` på `STD_INPUT_HANDLE`.
- **Funktioner**:
  - `Poll()` tømmer input-buffer for `KEY_EVENT`.
  - `_held` (`HashSet<ushort>`) tracker taster.
  - Første `bKeyDown` = Click, autorepeat ignoreres.
  - `bKeyDown=false` = Up.
  - Events gemmes i `_events` (`ConcurrentQueue<KEY_EVENT>`).
  - `IsHeld(vk)` returnerer taststatus.

---

## Task Manager

### Formål
Central styring af opgaver (max 5 aktive).

### Data og Struktur
- Singleton (`TaskManager.Instance`).
- Liste af `Task`.
- Konstanter: `MaxTasks`, `FilePath`.

### Kernefunktioner
- `AddTask(Task)`
- `RemoveTask(...)`
- `RetrieveTask(...)`
- `Exists(string name)`
- `GetList()`
- `GetEnumerator()`

### Persistens
- `SaveTasks()` → JSON til fil.
- `LoadTasks()` → JSON fra fil.

### UI-integration
- `TaskMenuPage` (arver `LeftRightMenuPage`):
  - Tilføj, fjern, markér færdig, vis alle, tilbage.

---

## Ping Pong

### Formål og Placering
- Ekstra menupunkt i hovedmenu.
- Simpelt 2-spiller spil i konsollen.

### Styring og Inputkilder
- Spiller 1: `W`/`S`.
- Spiller 2: Pil op/ned.
- Arduino på COM5 (9600 baud) som alternativ input.

### Livscyklus
- Ved aktivering åbnes serielport og buffer ryddes.
- `StartGame()`:
  - Læser skærmstørrelse.
  - Tegner ramme, paddles, score.
  - Starter `RawInput.Poll`.

### Game-loop og Fysik
- Fast timestep (1/240s).
- Kollisionsdetektion med reflekser.
- Justerer boldens retning ved paddle-træf.
- Reset ved scoring.

### Rendering
- Tegning direkte til Console.
- `#` = ramme, `|` = paddle, `O` = bold.
- Scorelinje centreres øverst.

### Robusthed
- Serielkommunikation i try/catch.
- Fallback til tastaturstyring.
- Første-down events → undgår autorepeat.
- Minimal tegning for at undgå flicker.
