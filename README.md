# V3UPSManager

A tool that allows you to easily mod "Danganronpa V3: Killing Harmony", as long as the mod (you wish to install) is providing you the necessary .ups files.

For example, it allows you to install the unofficial Italian translation!

It supports both the "Legacy" (Steam) version and the "Anniversary Edition" (Switch) version.

We'd also like to support the Xbox (Microsoft Store, Anniversary Edition) version, but due to our limited technical knowledge about .arc (ARC0) files, we cannot support it.

Note: this program will NOT handle conflicts between files!

For example, if two mods need to edit the same file, then that file might get overwritten, and the mod(s) might not work correctly!

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
     We are not in any way affiliated or associated with them.
     
     Thanks to rameshvarun for the UPS command line tools, "UPS Tools" (https://github.com/rameshvarun/ups)
     The UPS command line tools are licensed under the MIT license (https://github.com/rameshvarun/ups/blob/master/LICENSE).
	 
	 Thanks to jmacd for the XDelta 3.1 command line tools, "xdelta-gpl" (https://github.com/jmacd/xdelta-gpl/tree/release3_1)
	 The XDelta 3.1 command line tools are licensed under the GPLv2 (or newer) license (https://github.com/jmacd/xdelta-gpl/blob/release3_1/xdelta3/xdelta3.h)
     
     Thanks to all of our collaborators and reviewers.
