# Nine Sols Example Mod

## Set up your mod
1. Set up modding with BepInEx and r2modman: [Wiki: Getting Started](https://github.com/nine-sols-modding/Resources/wiki/Getting-started)
   1. download the `NineSolsAPI` mod if you want to use it, 
2. clone this repo ([generate from this template](https://github.com/new?template_name=NineSols-ExampleMod&template_owner=jakobhellermann), then update the `.csproj`
   1. Change `<AssemblyName>` to your mod name
   2. Make sure the `<NineSolsPath>` points to the installed game
3. Install `tcli` tool for building thunderstore mods: `dotnet tool install -g tcli`
4. Follow the **Building** section to make sure everything works as expected. Load into a game and press `Ctrl-H` to toggle your hat wherever you are.

_Next steps_:
- setup hot reloading for faster iteration times
- use a tool like [ILSpy](https://github.com/icsharpcode/ILSpy) or [dnSpy](https://github.com/dnSpy/dnSpy) to decompile the game code
- check out the [UnityExplorer](https://thunderstore.io/c/nine-sols/p/ninesolsmodding/UnityExplorer/) mod to investigate objects in the game

## Building

If you run
```sh
dotnet publish
```
it will build the DLL of your mod (`Source/bin/Release/netstandard2.1/publish/ExampleMod.dll`), then use `tcli` to
package the mod into a thunderstore-compatible zip in `thunderstore/build/`.

You can import that mod into your r2modman instance like this:
<img alt="r2modman config to import local mod" src="https://github.com/user-attachments/assets/c8e02c83-5d71-4a65-89ef-acf93db85327" width="600">


## Publishing

Make sure to fill out all fields in the [thunderstore.toml](./thunderstore/thunderstore.toml).
Then go to https://thunderstore.io/c/nine-sols/create and upload your mod zip, or use tcli with a token created in
[thunderstore.io](thunderstore.io) at `Settings / Teams / Service Accounts`:
```sh
tcli build --config-path ../thunderstore/thunderstore.toml --token $token
```

## Hot Reloading

Building the mod and restarting the game after every minor change becomes cumbersome quickly. Luckily, BepInEx supports hot reloading of DLLs via [ScriptEngine](https://github.com/BepInEx/BepInEx.Debug).

Download the [ScriptEngine](https://thunderstore.io/c/nine-sols/p/ninesolsmodding/BepinExScriptEngine/) mod in your r2modman instance, and the game will be able to reload DLLs from `r2modmanProfileFolder/BepInEx/scripts/`.

Go into the `ExampleMod.csproj` file and fill out the `<ProfileDir>` and uncomment the `<CopyDir>` below it.
Now, whenever you hit "Build" in your IDE, the mod DLL will be placed into that `scripts` folder.

Note: **Disable your mod in r2modman if it is active to prevent it from being loaded twice!**

By default, ScriptEngine will only reload scripts when you press `F6`, but you can go into r2modman's Config Editor and 
edit `BepInEx\config\com.bepis.bepinex.scriptengine.cfg` to have
- `EnableFileSystemWatcher=true`
- `AutoReloadDelay=0`
- `LoadOnStart=true`

to reload scripts immediately.

Hot reloading works by first destroying your mod instance game objects and then reinstantiating them, so make sure to clean
up any state you left in the `OnDestroy` callback.