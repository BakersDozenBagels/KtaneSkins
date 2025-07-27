# KTANE Module Skins

This mod contains skins for modded modules.
SKins are applied on top of the existing prefabs; as such, the original modules are not redistributed.

Skins are designed to not be advantageous, outside of identifying modules.

## Configuration

Skins are chosen randomly, weighted by the configuration settings.

For example, this configuration will choose the vanilla look for Art Appreciation 6/13ths of the time, and the EaTEoT skin 7/13ths of the time:

```json
"AppreciateArt": {
    "$": 7,
    "EaTEoT": 6
}
```

New modules are added to the settings automatically. The default `$` skin is set to have 0 weight, and each other skin is set to have 1 weight by default.

# Contributing

To make a new skin, create a new directory in `Assets/Skins`, and within, create a script for the skin derived from `ModuleSkin`.
Override `ModuleId` and `Name` appropriately.

`ModuleSkinService` will then automatically detect and load the skin.
Note that the skin script will be added as a component to the service (to read its metadata), so Unity messages such as `Start` should not be used.

## Changing Audio

To change audio, override the `SoundOverrides` property. This maps from audio clip names in the source mod to audio clip names in this mod.

## Changing Visuals

When `OnStart` is called, your script will already be attached to the module as a component. This means you can access `transform` to get parts of the source prefab.

To add new parts to the module, use `AddPrefab()`. This will instantiate a prefab registered with the service as a child of the skinned module. These are looked up by name, and the name defaults to `$"{ModuleID}_{Name}"`.

If you only need a shared prefab instance (e.g. to get a `Texture` in Art Appreciation), you can use `GetPrefab()` to retrieve it.

## Reflection

Any reflection should be cached, and ideally done using `System.Linq.Expressions`. This should happen in `Initialize()`.
