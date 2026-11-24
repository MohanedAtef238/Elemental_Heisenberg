# Elemental Vial Combinations Report

**Analysis Date:** 2026-04-29 03:58
**Classification:** Alchemical Combinatorics & Elemental Physics

This report details the hypothesized interactions between base elemental effects and the secondary "Twist" system—where alchemical results can be transmuted into advanced Tier 2 vials.

---

## Phase 1: Base Elements Registry (Tier 1)

| Element | Reference Prefab | Primary Color | Alchemical Property |
|---|---|---|---|
| **Fire** | `BigExplosion` | #FF4500 | Thermal Combustion |
| **Water** | `BigSplash` | #1E90FF | Fluid Conductivity |
| **Lightning** | `Electro hit` | #FFFF00 | Kinetic Discharge |
| **Ice** | `Freeze circle` | #ADFF2F | Endothermic Stasis |
| **Earth** | `Multiple Earth Orb`| #8B4513 | Physical Density |
| **Air** | `DustStorm` | #D2B48C | Atmospheric Pressure |
| **Magic** | `Magic circle` | #9370DB | Ethereal Resonator |
| **Holy** | `Healing` | #FFFACD | Vital Restoration |
| **Dark** | `Debuff` | #4B0082 | Entropic Decay |
| **Void** | `Meteors AOE` | #000000 | Gravitational Singularity |

---

## Phase 2: Tier 1 Combinations

Basic interactions resulting in stable elemental mixtures.

| Base A | Base B | Resulting Vial | The Vibes | Difficulty (Coins) |
|---|---|---|---|---|
| Fire | Water | **Scalding Steam** | Boiling mist that obscures vision and deals heat damage. | 45 |
| Lightning | Water | **Conductive Plasma** | Electrified liquid that causes chain-reactions. | 65 |
| Earth | Fire | **Molten Slag** | Persistent floor burn and high viscosity slow effect. | 55 |
| Earth | Water | **Petrifying Mud** | Sludge that gradually roots targets in stone. | 50 |
| Holy | Water | **Pure Essence** | HP regeneration and status cleanse. | 35 |

---

## Phase 3: The "Twist" - Advanced Transmutations (Tier 2)

Certain alchemical results can be stabilized into a "Secondary Base" vial, which can then be combined with a third element to create Legendary effects.

| Tier 1 Result | + Ingredient | Tier 2 Final Vial | The Vibes | Difficulty (Coins) |
|---|---|---|---|---|
| **Scalding Steam** | Lightning | **Superheated Ion Cloud** | A high-pressure electrical storm that shreds physical armor and deals massive AOE. | 120 |
| **Molten Slag** | Ice | **Volcanic Glass** | Brittle obsidian shards that explode into shrapnel when stepped on. | 90 |
| **Conductive Plasma**| Magic | **Aetheric Battery** | A hovering orb that zaps nearby enemies and recharges friendly mana. | 110 |
| **Petrifying Mud** | Dark | **Living Quicksand** | A dark vortex on the ground that pulls enemies into the earth, suffocating them. | 85 |
| **Pure Essence** | Magic | **Elixir of Divinity** | Grants temporary invulnerability and a golden "Halo" aura that heals allies. | 150 |
| **Molten Slag** | Air | **Pressure Bomb** | A compressed fireball that detonates with a massive shockwave, knocking back all units. | 80 |

---

## Phase 4: Alchemical Notes
>
> [!IMPORTANT]
> **Recursive Stability:** Combining a Tier 2 vial with a Void element has a 95% chance of resulting in a `Void Collapse`. This process is currently undocumented and highly dangerous.
>
> [!TIP]
> Use the `PlasmaExplosionEffect` VFX to represent successful Tier 2 transmutations to provide visual feedback for high-tier crafting.
