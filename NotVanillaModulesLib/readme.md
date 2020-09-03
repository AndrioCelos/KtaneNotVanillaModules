# Not Vanilla Modules Helper Plugin

This is a Unity plugin that contains scripts that connect a Not Vanilla script with the module components.

There are two versions of this plugin, determined by the build configuration. The Debug version excludes references to the vanilla game and enables test models of the modules, allowing the modules to work in the test harness. The Release version uses the vanilla game's prefabs to make the modules look much more like the vanilla ones.

A Debug build will automatically copy the build artifact to the Unity plugins directory. A Release build will copy it to the installed mod directory.

Note that this plugin deals _only_ with connecting module components. It does not contain any puzzle logic, Twitch Plays command parsing logic, or anything not specific to either the live or test models.
