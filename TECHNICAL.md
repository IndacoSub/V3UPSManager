# V3UPSManager — Technical Details

This document explains what **V3UPSManager** does internally, how the installation process works, and why the project is structured the way it is.

The goal of this document is not to replace the main `README.md`. The README is the short, user-facing introduction; this file is the technical explanation for contributors and anyone who wants to understand the project before modifying it.

---

## 1. What V3UPSManager is

V3UPSManager is a Windows WinForms application that automates the installation and removal of file patches for a small set of games.

The original project was created around **Danganronpa V3: Killing Harmony** and later gained support for **AI: The Somnium Files** and **Tokyo Psychodemic**.

A mod is not treated as a generic archive to be copied into a game directory. Instead, the manager expects patch files (currently UPS or XDelta, depending on the game) and uses those patches to produce modified versions of files that already exist in the game installation.

That distinction is important:

- The game installation provides the clean/base file.
- The mod provides a patch describing how to transform that file.
- V3UPSManager finds the correct base file, creates a backup of it, applies the patch, and leaves the modified file in place.
- Uninstall restores the backup rather than trying to reverse the patch mathematically.

This is why the backup system is a fundamental part of the application rather than an optional convenience.

---

## 2. The basic model

At a high level, the application performs this sequence:

```text
Game installation
       │
       ▼
 Identify game
       │
       ▼
 Identify edition/platform
       │
       ▼
 Validate installation
       │
       ▼
 Scan mod/patch folder
       │
       ▼
 Match each patch to a game file
       │
       ▼
 Create/keep original backup
       │
       ▼
 Apply patch
       │
       ▼
 Verify output / restore on failure
```

Uninstall follows the opposite strategy:

```text
Modified installation
       │
       ▼
 Find V3UPSManager backups
       │
       ▼
 Restore original file
       │
       ▼
 Remove backup after successful restore
```

The application deliberately does **not** attempt to reconstruct an original file from a patch in reverse. Restoring a known-good backup is safer and much simpler.

---

## 3. Why game-specific information belongs in `Games/*.cs`

One of the most important design rules in the project is:

> A rule that exists because of one particular game should live with that game's definition whenever practical.

For example, Danganronpa V3 has rules such as:

```csharp
public override IReadOnlyList<string> UninstallExtensions { get; } =
    new[] { ".spc", ".awb" };
```

Those extensions are not a general V3UPSManager concept. They describe which files **Danganronpa V3** can have restored by its backup-based uninstaller.

The same applies to things such as:

- game folder identifiers;
- executable names;
- executable hashes;
- Unity data directory names;
- patchable extensions;
- validation extensions;
- supported Unity title IDs;
- archive names used to distinguish editions;
- legacy/Xbox data paths;
- edition-specific filesystem layout;
- patch format and its executable tool;
- exceptions to the normal backup/uninstall behaviour.

Keeping these rules together has two advantages:

1. **The generic workflow remains generic.**
   `Install`, patch discovery, and uninstallation do not need another giant `switch` every time a new game is added.

2. **A contributor can inspect one game in one place.**
   `Games/DRV3.cs` answers most questions about Danganronpa V3 without having to chase the same game's constants through unrelated files.

This is intentionally not an abstraction exercise. The project does **not** create a separate class or service for every single property merely to move a few lines elsewhere. Shared behaviour remains shared; game-owned rules stay with the game.

---

## 4. `GameBase` and the supported games

`Games/GameBase.cs` contains the common contract shared by supported games.

The concrete game classes are:

| Class | Game | Patch format | Main environment |
|---|---|---|---|
| `DRV3` | Danganronpa V3: Killing Harmony | UPS | Legacy / Unity / Xbox detection |
| `AITSF` | AI: The Somnium Files | XDelta | Unity |
| `TokyoPsychodemic` | Tokyo Psychodemic | UPS | Unity |

`GameBase` provides defaults for properties that are common or optional. A concrete game overrides only what differs from those defaults.

For example, a game can define:

