# V3UPSManager

A tool that allows you to easily mod "Danganronpa V3: Killing Harmony", as long as the mod (you wish to install) is providing you the necessary .ups files.

For example, it allows you to install the unofficial Italian translation!

It supports the "Legacy" (Steam) version, the "Xbox" (Microsoft Store on PC) version, and the "Unity" (Switch) version.

Note: this program will NOT handle conflicts between files!

For example, if two mods need to edit the same file, then that file might get overwritten, and the mod(s) might not work correctly!

# Supported games

- Danganronpa V3: Killing Harmony

- AI: The Somnium Files

- Tokyo Psychodemic

## Localization ##

Feel free to localize this tool into your preferred language!

All you need to do is boot the program once, wait for it to create the Localization folder, copy the English subfolder and rename it, then translate the files inside your newly-renamed folder.

Once you're done, restart the tool in order for it to scan the subfolders, and hopefully it should load your new localization.

If you wish to include your localization in our next release of this tool, please contact us.

You may find an invite to our Discord server here: https://indacosub.github.io/about/useful.html#discord

## License ##

This tool is licensed under the ISC License.

"V3UPSManager" depends on "UPS Tools" by rameshvarun (https://github.com/rameshvarun/ups), licensed under the MIT license (https://github.com/rameshvarun/ups/blob/master/LICENSE).

Since version 1.2, "V3UPSManager" also depends on "xdelta-gpl" by jmacd (https://github.com/jmacd/xdelta-gpl/tree/release3_1), licensed under the GPLv2 (or newer) license (https://github.com/jmacd/xdelta-gpl/blob/release3_1/xdelta3/xdelta3.h)

The source code for "V3UPSManager" may be compiled without "UPS Tools" or "xdelta-gpl".

Those tools are only needed at runtime and are, therefore, only included and redistributed in our release builds (found here: https://github.com/IndacoSub/V3UPSManager/releases), along with the source code, the license notice and a summary of any changes made, in order to fully comply with the MIT and GPLv2 licenses.

## Credits ##

     “DANGANRONPA” is a registered trademark of Spike Chunsoft Co., Ltd., Too Kyo Games, LLC and/or NIS America Inc.
	 "AI: THE SOMNIUM FILES" is a registered trademark of Spike Chunsoft Co., Ltd., Too Kyo Games and/or NIS America Inc.
	 "TOKYO PSYCHODEMIC" is a registered trademark of Gravity Co., Ltd and GRAVITY GAME ARISE Co., Ltd.
     We are not in any way affiliated or associated with them.
     
     Thanks to rameshvarun for the UPS command line tools, "UPS Tools" (https://github.com/rameshvarun/ups)
     The UPS command line tools are licensed under the MIT license (https://github.com/rameshvarun/ups/blob/master/LICENSE).
	 
	 Thanks to jmacd for the XDelta 3.1 command line tools, "xdelta-gpl" (https://github.com/jmacd/xdelta-gpl/tree/release3_1)
	 The XDelta 3.1 command line tools are licensed under the GPLv2 (or newer) license (https://github.com/jmacd/xdelta-gpl/blob/release3_1/xdelta3/xdelta3.h)
     
     Thanks to all of our collaborators and reviewers.

## Development (.NET 10 LTS)

The manager and wizard require Windows and the **.NET 10 Desktop Runtime**, or a self-contained Windows build. Development requires the **.NET 10 SDK**. `global.json` selects a compatible .NET 10 SDK feature band.

The solution contains:

- `V3UPSManager`: the existing WinForms manager and localized presentation. Its visual layout is unchanged.
- `V3UPSManager.Core`: portable game definitions, installation validation, patch discovery, patch transactions, backup/snapshot services, and wizard package preparation. It has no WinForms dependency.
- `V3UPSWizard`: a setup-style front end for selecting a patch package, reading its instructions, choosing a game installation, reviewing the plan, and installing through the same Core services as the manager.
- `V3UPSManager.Core.Tests`: regression tests that also run on Linux.

```sh
dotnet restore V3UPSManager.sln -p:EnableWindowsTargeting=true
dotnet build V3UPSManager.sln --no-restore -c Release -p:EnableWindowsTargeting=true
dotnet test V3UPSManager.Core.Tests/V3UPSManager.Core.Tests.csproj --no-build -c Release
```

`EnableWindowsTargeting` permits cross-compilation on Linux; running either WinForms application still requires Windows. CI builds the solution and runs Core tests on Windows and Linux.

### Wizard packages and distribution

Publish `V3UPSWizard.exe` and/or `V3UPSManager.exe` with their .NET dependencies, including `V3UPSManager.Core.dll`, and the required external patch tools. The wizard installs directly and does not require the manager executable. The wizard accepts an extracted patch folder or a `.DAIX.7z` archive. Archive packages require `7za.exe` next to the wizard; ordinary patch folders do not. Distributors must supply the external tools and their applicable licenses separately, as described above.

A package can contain `README.txt` or `LEGGIMI.txt`, one optional `*.game` file containing `DanganronpaV3`, `AITheSomniumFiles`, or `TokyoPsychodemic`, and either loose UPS/XDelta patches or one nested `PATCH.7z`. Patch paths must mirror the game's directory structure and follow the existing `_patch.ups` / `_patch.xdelta` naming convention. Archive entries must be relative paths without links. The wizard keeps temporary extracted patches available through installation, then cleans up its own temporary directory when it closes. Package extraction can be cancelled; an active patch transaction must finish or roll back before the wizard can close. It never deletes the selected source folder.

To publish both applications together from PowerShell:

```powershell
./scripts/publish.ps1
```

This creates a self-contained `win-x64` distribution under `artifacts/windows-x64`. Add the licensed patch tools (`ups.exe` and/or `xdelta3-3.1.0-x86_64.exe`) and `7za.exe` for the workflows you distribute. Publication does not download those tools or include game/mod files.

### Recovery behavior

The Core validates the complete patch plan before writing game files. It preserves existing clean `_bak` files, stages all patch outputs before publication, and keeps separate copies of the live files for rollback. A failed reinstall restores the pre-attempt live content rather than replacing an existing mod with the clean original. If automatic rollback fails, `.v3ups_rollback.*` files remain available for manual recovery. Clean snapshots use original backups; modded snapshots use the live files. Snapshot destinations must not overlap the game installation.
