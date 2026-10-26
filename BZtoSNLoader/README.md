# BZtoSNLoader

BZtoSNLoader is a library mod that provides other mods with the utilities to load Below Zero assets. 
It does **NOT** add anything on its own. Mods that depend on BZtoSNLoader will add Below Zero assets to the world using this library.



## Developer Usage
An command is added to spawn BZ addressables for testing. Try this command to spawn the shadow leviathan prefab:
```c#
  spawnbz WorldEntities/Creatures/Expansion/ShadowLeviathan.prefab
```

Add this mod as a dependency:
```c#
[BepInDependency("com.snmodding.BZtoSNLoader", "1.0.0")] // please use the latest version #, this may be out of date
public class Plugin : BaseUnityPlugin
{ ... }
```

Loading assets from BZ is as simple as getting the addressables path. There are ways to view them when running BZ, but a list of all available can be found here.
Getting a BZ prefab is very similar to requesting a prefab from SN's existing addressables:
```c#
private static IEnumerator SpawnAsync()
{
    const string shadowAddressablesPath = "WorldEntities/Creatures/Expansion/ShadowLeviathan.prefab";
    TaskResult<GameObject> requestResult = new();
    yield return BZAssetLoader.LoadAssetAsync(new(shadowAddressablesPath), requestResult);
    GameObject shadowLeviathanPrefab = requestResult.Get();
}
```
> [!WARNING]
> When a BZ prefab is loaded, all MonoBehavior scripts on the Prefab will have been stripped (to prevent load errors).
> So the Unity Animator, Mesh, and Colliders will still exist in the hierarchy, but any creature behavior/custom scripts are gone.

## License

License: MIT (see LICENSE.md)

> [!NOTE]  
> This library does not redistribute Subnautica: Below Zero's assets. It loads the requested assets directly from Below Zero's files, meaning you Must own a copy of BZ to use this mod (and any that depend on it). Unknown Worlds has given permission to load assets from one game to another under the condition that no assets are redistributed. This project is not affiliated with Unknown Worlds.

Copyright (c) 2026 Jbeast291 & Kallie23
