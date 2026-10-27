# V3UPSManager — Technical Details

## Projects and dependencies

The solution targets .NET 10 LTS. `V3UPSManager` and `V3UPSWizard` are Windows Forms executables targeting `net10.0-windows`; both reference the portable `net10.0` library `V3UPSManager.Core`. The Core has no UI framework dependency. `V3UPSManager.Core.Tests` exercises filesystem and workflow behavior without loading Windows Forms.

The manager's existing designer, resources, controls and localization files retain their visual layout. Its partial classes manage selection state, localized prompts, logging and button availability. They delegate game validation, patch discovery and file changes to the Core. The wizard provides a guided installation workflow over the same services.

## Game definitions

`V3UPSManager.Core/Games/GameBase.cs` defines the game contract and the `Game` and `FormatType` enums. Concrete definitions own their filenames, folder identifiers, patch suffixes, supported extensions, hashes, archive checks and edition-specific paths:

| Definition | Game | Patch format |
|---|---|---|
| `DRV3` | Danganronpa V3: Killing Harmony | UPS |
| `AITSF` | AI: The Somnium Files | XDelta |
| `TokyoPsychodemic` | Tokyo Psychodemic | UPS |

`GameCatalog` creates definitions and detects known folder names at complete path-segment boundaries, including Windows separators. Folder-name detection is a convenience; it is not installation validation. The wizard permits explicit game selection, so an installation does not have to keep its default directory name.

Adding a game starts with a `GameBase` subclass and catalog registration. Unique validation requirements belong in the Core validator; the front ends should not acquire their own game-specific filesystem rules.

## Installation validation

`InstallationValidator.Validate` returns an `InstallationValidationResult` or `null`. The result contains the edition, resolved data directory, executable-access capability and optional Switch title ID. `ValidationMessage` reports existing localization IDs, arguments and whether explicit confirmation is required. The manager resolves them through its localization tables; another caller must provide its own presentation. An unanswered confirmation is rejected.

The validator selects a Danganronpa edition from identifying files before checking its contents. A rejected hash or conflict warning cannot silently fall through to another edition. Legacy validation checks language, unpacked archives and required directories. Xbox validation disables executable patching. Switch validation identifies a single platform by its required `all` directory rather than filesystem enumeration order. AI: The Somnium Files resolves its validated data under `StreamingAssets/AssetBundles/StandaloneWindows64`.

Executable MD5 values identify known game builds; they are compatibility checks, not security signatures. Hash mismatches can require user confirmation because newer legitimate game builds may exist.

## Read-only patch discovery

`PatchDiscovery.Discover` produces a `PatchDiscoveryResult` containing paired `PatchTarget` values, unresolved targets and the number of candidate patches. A plan is complete only when every candidate has one unique target.

Patch files use `_patch.ups` or `_patch.xdelta`. Their paths mirror the installation tree. Targets may already include their extension, omit an extension resolved through the game's allowed extensions, or genuinely have no extension when supported. Multiple matching target extensions are rejected instead of guessed. A target may match an existing `_bak` when its live file is missing; discovery never restores or modifies that file.

Executable-access restrictions apply to discovery as well as installation. Duplicate targets, missing files, invalid package roots and linked paths cannot produce a complete plan. The manager clears stale plans when the installation changes or a new package is rejected. Cancelling a folder dialog preserves the previous selection.

## Patch transaction and recovery

`PatchInstaller` accepts a `PatchInstallRequest` containing immutable target pairs, the selected roots, patch format, executable-access policy and absolute tool path. `IPatchTool` isolates external execution for tests; `ProcessPatchTool` runs the bundled tool without a command shell, uses `ArgumentList`, and drains stdout and stderr concurrently.

The transaction proceeds as follows:

1. Validate every target, patch, backup and path boundary before writing game data.
2. Save separate recovery copies of existing live files.
3. Create missing permanent clean `_bak` files through staged copies and publish them without overwriting an existing backup.
4. Apply every patch to its clean backup, producing a unique temporary output beside the target.
5. Require a successful tool exit and a produced output for every patch before publishing any live output.
6. Publish outputs with file moves, retaining recovery copies until the operation succeeds.

