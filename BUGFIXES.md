# Bug fixes

This list covers the corrections introduced by the .NET 10 Core extraction and wizard rewrite relative to the earlier implementation. Framework upgrades, project separation and visual changes are not counted as bug fixes.

## Installation and recovery

- **Preserve an existing mod after a failed reinstall.** Rollback restores the live content from before the attempt instead of copying the clean original over previously modified files.
- **Avoid publishing a partially prepared installation.** All patch outputs must be produced successfully before any live game file is replaced. A failed publication triggers rollback of already published files.
- **Never retain a partially copied original as a valid backup.** New `_bak` files are staged and published only after copying succeeds; existing clean backups are never overwritten.
- **Keep recovery data when rollback fails.** Recovery copies remain available for manual restoration, and restoration errors are reported rather than hidden behind a generic success state.
- **Use original content for clean snapshots.** When both the live file and its `_bak` exist, clean snapshots use the backup rather than the already modded live file. When only a backup exists, the snapshot keeps the live target's filename instead of the `_bak` suffix.
- **Prevent snapshots from replacing game directories.** Snapshot destinations overlapping the installation in either direction are rejected. A failed snapshot publication preserves the previous complete snapshot.
- **Report partial uninstall failures correctly.** Uninstall no longer returns unconditional success. Matching backups are enumerated once, enumeration failures are caught, and a failed restore retains its backup.

## Game validation and patch selection

- **Do not try another edition after a rejected warning.** Declining a Legacy executable hash/conflict confirmation cannot fall through to Unity or Xbox validation.
- **Resolve the correct AI: The Somnium Files data path.** Validation uses `StreamingAssets/AssetBundles/StandaloneWindows64` instead of selecting an unrelated directory by enumeration order.
- **Choose a Switch platform from its content.** Detection requires one platform with the expected `all` directory instead of taking the first non-excluded directory. Ambiguous platforms are rejected.
- **Reject ambiguous patch targets.** A patch whose extensionless name matches multiple supported files no longer patches whichever extension happens to be checked first.
- **Apply executable-access restrictions consistently.** An `.exe` target cannot be accepted through the extensionless-file rule when executable patching is disabled for the selected edition.
- **Invalidate stale or incomplete patch plans.** Changing the installation clears the previous patch plan. Failed game/package validation disables installation and discards rejected plans, so later UI actions cannot re-enable a partial plan.
- **Allow reinstall after successful uninstall.** The Install button is re-enabled when a complete valid patch plan remains available.
- **Handle missing command-line values correctly.** An option without a value no longer consumes the following option as its value.
- **Recover cleanly from operation-lock initialization errors.** Mutex creation is covered by error handling and always resets the in-process operation state.

## Wizard workflow

- **Stop after failed extraction.** Nonzero extractor exit codes, missing nested patches and missing outputs cannot advance to a ready or successful state.
- **Show completion only after installation succeeds.** Preparing files or starting another program is no longer presented as a completed installation; the wizard runs and checks the actual patch transaction.
- **Handle folder packages through the full workflow.** Extracted folders receive metadata checks, readme review and patch validation instead of skipping those steps or showing an unrelated extraction page.
- **Reject invalid or ambiguous package metadata.** Unsupported game names, multiple `.game` files, multiple nested `PATCH.7z` files and packages without patches are reported instead of selecting arbitrary metadata or continuing.
- **Prevent failed preparation from becoming ready.** A failed package open or cancelled extraction cannot leave cached state that allows subsequent preparation to succeed. Loose patches are rechecked before their first preparation.
- **Keep temporary patches for the entire operation.** The session owns its extraction directories, cleans them when the wizard closes, and never deletes the user's selected source directory. Active installation must finish or roll back before closing.
- **Make extraction failures actionable.** Error output is included in the failure dialog; cancellation stops the extractor process tree and waits for it before cleanup.

## Defensive fixes

These protections address unsafe failure cases and malformed inputs; they are not claims that a particular user's installation was previously affected.

- Validate complete target/backup plans before filesystem mutation, rejecting duplicate targets, target/backup overlap, paths outside the selected roots and linked paths.
- Validate archive entry paths before extraction, rejecting parent traversal, absolute paths, Windows device names and archive links; inspect extracted files again before use.
- Resolve bundled tools relative to the application directory and pass arguments without a command shell, preserving paths containing spaces.
- Drain stdout and stderr concurrently and close stdin, preventing pipe-buffer deadlocks and hidden tools waiting for interactive input.
- Coordinate direct wizard installations and manager file operations through the same named mutex, acquired and released on one thread.
- Keep optional local game discovery explicitly opt-in, limited to the chosen drive/library root and known launcher locations; never scan other drives automatically or recursively walk a complete disk.

Regression tests cover these behaviors using temporary files, injected patch-tool failures and real subprocess fixtures where supported. Interactive Windows UI behavior and compatibility with actual game/mod releases still require Windows testing.

## CI test portability

- **Normalize filesystem paths in test fixtures and expectations.** The discovery regression test compared mixed-separator paths against normalized paths, causing a false failure on Windows despite successful discovery. Assertions now use native absolute paths on both platforms.
- **Expose failed-test messages in Actions annotations.** Failed TRX results are summarized alongside the workflow, while the original test failure status and full result artifacts are preserved.
