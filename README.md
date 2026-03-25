# Elemental Heisenberg
## Architecture Overview

The interaction system is split into three layers:

```
ItemDefinition (ScriptableObject)
    └── defines what a type of item looks and is called


ItemInstance (MonoBehaviour, on scene objects)
    └── says "this object is a GreenFlask" by pointing at an ItemDefinition
    └── listens for physical collisions and fires an event


InteractionCoordinator (MonoBehaviour, on XR Interaction Manager)
    └── holds a list of Interactions
    └── each Interaction pairs two ItemDefinitions and an InteractionResult
    └── when a collision event matches a pair, the result is applied
```

The design is **data-driven**:
- No scripting is needed to add new combinations since everything is wired in the Inspector.
- Matching is order-independent: Green+Blue fires the same as Blue+Green.
- Once both items collide and a rule matches, both are deactivated before the result runs,
  so events only fire once even though both objects detect the same collision.

---

## How to Create a New interaction
### you start by defining new items, then pair them in the InteractionCoordinator

1. **Create an ItemDefinition asset**
   Right-click in the Project window → `Create > Interactions > Item Definition`

2. **Fill in the fields:**

   | Field | Purpose |
   |---|---|
   | `Item Id` | Machine-readable unique key (e.g. `fire_flask`). Used internally for debugging. |
   | `Display Name` | Human-readable name shown in logs or future UI (e.g. `Fire Flask`). |
   | `Effect Prefab` | A particle system prefab parented to the object (aura, glow, etc.). Optional. |
   | `Trail Prefab` | A trail/ribbon VFX prefab also parented to the object. Optional. |

   > **Note:** `EffectPrefab` and `TrailPrefab` are metadata for future procedural spawning.
   > In the current build, VFX are placed manually as children of the item in the scene.
   > These fields are here so a future `ItemSpawner` can instantiate fully-dressed items from scratch.

3. **Place a scene object and add `ItemInstance`**
   - Create or place a GameObject (Capsule, imported mesh, etc.)
   - Add component: `Interactions > Item Instance`
   - Assign the `ItemDefinition` you just created to the `Definition` field
   - Ensure a `Rigidbody` and `Collider` are present — physical collision is required

4. **Add `XRGrabInteractable`** if the player should be able to pick it up
   - Add component: `XR Interaction Toolkit > XR Grab Interactable`
   - Optionally add `GrabHighlight` for a glow-on-hover effect

---

## How to Add a New Interaction

All interactions are configured on the **`InteractionCoordinator`** component, which lives on the
`XR Interaction Manager` scene object.

1. Select `XR Interaction Manager` in the Hierarchy
2. Find the `Interaction Coordinator` component
3. Expand `Interactions` and click **+** to add a new entry

### Interaction Fields

| Field | Purpose |
|---|---|
| `Label` | A human-readable name for this rule (e.g. `Green + Blue → Sunset`). Editor-only, no effect at runtime. This aids the debugging process greatly. |
| `Item A` | First `ItemDefinition` in the pair. Order does not matter. |
| `Item B` | Second `ItemDefinition` in the pair. |
| `Result` | What happens when these two items collide (see below). |

### InteractionResult Fields

#### Skybox
| Field | Purpose |
|---|---|
| `Change Sky` | Toggle on to replace the skybox when this interaction fires. |
| `Skybox Material` | The skybox material to switch to. All Fantasy Skybox FREE materials for now|

> The switch creates a runtime clone of the material so the original asset is never modified during Play mode.

#### Screen Tint
| Field | Purpose |
|---|---|
| `Apply Screen Tint` | Toggle on to push a colour overlay through the URP post-processing volume. |
| `Screen Tint Color` | The tint colour. Alpha is ignored, a full-opacity override is applied. Use subtle desaturated colours for weather moods; saturated colours for dramatic effects. The tint persists until manually cleared or the scene reloads. |

> The tint is applied via a `ColorAdjustments` override on the `ScreenTintController` Volume
> (priority 100, global). It does not conflict with the existing global bloom volume.

#### Sound
| Field | Purpose |
|---|---|
| `Sound Effect` | An `AudioClip` played once at full volume when the interaction fires, via `PlayOneShot`. It does not loop and does not block subsequent interactions. |

#### Weather Effect
| Field | Purpose |
|---|---|
| `Weather Effect Prefab` | A prefab instantiated at the midpoint between the two colliding objects. Intended for rain, snow, fog, lightning or any world-space effect. Currently a placeholder since weather assets are not yet implemented. |

---

## Quick Checklist for a New Combination for the future

- [ ] Created an `ItemDefinition` SO for each new item
- [ ] Scene object has `ItemInstance` → `Definition` assigned
- [ ] Scene object has `Rigidbody` + `Collider`
- [ ] New `Interaction` entry added to `InteractionCoordinator` with both definitions
- [ ] `Result` configured (at minimum toggle `Change Sky` or `Apply Screen Tint`)
- [ ] Tested in Play mode — throw both items at each other to confirm