On failure, already published files are restored from their pre-attempt live copies. This preserves earlier mods during a failed reinstall; permanent clean backups serve a different purpose and are not used as transaction rollback copies. A target that was absent before the attempt is removed again during rollback. If automatic recovery fails, its `.v3ups_rollback.*` file remains for manual recovery. Successful operations remove temporary output and rollback files.

`PatchPathSafety` rejects paths outside the selected roots and symbolic links/junctions in their ancestry. These checks are defensive boundaries, not an operating-system sandbox against another process concurrently replacing filesystem entries.

`FileOperationLock` provides the shared named mutex used by both front ends. The manager holds it across snapshots, installation or uninstall. Mutex acquisition and release must occur on the same thread. Core file services remain composable; callers running them directly must supply equivalent coordination.

## Snapshots and uninstall

`SnapshotService` stages a complete snapshot before replacing a previous snapshot directory. Clean snapshots prefer the original `_bak` content while keeping the live target's relative filename; modded snapshots use live files. Optional title-ID/`romfs` nesting is preserved. A snapshot destination must not overlap the game installation in either direction. A failed publication restores or retains the previous snapshot.

`BackupRestorer` enumerates matching `_bak` files once, including wildcard extension rules, without following linked directories. Each restore stages the backup content, publishes the restored file, then removes the backup. Failed restores retain their recovery copy and do not prevent attempts to restore other files. `RestoreResult` distinguishes complete success, partial failure and no matching backups. Game definitions can disable backup-based uninstall for unsupported editions, such as Danganronpa V3 Unity.

The application does not merge mods or resolve conflicts. Backups restore known content; they do not reverse arbitrary patch transformations.

## Wizard package preparation

`Core/Wizard/PackageSession` owns temporary extraction directories and never deletes a user-selected source directory. It accepts an extracted patch folder or `.DAIX.7z` package. Metadata consists of an optional single `*.game` file and `README.txt`/`LEGGIMI.txt`; patches can be loose or supplied in one nested `PATCH.7z`. Unsupported game names, ambiguous metadata and packages without patches are rejected. Preparation requires a successfully opened package.

`SevenZipExtractor` uses the adjacent `7za.exe`. It inspects archive entries before extraction, rejecting absolute paths, parent traversal, links and Windows device names, and validates extracted content again. `ExternalTool` drains both output streams and closes stdin so a hidden password prompt cannot wait for interactive input. Cancellation terminates an extraction process tree and waits for it to finish before temporary cleanup.

The wizard always permits manual game-directory selection. Optional Steam/Xbox suggestions require a separate, initially unchecked permission checkbox and an explicit Search action. `GameInstallationDiscovery` inspects known launcher locations within one selected local drive/library root, plus bounded Steam library/manifests metadata within that root. It never recursively scans an entire disk, follows links, probes libraries on other drives, modifies launcher metadata, or uploads results. Suggestions are transient and must pass the normal installation validation before patching. Search runs off the UI thread and can be cancelled; unchecking the option discards suggestions. On Windows, network roots and mapped network drives are rejected before launcher contents are inspected. Package preparation remains cancellable. Installation uses the shared validator, discovery and patch transaction; it must complete or roll back before the window can close. A completion page represents the actual transaction result.

## Localization and presentation

Manager localization remains external under `V3UPSManager/Localization/<language>`. The project copies all language files to build and publish output. Localization fallback and UI state stay in the manager. Core operation messages use structured arguments and existing localization IDs so file services never show a message box or depend on a control.

The manager still accepts `--game`, `--install-dir` and `--patch-folder`. Command-line selection uses the same validation and patch discovery as interactive selection. Missing argument values must not consume the following option.

## Build, tests and distribution

See the appended development section in `README.md` for exact commands. Linux supports Core tests and cross-compilation with `EnableWindowsTargeting=true`; it cannot run the WinForms interfaces natively. CI builds and runs the Core suite on Windows and Linux. Tests cover successful operations and failures, including preservation of previous live mods, output publication failures, rejected paths, snapshot replacement, invalid packages, process output and cancellation.

`scripts/publish.ps1` publishes both Windows x64 front ends into a shared self-contained directory. External `ups.exe`, `xdelta3-3.1.0-x86_64.exe` and `7za.exe` are distribution/runtime requirements for their respective workflows, not managed build dependencies. They must be distributed with their applicable licenses; publishing the .NET projects does not download them. Real-game patch compatibility and interactive GUI behavior require Windows testing with representative game/mod files.