```csharp
public override string UnityDataFolder => "AI_TheSomniumFiles_Data";
public override string UNITY_EXE_NAME => "AI_TheSomniumFiles.exe";
public override IReadOnlyList<string> UnityPatchExtensions { get; } =
    new[] { "", ".assets", ".exe" };
```

while another game can use a completely different Unity data folder, patchable file set, or patch root.

### Why the base class contains properties instead of giant global dictionaries

A global dictionary such as:

```csharp
Dictionary<Game, string[]> ...
```

works at first, but it gradually turns the application into a collection of unrelated lookup tables.

As more games are added, it becomes easy to forget to update one of those tables. It also makes ownership unclear: is the list global application configuration, or is it part of the game's definition?

The current model makes ownership explicit.

---

## 5. Game detection

Game detection is based on known folder identifiers defined by the game classes.

`MainWindow.GetGameByFolder()` iterates through the supported game definitions and compares normalized path segments.

Path normalization is important because users may provide paths with:

- different slash styles;
- trailing separators;
- different casing on case-insensitive filesystems.

The matching logic also requires a path boundary, preventing accidental substring matches.

Once a game ID is known, `SpawnGameByID()` creates the corresponding `GameBase` implementation.

The rest of the program can then ask `CurrentGame` for game-specific rules instead of repeatedly branching on `Game` throughout the workflow.

---

## 6. Installation detection is more than finding an EXE

Finding an executable is not sufficient to declare an installation valid.

The application attempts to determine which edition/platform is being selected and then validates the expected filesystem layout.

The main paths are:

### Legacy configuration

Used for games that have a traditional PC layout and support the Legacy validation flow.

The game definition decides where the actual game data lives through `GetLegacyDataFolder()`.

Danganronpa V3, for example, maps the installation root to:

```text
<data root>/data/win
```

for its Legacy layout.

### Unity configuration

Unity games are validated through their expected Unity data folder and `StreamingAssets` structure.

The common layout starts from the game-specific `UnityDataFolder` value. Games can override `GetUnityDataFolderPath()` when their layout differs.

Danganronpa V3 additionally detects its platform folder and title ID structure. Tokyo Psychodemic overrides the Unity data path because its patchable content is stored below:

```text
TOKYO_PSYCHODEMIC_Data/StreamingAssets/AssetBundles
```

### Xbox / Microsoft Store configuration

The project contains Xbox/Anniversary detection specifically for Danganronpa V3.

Because Microsoft Store/Xbox installations do not expose the executable for patching in the same way, executable patching is disabled for that configuration.

The application also checks known archive names to distinguish incompatible layouts.

---

## 7. Validation philosophy

The validation code is intentionally conservative.

Depending on the game and edition, V3UPSManager may check:

- expected directories;
- expected executable names;
- executable MD5 hashes;
- Unity folder names and case;
- expected archive files;
- known title IDs;
- expected language configuration;
- presence of files needed to identify a valid installation;
- potentially conflicting external modifications such as ReShade, BepInEx or game-specific fixes.

A hash mismatch does not always mean the user must be blocked. In the existing workflow, the application can warn and ask whether the user wants to continue when the relevant validator allows that behaviour.

The important distinction is between:

- **"This cannot be a valid installation"**, where the operation stops; and
- **"This installation differs from the expected clean version"**, where the user may be allowed to proceed deliberately.

---

## 8. Patch discovery

The patch folder is scanned before installation begins.

The objective is not simply "find every `.ups` file". The manager needs to associate each patch with the actual file it is intended to modify.

The discovery process therefore builds two aligned collections:

```text
_filesToPatch
_patchesToApply
```

Entry `N` in `_patchesToApply` corresponds to entry `N` in `_filesToPatch`.

For example:

```text
_filesToPatch[3]    -> game/path/file.assets
_patchesToApply[3] -> mod/path/file_patch.ups
```

Before installation, the program checks that both lists have the same length. This is a deliberate safety check: applying two independently ordered lists without verifying their alignment could apply the right patch to the wrong file.

### Patch filename convention

The current workflow uses the game's `PatchSpecificString` to remove the patch suffix and reconstruct the approximate base-file path.

Examples conceptually look like:

