# Not Vanilla Modules

This is a mod for the game [_Keep Talking and Nobody Explodes_](https://keeptalkinggame.com/) which adds new modules that look suspiciously similar to the vanilla modules.

A build is available on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2003251353).

Manuals are available on the [Repository of Manual Pages](https://ktane.timwi.de/).

Based on the [_Keep Talking and Nobody Explodes_ modkit](https://github.com/keeptalkinggame/ktanemodkit/). Thanks to Kusane for the module designs.

## How to build

Building this mod is a little more involved than with most mods.

1. Open this repository using Unity 2017 LTS.
2. Open [the helper library Visual Studio project](NotVanillaModulesLib/NotVanillaModulesLib.csproj) with a text editor and ensure the variables `UnityInstallPath` and `GameInstallPath` are set to the correct paths for your installations. You will also need to reconfigure the mod build path in Unity.
3. Set the build configuration to Debug and build the helper library. This will allow Unity to import the library.
4. In Unity, select `Keep Talking ModKit` → `Build AssetBundle`.
5. Set the build configuration to Release and rebuild the helper library. The library will automatically be copied to the installed mod directory.

See [the helper library readme file](NotVanillaModulesLib/readme.md) for more information.
