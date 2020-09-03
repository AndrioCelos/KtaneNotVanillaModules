# Not Vanilla Modules

This is a mod for the game [_Keep Talking and Nobody Explodes_](https://keeptalkinggame.com/) which adds new modules that look suspiciously similar to the vanilla modules.

A build is available on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2003251353).

Manuals are available on the [Repository of Manual Pages](https://ktane.timwi.de/).

Based on the [_Keep Talking and Nobody Explodes_ modkit](https://github.com/keeptalkinggame/ktanemodkit/). Thanks to Kusane for the module designs.

## How to build

Building this mod is a little more involved than with most mods.

1. Open [the helper plugin Visual Studio project](NotVanillaModulesLib/NotVanillaModulesLib.csproj) with a text editor and ensure the variables `UnityInstallPath` and `GameInstallPath` are set to the correct paths for your installations. Then delete the `PreBuildEvent` directive.
2. Set the build configuration to Debug and build the helper plugin.
3. Open this repository using Unity 2017 LTS.
4. In Unity, select `Keep Talking ModKit` → `Configure Mod` and update the build path to match your installation.
5. Select `Keep Talking ModKit` → `Build AssetBundle`.
6. Set the build configuration to Release and rebuild the helper plugin. The library will automatically be copied to the installed mod directory.

See [the helper plugin readme file](NotVanillaModulesLib/readme.md) for more information.