```text
original file:  script.assets
patch file:     script.assets_patch.ups
```

or, for games using XDelta:

```text
original file:  somefile
patch file:     somefile_patch.xdelta
```

The extension rules then determine which target file extensions are valid for the selected game and edition.

### Files without an extension

An empty extension (`""`) is meaningful.

Some Unity titles contain patchable files that do not have a normal file extension. The discovery code therefore treats an empty extension as a valid target rather than assuming every patch target must end in `.assets`, `.dll`, `.exe`, etc.

### Executable patching

Executable files are special because some environments do not provide safe/read-write access to the executable.

When `_canAccessExe` is false, `.exe` targets are filtered out from the patch target set. This is particularly relevant to the Xbox/Microsoft Store configuration.

---

## 9. Why extensions are part of the game definition

There are three separate concepts that should not be confused:

### Patchable files

These are the files the installer is allowed to look for as patch targets.

They are represented by:

- `LegacyPatchExtensions`
- `UnityPatchExtensions`

### Validation files

Some games need a narrower set for installation validation.

For that reason `UnityValidationExtensions` may differ from `UnityPatchExtensions`.

For Danganronpa V3, for example, `.ab`, `.assets`, and `.pb` are used for a particular Unity validation check even though the complete patchable list is broader.

### Uninstall targets

Uninstallation is yet another concern. `UninstallExtensions` defines which backup files the game-specific uninstaller should consider.

For example:

```csharp
new[] { ".spc", ".awb" }
```

means that backups of those file types are eligible for restoration.

The special value:

```csharp
"*"
```

means "all backup files" for the historical uninstaller pattern, implemented as a search for `*_bak`.

This separation is deliberate. A file may be patchable without necessarily belonging to the set that the generic backup-based uninstaller should restore.

---

## 10. Installation and backups

The installation routine is transactional as far as this application's file operations reasonably allow.

Before running the external patch tool, the manager makes sure a clean source is available.

The normal pattern is:

```text
live file
   │
   ├── first installation ──► create file_bak
   │
   └── later installations ─► keep existing file_bak
```

The backup is intentionally not overwritten by a later installation. This preserves the known original source instead of turning a backup into a backup of an already-modded file.

If a live target has disappeared but its `_bak` still exists, the installer can use the backup as the source for recovery in the relevant code paths.

That behaviour is particularly important after a partially interrupted previous run.

---

## 11. Applying the external patch tool

V3UPSManager does not implement the UPS or XDelta algorithms itself.

It launches the corresponding command-line tool supplied with the release build.

The tool is selected from the active game definition:

```text
GameBase.PatchFormat
GameBase.PatchFormatExtension
GameBase.PatchFormatInstaller
GameBase.PatchSpecificString
```

This allows Danganronpa V3 and Tokyo Psychodemic to use UPS while AI: The Somnium Files uses XDelta without putting a game-specific `if` block into the core patching workflow.

The application executes the tool as a child process, captures its output/error streams, and treats a failed process as an installation failure.

---

## 12. Temporary patch output and rollback

A patch operation should not directly destroy the only usable copy of the target file before the new output is known to be valid.

For this reason the installer works with temporary output paths during the patch operation.

The high-level sequence is:

```text
backup original
      │
      ▼
create temporary patched output
      │
      ▼
run UPS/XDelta
      │
      ├── failure ──► delete temporary output + restore modified files
      │
      └── success ─► publish patched output
```

If one file in a multi-file installation fails, previously modified files can be restored from the backups created during the same operation.

This is not a database-style transaction and cannot make the filesystem literally atomic across every possible Windows failure, but it significantly reduces the chance that a failed patch leaves the game in an intentionally incomplete state.

Diagnostic information is also written when important preconditions fail, making malformed patch sets easier to inspect.

---

## 13. Uninstallation

The uninstaller intentionally works from backups rather than attempting to detect and undo patch contents.

For a backup such as:

```text
some/path/file.assets_bak
```

uninstallation restores:

```text
some/path/file.assets
```

and removes the `_bak` file only after the restore succeeds.

This ordering matters. If the copy fails because a file is locked or inaccessible, the backup remains available for manual recovery.

The extensions used by this process are defined by the active game. That is why entries such as:

```csharp
[Game.AITheSomniumFiles] = new[] { ".assets", ".exe", "*" }
```

belong conceptually to `Games/AITSF.cs`, not to a global uninstaller table.

The current game definitions can also explicitly disable backup-based uninstallation for a particular environment. Danganronpa V3's Unity configuration uses such an exception because its content layout does not follow the same backup restoration assumptions as its other editions.

---

## 14. Conflict handling

V3UPSManager does **not** resolve mod conflicts.

Suppose two mods both patch:

```text
assets/example.assets
```

Installing one after the other can result in the second modification becoming the effective file content. The manager does not attempt to merge the patches or determine whether they are mathematically compatible.

This is a deliberate limitation rather than an accidental omission.

UPS/XDelta patches describe transformations from particular input files. Safely merging two arbitrary transformations requires information about the exact base file, patch ordering, and the semantics of the modified content. A generic "conflict resolver" would risk silently generating broken data.

The application therefore keeps the workflow simple and explicit: it applies the patches it was asked to apply and relies on the user/mod author to avoid incompatible combinations.

---

## 15. Localization

Localization is deliberately external to the game-definition model.

At runtime, V3UPSManager scans the `Localization` directory and loads language folders containing the expected text files.

This means a translation does not require recompiling the executable.

The important separation is:

```text
Game definitions      -> technical game rules
Localization          -> UI text and messages
WinForms Designer     -> UI layout
Installation logic    -> patch/backup workflow
```

Numeric message IDs may still appear inside the code because the workflow needs to select a localized message, but the text itself remains in the localization data.

The folder is copied to build/publish output by the project file so localized resources are available at runtime.

---

## 16. GUI and workflow code

The project uses WinForms partial classes. The GUI layout is generated by `Main.Designer.cs` and the visual resources live in `Main.resx`.

The refactor intentionally leaves those files alone.

The non-designer partial files contain the application logic around the existing controls. This allows the project to improve the internal organization without forcing a visual redesign or changing the user workflow.

The same principle applies to the UPS wizard project: it is a separate utility and is not required to be folded into the main manager's architecture.

---

## 17. Main source files

The current structure remains intentionally compact.

| File | Responsibility |
|---|---|
| `Main.cs` | Main window state, command-line startup, UI events and operation coordination |
| `Games/GameBase.cs` | Common game contract and game factory/discovery helpers |
| `Games/DRV3.cs` | Danganronpa V3-specific rules |
| `Games/AITSF.cs` | AI: The Somnium Files-specific rules |
| `Games/TokyoPsychodemic.cs` | Tokyo Psychodemic-specific rules |
| `LoadFS.cs` | Installation and patch-folder filesystem discovery |
| `Legacy.cs` | Legacy installation validation |
| `Unity.cs` | Unity installation validation and platform detection |
| `Xbox.cs` | Xbox/Microsoft Store validation |
| `Common.cs` | Shared installation checks |
| `Install.cs` | Backup + patch application + rollback |
| `Uninstall.cs` | Backup restoration |
| `BackupChanges.cs` | Optional snapshot/backup management |
| `Localization.cs` | Language discovery and localized resources |
| `Utils.cs` | Filesystem, hash, registry and low-level utility helpers |
| `DisplayInfo.cs` / `Info.cs` | Display/localization-related UI support |
| `Program.cs` | Application entry point |

The goal is not to create a large framework. The file boundaries correspond to recognizable responsibilities that are useful when maintaining this specific application.

---

## 18. Why the project is intentionally not more abstract

There is a point where refactoring stops improving a project and starts obscuring it.

For this application, introducing a separate service, interface, repository, provider, strategy, DTO, or configuration file for every small operation would make the code harder to follow without providing a real benefit.

The preferred rule is:

> Abstract behaviour when it is genuinely shared; keep small game-specific facts next to the game that owns them.

For example:

- `UninstallExtensions` belongs to the game.
- `UnityDataFolder` belongs to the game.
- The generic act of enumerating `*_bak` files belongs to the uninstaller.
- The generic act of invoking the configured patch executable belongs to installation.
- The actual visual layout belongs to the WinForms designer.

This division keeps responsibilities clear without creating an unnecessary architecture layer.

---

## 19. Adding a new supported game

A new game should normally start with a new file under `Games/`, deriving from `GameBase`.

That file should contain the game's own facts, for example:

```csharp
public sealed class NewGame : GameBase
{
    public override Game GameID => Game.NewGame;

    public override string UNITY_EXE_NAME => "NewGame.exe";

    public override string UnityDataFolder => "NewGame_Data";

    public override IReadOnlyList<string> UnityPatchExtensions { get; } =
        new[] { ".assets", ".bundle" };

    public override IReadOnlyList<string> UninstallExtensions { get; } =
        new[] { ".assets", ".bundle" };
}
```

Then the application's game registration is updated in the existing central factory so the UI can instantiate it.

After that, only genuinely unique workflow rules should be added to the appropriate validation code.

The important objective is that adding a game does **not** require scattering the same list of extensions across multiple unrelated global dictionaries.

---

## 20. External dependencies and runtime tools

The source project is a .NET 6 Windows Forms application.

The source code itself does not need to compile the external UPS/XDelta implementations into the application binary.

The release distribution provides the required command-line executables alongside the manager, and the manager verifies that the expected patch tool exists before starting an installation.

The licensing information and attribution for those tools remain documented in `README.md`.

---

## 21. Safety-oriented invariants

Several rules in the current code are intentionally stronger than the minimum required for a basic patcher.

### Do not overwrite the original backup

Once `file_bak` represents the clean source, later installations should not silently replace it with an already modified version.

### Keep patch and target lists aligned

A patch must never be applied to an unrelated target because two lists happened to get out of sync.

### Reject paths outside the verified installation

Backup creation checks that a source file resolves under the verified installation root before copying it into the application-owned backup tree.

### Restore before deleting a backup

If restoration fails, the backup remains available.

### Do not leave a cancelled folder selection half-applied

Folder selection routines preserve the previous verified state when the user cancels, rather than leaving half-updated game/platform state behind.

### Do not assume executable access

Some supported environments cannot safely expose their executable. The installer therefore filters executable patch targets when the selected installation does not permit them.

These are not separate products or frameworks; they are small invariants that protect the existing workflow from common failure modes.

---

## 22. What V3UPSManager deliberately does not do

The application is intentionally focused.

It does not:

- download mods from the Internet;
- resolve arbitrary mod conflicts;
- merge unrelated UPS/XDelta patches;
- reconstruct an original file when no usable backup exists;
- support every distribution/edition of every supported game;
- understand proprietary archive formats it cannot safely manipulate.

For example, the Xbox/Microsoft Store edition of Danganronpa V3 is detected in the code, but it does not expose the same patching capabilities as the desktop/Unity layouts.

That is preferable to pretending an unsupported filesystem format is safe to modify.

---

## 23. Design summary

The central idea of V3UPSManager is simple:

```text
                ┌──────────────────────┐
                │      GameBase        │
                │ common game contract │
                └──────────┬───────────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
      ┌───▼───┐        ┌───▼───┐        ┌───▼────────────┐
      │ DRV3  │        │ AITSF │        │ TokyoPsychodemic│
      └───────┘        └───────┘        └─────────────────┘
          │                │                │
          └────── game-owned rules ────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │  Common workflow     │
                │ detect / scan /      │
                │ backup / patch /     │
                │ rollback / uninstall │
                └──────────────────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │ Existing WinForms UI │
                └──────────────────────┘
```

The design deliberately keeps the application close to the way it actually behaves:

- **games own their differences;**
- **shared code owns shared operations;**
- **the GUI stays the GUI;**
- **backups are the basis of safe uninstall;**
- **patch tools are external and selected by game configuration;**
- **unsupported or ambiguous situations are rejected or explicitly confirmed rather than guessed.**

That balance is the main reason the project can grow without turning every new game or every new file-extension rule into another global switch statement.

