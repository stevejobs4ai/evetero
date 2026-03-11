# OSRS Skill Tree Research + Evetero Skill Config Schema

> Research document for Evetero — mobile fantasy strategy game.
> Covers OSRS skill mapping, proposed Evetero skill set (10 skills), JSON schema, dependency web, and comparison table.
> Written: 2026-03-11

---

## Table of Contents

1. [Full OSRS Skill List](#1-full-osrs-skill-list)
2. [Mobile Fitness Analysis](#2-mobile-fitness-analysis)
3. [Proposed Evetero Skill List](#3-proposed-evetero-skill-list)
4. [Milestone Unlock Tables](#4-milestone-unlock-tables)
5. [Cross-Skill Dependencies](#5-cross-skill-dependencies)
6. [Combat Integration](#6-combat-integration)
7. [SkillDefinition JSON Schema](#7-skilldefinition-json-schema)
8. [Resource Interdependency Web](#8-resource-interdependency-web)
9. [OSRS → Evetero Comparison Table](#9-osrs--evetero-comparison-table)
10. [XP Curve Reference](#10-xp-curve-reference)

---

## 1. Full OSRS Skill List

OSRS has **23 skills** across four broad categories.

### 1.1 Combat Skills (7)
These are combat-only and have no non-combat equivalent in Evetero (Evetero uses a flat combat stat system per hero).

| Skill      | What It Does                                                             |
|------------|--------------------------------------------------------------------------|
| Attack     | Increases melee accuracy; unlocks higher-tier melee weapons              |
| Strength   | Increases melee max hit                                                  |
| Defence    | Reduces incoming damage; unlocks armour                                  |
| Hitpoints  | Total HP pool; grows with combat XP                                      |
| Ranged     | Bow/crossbow accuracy and damage; unlocks ranged gear                    |
| Magic      | Spellcasting damage and accuracy; unlocks spells, staves                 |
| Prayer     | Activatable combat buffs (protect prayers, damage boosts); uses bones    |

### 1.2 Gathering Skills (5)
Produce raw materials consumed by artisan skills.

| Skill      | Gathers                          | Feeds Into              | Key mechanic                     |
|------------|----------------------------------|-------------------------|----------------------------------|
| Mining     | Ores, gems, essence              | Smithing, Crafting, RC  | Rock respawn timers; tick mining |
| Woodcutting| Logs (normal → magic)            | Fletching, Firemaking   | Tree respawn; axe tier gates     |
| Fishing    | Raw fish (shrimp → anglerfish)   | Cooking                 | Spot movement; bait/lure methods |
| Farming    | Crops, herbs, trees, bushes      | Cooking, Herblore       | Real-time grow timers            |
| Hunter     | Chinchompas, kebbits, birds      | Ranged ammo, bait       | Trap placement mechanics         |

### 1.3 Artisan / Production Skills (9)
Transform raw materials into finished goods.

| Skill        | Produces                                    | Requires                | Key mechanic                          |
|--------------|---------------------------------------------|-------------------------|---------------------------------------|
| Smithing     | Metal bars → weapons, armour, nails          | Mining (ores)           | Furnace → anvil two-step process      |
| Cooking      | Cooked food (healing consumables)            | Fishing, Farming        | Burn rate decreases with level        |
| Fletching    | Bows, arrows, bolts, darts                  | Woodcutting             | Fast, click-intensive; good GP method |
| Crafting     | Leather armour, jewelry, glass, pottery      | Mining (gems), Farming  | Many subskills in one                 |
| Herblore     | Potions (combat, skilling, utility buffs)    | Farming (herbs)         | Unfinished potion + ingredient        |
| Firemaking   | Fires, lanterns, beacons; Wintertodt mini    | Woodcutting             | Mostly XP sink; limited utility       |
| Runecrafting | Runes (all magic spell components)           | Mining (rune essence)   | Altar run grind; abyss method         |
| Construction | Player-owned house rooms, furniture, portals | Woodcutting, Mining     | House as utility hub                  |
| Thieving     | GP, resources via pickpocket/stall/chest     | Nothing (self-contained)| Click timing, fail state punishment   |

### 1.4 Support Skills (2)
Skills that provide traversal, access, or alternative resource acquisition.

| Skill    | What It Does                                              | Key mechanic                            |
|----------|-----------------------------------------------------------|-----------------------------------------|
| Agility  | Shortcut access, run energy regen; rooftop courses        | Obstacle click-timing; fail-and-fall    |
| Slayer   | Unlocks monsters you can kill; task-based combat grind    | Slayer master assignment system         |

---

## 2. Mobile Fitness Analysis

### 2.1 Skills that translate well to mobile
These work because they have clear resource loops, no tick-perfect input, and map to idle/async play.

| OSRS Skill   | Mobile Fit | Reason                                                              |
|--------------|------------|---------------------------------------------------------------------|
| Mining       | Excellent  | Clear gather loop; can be async / hero-assigned                     |
| Woodcutting  | Excellent  | Same; feeds crafting pipeline                                       |
| Fishing      | Excellent  | Same; feeds cooking; thematic fantasy                               |
| Farming      | Excellent  | Real-time grow timers = perfect idle mechanic                       |
| Smithing     | Excellent  | Satisfying gear progression; visible power upgrades                 |
| Cooking      | Excellent  | Consumables are a clear in-combat use case                          |
| Crafting     | Excellent  | Wide coverage (leather, jewelry, pottery); flexible                 |
| Fletching    | Good       | Useful for Ranger hero specialisation; minor simplification needed  |
| Herblore     | Good       | Potion system is universally loved; maps to Herbalism               |
| Construction | Moderate   | Could map to base-building (already in city builder loop)           |
| Thieving     | Moderate   | Could be a one-off event/expedition mechanic, not a skill           |
| Agility      | Poor       | No map traversal in a strategy game; only mechanic is run energy    |
| Hunter       | Poor       | Trap placement requires real-time input; poor async fit             |
| Firemaking   | Poor       | Mostly an XP sink in OSRS; no strong standalone use case            |
| Runecrafting | Poor       | Rune economy doesn't exist in Evetero; complex for no payoff        |
| Slayer       | Poor       | Task-based combat assignment; this is combat, not a skill           |

### 2.2 Combat skills
All 7 OSRS combat skills (Attack, Strength, Defence, Hitpoints, Ranged, Magic, Prayer) are **excluded** — Evetero handles combat via per-hero stat blocks and the three combat types (Warrior / Ranger / Mage). Non-combat skills influence combat only through **gear and consumables**.

---

## 3. Proposed Evetero Skill List

**10 skills** — matching the 8 already in `SkillType.cs` plus 2 additions (Herbalism, Alchemy). Organised into four categories.

### Category: Gathering (4 skills)

#### 1. Woodcutting
- **Category:** Gathering
- **Description:** Heroes fell trees across the world of Evetero to collect logs and rare timber. Higher levels unlock ancient groves and magical trees that produce enchanted wood used in advanced crafting.
- **Resource produced:** Logs (Normal → Oak → Willow → Maple → Yew → Magic)
- **Feeds into:** Crafting (handles, hafts, furniture), Fletching (bows, bolts), Alchemy (enchanted wood essence)

#### 2. Mining
- **Category:** Gathering
- **Description:** Heroes excavate ore veins, gemstone deposits, and ancient ruins. Higher levels access deeper veins yielding rarer metals and gemstones critical to gear progression.
- **Resource produced:** Ore (Copper/Tin → Iron → Coal → Mithril → Adamant → Runite), Gems (Sapphire → Diamond)
- **Feeds into:** Smithing (bars → gear), Crafting (gem settings, jewelry)

#### 3. Fishing
- **Category:** Gathering
- **Description:** Heroes fish in rivers, lakes, and ocean shores. Beyond food, rare deep-sea catches yield alchemical components and tradeable luxuries that fuel the economy.
- **Resource produced:** Raw fish (Sardine → Trout → Salmon → Lobster → Shark → Leviathan Fin)
- **Feeds into:** Cooking (healing consumables), Herbalism (fish oil extracts at high level)

#### 4. Farming
- **Category:** Gathering
- **Description:** Heroes cultivate crops, herbs, and magical plants. Farming operates on real-time grow timers — heroes plant before a campaign and harvest on return.
- **Resource produced:** Crops (Potato → Cabbage → Wheat → Pineapple), Herbs (Guam → Ranarr → Snapdragon → Torstol)
- **Feeds into:** Cooking (food ingredients), Herbalism (potion bases)

---

### Category: Crafting (4 skills)

#### 5. Smithing
- **Category:** Crafting
- **Description:** Heroes smelt raw ores into bars at a forge, then hammer bars into weapons, armour, and tools at an anvil. Smithing is the primary path to metal combat gear for Warriors and Rangers.
- **Resource produced:** Bars (Bronze → Iron → Steel → Mithril → Adamant → Runite) → Weapons, Armour, Arrowheads
- **Requires:** Mining (ores)
- **Combat link:** Unlocks metal armour tiers for Warriors; arrowheads and bolt tips for Rangers

#### 6. Cooking
- **Category:** Crafting
- **Description:** Heroes prepare raw ingredients into food that restores HP during and after combat. Higher levels produce meals that grant temporary combat buffs beyond simple healing.
- **Resource produced:** Cooked fish/meals (Sardine → Shark Steak → Feast), Combat food (Strength food, Defence ale)
- **Requires:** Fishing (fish), Farming (ingredients)
- **Combat link:** Consumables consumed during battle to restore HP; high-level meals grant +ATK/DEF/MAG buffs

#### 7. Crafting
- **Category:** Crafting
- **Description:** Heroes work leather, cloth, and gemstones into armour, accessories, and decorative items. Crafting is the primary gear path for Rangers and Mages who avoid heavy metal.
- **Resource produced:** Leather armour (Body → Studded → Dragonhide), Jewelry (rings, amulets with passive bonuses), Cloth armour
- **Requires:** Mining (gems), Farming (flax → thread)
- **Combat link:** Leather/dragonhide armour for Rangers; enchanted amulets give passive stat bonuses for all types

#### 8. Fletching
- **Category:** Crafting
- **Description:** Heroes craft ranged ammunition and bows from wood and metal components. A Ranger's power scales directly with the quality of their ammo, making Fletching a specialised but critical craft.
- **Resource produced:** Shortbows → Longbows → Magic bows; Arrows (Bronze tip → Adamant tip → Dragon tip); Crossbow bolts
- **Requires:** Woodcutting (logs → limbs, shafts), Smithing (arrowheads, bolt tips)
- **Combat link:** Directly gates Ranger damage output; higher Fletching = stronger arrow tier available

---

### Category: Support (1 skill)

#### 9. Herbalism
- **Category:** Support
- **Description:** Heroes brew potions from harvested herbs and alchemical ingredients. Potions provide temporary combat boosts, skill efficiency buffs, and defensive wards — the apothecary's art.
- **Resource produced:** Attack potion, Defence potion, Magic potion, Antipoison, Stamina potion, Super combat potion
- **Requires:** Farming (herbs), Fishing (fish oil at high level), Cooking (secondary ingredients)
- **Combat link:** Potions consumed in battle for +15%/+20%/+25% combat stat boosts at tiers; anti-debuff potions counter enemy debuffs

---

### Category: Utility (1 skill)

#### 10. Alchemy
- **Category:** Utility
- **Description:** Heroes study the transmutation of materials and enchantment of objects. Alchemy converts raw surplus resources into refined magical components, enchants gear with passive runes, and at mastery level can transmute one resource type into another.
- **Resource produced:** Enchanted gear (passive bonus upgrades), Magical components (sold or used in Herbalism), Resource conversion (transmute surplus ore into rarer ore at 80+)
- **Requires:** Woodcutting (enchanted wood), Mining (essence stones at 40+)
- **Combat link:** Enchants weapons/armour with elemental runes (Fire rune on sword → bonus fire damage); Mage heroes gain extra enchantment slots

---

## 4. Milestone Unlock Tables

XP milestones from OSRS curve (already implemented in `SkillSystem.cs`):
- Level 1 = 0 XP
- Level 10 = 1,154 XP
- Level 20 = 4,470 XP
- Level 40 = 37,224 XP
- Level 60 = 273,742 XP
- Level 80 = 2,031,628 XP
- Level 92 = 6,517,253 XP ← halfway to 99 by XP
- Level 99 = 13,034,431 XP

### Woodcutting Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Chop Normal Trees (logs)                                      |
| 10    | Chop Oak Trees (oak logs)                                     |
| 20    | Chop Willow Trees (willow logs)                               |
| 40    | Chop Maple Trees (maple logs); unlock second axe slot         |
| 60    | Chop Yew Trees (yew logs); unlock Ancient Grove zone          |
| 80    | Chop Magic Trees (magic logs); passive 10% bonus log yield    |
| 99    | **MASTERY:** Axe of the Forest (unique heirloom axe cosmetic); never fail a tree; 25% bonus logs |

### Mining Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Mine Copper & Tin (bronze bars via Smithing)                  |
| 10    | Mine Iron Ore                                                 |
| 20    | Mine Coal; unlock Gem veins (random sapphires/emeralds)       |
| 40    | Mine Gold; Mine Mithril Ore                                   |
| 60    | Mine Adamantite; unlock Essence Stones (feeds Alchemy)        |
| 80    | Mine Runite; gem drop rate +15%                               |
| 99    | **MASTERY:** Runite Pickaxe (heirloom); +30% ore yield; never deplete a vein |

### Fishing Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Net Fish (sardines, herrings)                                 |
| 10    | Fly Fish (trout, salmon)                                      |
| 20    | Cage Fish (lobsters)                                          |
| 40    | Harpoon Fish (swordfish, tuna)                                |
| 60    | Deep Sea Fish (sharks); unlock Ocean Expedition zone          |
| 80    | Fish Leviathan Fins (rare alchemy ingredient); +10% catch rate|
| 99    | **MASTERY:** Kraken Rod (heirloom); double catch per action; fish any zone |

### Farming Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Plant Potatoes, Onions, Cabbages                              |
| 10    | Plant Tomatoes, Sweetcorn; Herb patch unlocked (Guam, Marrentill) |
| 20    | Plant Pineapples; Ranarr herbs (best early potion herb)       |
| 40    | Plant Watermelons; Snapdragon herbs; second herb patch        |
| 60    | Plant Dragonfruit; Torstol herbs (best potion herb); Tree patches |
| 80    | Magical mushroom patch (provides Alchemy ingredients); +20% yield |
| 99    | **MASTERY:** Eternal Harvest (passive auto-harvest 2× per day without hero action) |

### Smithing Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Smelt Bronze bars; smith Bronze weapons & armour              |
| 10    | Smelt Iron bars; smith Iron armour                            |
| 20    | Smelt Steel bars; smith Steel armour (Warriors T2)            |
| 40    | Smelt Mithril bars; Mithril armour (Warriors T3); craft arrowheads |
| 60    | Smelt Adamant bars; Adamant armour (Warriors T4); crossbow bolts |
| 80    | Smelt Runite bars; Runite armour (Warriors T5, BiS craftable) |
| 99    | **MASTERY:** Masterwork armour (heirloom set); +5% passive damage reduction for equipped hero |

### Cooking Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Cook Sardines (+8 HP), Bread (+5 HP)                          |
| 10    | Cook Trout (+12 HP); 0% burn rate for sardines               |
| 20    | Cook Lobster (+22 HP); unlock Spice Rack (flavour buffs)      |
| 40    | Cook Swordfish (+35 HP); craft Strength Ale (+5% ATK, 3 turns)|
| 60    | Cook Shark (+55 HP); craft Defence Brew (+10% DEF, 5 turns)   |
| 80    | Cook Leviathan Feast (+80 HP, +5 all stats, 3 turns)          |
| 99    | **MASTERY:** Grand Feast (passive +5 HP regen per turn to entire party during combat) |

### Crafting Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Craft Leather Body, Leather Chaps                             |
| 10    | Craft Sapphire Ring (+1 Magic), Studded Leather               |
| 20    | Craft Emerald Amulet (+2 Attack), Gold Jewelry                |
| 40    | Craft Ruby Amulet (+3 Strength), Dragonhide Body (Rangers T3) |
| 60    | Craft Diamond Amulet (+5 all stats), Blue Dragonhide (Rangers T4)|
| 80    | Craft Dragonstone ring (+10% crit); Black Dragonhide (Rangers T5)|
| 99    | **MASTERY:** Onyx Amulet of Power (+8 all stats, unique); craft Void armour set |

### Fletching Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Fletch Arrow Shafts, Bronze Arrows (damage tier 1)            |
| 10    | Fletch Shortbow (willow), Iron Arrows (damage tier 2)         |
| 20    | Fletch Oak Longbow, Steel Arrows (damage tier 3)              |
| 40    | Fletch Maple Shortbow, Mithril Arrows (damage tier 4); Crossbow |
| 60    | Fletch Yew Longbow, Adamant Arrows (damage tier 5)            |
| 80    | Fletch Magic Longbow, Runite Arrows (damage tier 6); Dragon bolts|
| 99    | **MASTERY:** Crystal Bow (heirloom, uses no arrows); +15% Ranger crit chance |

### Herbalism Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Brew Attack Potion (+10% ATK, 2 turns), Antipoison            |
| 10    | Brew Strength Potion (+10% STR), Defence Potion (+10% DEF)    |
| 20    | Brew Ranging Potion (+10% Ranged ATK), Magic Potion (+10% MAG)|
| 40    | Brew Super Combat Potion (+15% all combat stats, 3 turns)     |
| 60    | Brew Stamina Potion (hero acts twice in one turn, 1 use)      |
| 80    | Brew Overload (+25% all combat stats, 5 turns; costs 15 HP)   |
| 99    | **MASTERY:** Elixir of Evetero (permanent +5 to all hero combat stats; one per hero) |

### Alchemy Unlocks
| Level | Unlock                                                        |
|-------|---------------------------------------------------------------|
| 1     | Low Alchemy (convert items to gold; 3× base value)            |
| 10    | Enchant Sapphire (ring/amulet gains +2 magic bonus)           |
| 20    | Enchant Emerald (gear gains +3 attack bonus); High Alchemy (5× value)|
| 40    | Enchant Ruby (weapon gains bleed effect); Transmute Coal→Iron |
| 60    | Enchant Diamond (weapon gains +5% accuracy); Transmute Iron→Mithril|
| 80    | Enchant Dragonstone (armour gains +10% elemental resistance); Transmute Mithril→Runite|
| 99    | **MASTERY:** Enchant Onyx (legendary weapon effect — unique per hero); Infinite transmutation |

---

## 5. Cross-Skill Dependencies

```
DEPENDENCY GRAPH (→ = "required by" / "feeds into")

Gathering ──────────────────────────────────────────────────────────────────
  Woodcutting ──► Fletching (logs → bow limbs, arrow shafts)
              ──► Crafting  (logs → tool handles at 40+)
              ──► Alchemy   (magic logs → enchanted wood essence at 60+)

  Mining ────────► Smithing  (ores → bars)
              ──► Crafting  (gems → jewelry)
              ──► Alchemy   (essence stones at 60+)

  Fishing ───────► Cooking   (raw fish → cooked fish)
              ──► Herbalism (fish oil at 60+)

  Farming ───────► Cooking   (crops → ingredient food)
              ──► Herbalism (herbs → potion bases)
              ──► Crafting  (flax → thread at 20+)

Crafting ────────────────────────────────────────────────────────────────────
  Smithing ──────► Fletching (arrowheads, bolt tips)
  Cooking  ──────► Herbalism (secondary ingredients at 40+)
  Crafting ──────► Alchemy   (gem components at 40+)
  Fletching ─────► (end product: Ranger ammo — no further dependencies)

Support / Utility ──────────────────────────────────────────────────────────
  Herbalism ─────► (end product: combat potions)
  Alchemy   ─────► (end product: enchanted gear, transmuted resources)
```

### Key dependency chains (longest paths):
1. **Full Ranger pipeline:** Woodcutting → Fletching (bows) + Mining → Smithing → Fletching (tips) → Crafting (leather armour) = 4 prerequisite skills
2. **Full potion pipeline:** Farming → Herbalism + Fishing → Herbalism + Cooking → Herbalism = 3 prerequisite skills
3. **Enchanted weapon:** Mining → Smithing (weapon) + Mining → Crafting (gem) + Alchemy (enchant) = 3 prerequisite skills

---

## 6. Combat Integration

### How non-combat skills affect combat (by hero type)

| Skill      | Warrior Bonus                     | Ranger Bonus                       | Mage Bonus                          |
|------------|-----------------------------------|------------------------------------|-------------------------------------|
| Smithing   | Unlocks metal armour tiers (T1-T5)| Unlocks arrowheads (damage gates)  | Unlocks bolt tips (crossbow mages)  |
| Crafting   | Gem amulets (+ATK stats)          | Leather/dragonhide armour (T1-T5)  | Gem amulets (+MAG stats)            |
| Fletching  | —                                 | Unlocks arrow/bow tier (damage)    | Enchanted bolts at 80+              |
| Cooking    | HP consumables; Strength Ale      | HP consumables; Ranging food       | HP consumables; Magic Tea           |
| Herbalism  | Attack/Strength potions           | Ranging potions                    | Magic potions; Overload (all types) |
| Alchemy    | Weapon enchantments (fire, bleed) | Arrow enchantments (poison, freeze)| Extra enchant slots (+1 at 99)      |
| Farming    | Passive (feeds Herbalism)         | Passive (feeds Herbalism)          | Passive (feeds Herbalism)           |
| Mining     | Passive (feeds Smithing)          | Passive (feeds Smithing/Fletching) | Passive (feeds Crafting)            |
| Woodcutting| Passive (feeds Crafting)          | Passive (feeds Fletching)          | Passive (feeds Crafting)            |
| Fishing    | Passive (feeds Cooking)           | Passive (feeds Cooking)            | Passive (feeds Cooking)             |

### Gear tier summary

| Tier | Level Range | Warrior Armour   | Ranger Armour         | Mage Armour         | Source             |
|------|-------------|------------------|-----------------------|---------------------|--------------------|
| T1   | 1-19        | Bronze/Iron      | Leather               | Cloth robes         | Smithing 1 / Craft 1 |
| T2   | 20-39       | Steel             | Studded Leather       | Mystic robes        | Smithing 20 / Craft 20 |
| T3   | 40-59       | Mithril           | Dragonhide            | Enchanted robes     | Smithing 40 / Craft 40 |
| T4   | 60-79       | Adamant           | Blue Dragonhide       | Arcane robes        | Smithing 60 / Craft 60 |
| T5   | 80-98       | Runite            | Black Dragonhide      | Void robes          | Smithing 80 / Craft 80 |
| T6   | 99 (mastery)| Masterwork        | Crystal/Void hybrid   | Arcane Void         | Smithing 99 / Craft 99 |

---

## 7. SkillDefinition JSON Schema

This schema defines a single non-combat skill in data. All skills share this structure, stored as `.json` files loadable as `SkillDefinition` ScriptableObjects.

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "SkillDefinition",
  "description": "Defines a single non-combat skill in Evetero.",
  "type": "object",
  "required": ["id", "name", "category", "description", "xpCurve", "unlocks"],
  "properties": {

    "id": {
      "type": "string",
      "pattern": "^[a-z_]+$",
      "description": "Snake_case identifier matching SkillType enum name. E.g. 'woodcutting'.",
      "examples": ["woodcutting", "mining", "herbalism"]
    },

    "name": {
      "type": "string",
      "description": "Display name shown in UI.",
      "examples": ["Woodcutting", "Mining", "Herbalism"]
    },

    "category": {
      "type": "string",
      "enum": ["Gathering", "Crafting", "Support", "Utility"],
      "description": "Skill category for grouping in the skill UI panel."
    },

    "description": {
      "type": "string",
      "maxLength": 300,
      "description": "Short flavour + function description shown in skill detail view."
    },

    "icon": {
      "type": "string",
      "description": "Path to the skill icon sprite asset (relative to Resources/).",
      "examples": ["Icons/Skills/woodcutting"]
    },

    "xpCurve": {
      "type": "string",
      "enum": ["osrs_standard"],
      "description": "Reference to the XP curve formula. 'osrs_standard' uses the OSRS formula implemented in SkillSystem.cs: total_xp(L) = floor(sum_{n=1}^{L-1} floor(n + 300 * 2^(n/7))) / 4. Level 92 is the XP halfway point to 99.",
      "default": "osrs_standard"
    },

    "primaryResource": {
      "type": "object",
      "description": "The main resource this skill gathers or produces.",
      "required": ["name", "tiers"],
      "properties": {
        "name": {
          "type": "string",
          "description": "Resource family name (e.g. 'Logs', 'Ore', 'Raw Fish')."
        },
        "tiers": {
          "type": "array",
          "description": "Ordered list of resource tiers from lowest to highest quality.",
          "items": {
            "type": "object",
            "required": ["name", "levelRequired"],
            "properties": {
              "name":          { "type": "string" },
              "levelRequired": { "type": "integer", "minimum": 1, "maximum": 99 },
              "xpPerAction":   { "type": "number", "minimum": 0 }
            }
          }
        }
      }
    },

    "unlocks": {
      "type": "array",
      "description": "List of things unlocked at specific level milestones.",
      "items": {
        "type": "object",
        "required": ["level", "type", "description"],
        "properties": {
          "level": {
            "type": "integer",
            "minimum": 1,
            "maximum": 99,
            "description": "The level at which this unlock becomes available."
          },
          "type": {
            "type": "string",
            "enum": [
              "resource_tier",
              "recipe",
              "zone_access",
              "passive_bonus",
              "gear_tier",
              "consumable",
              "mastery_reward"
            ],
            "description": "Category of unlock for programmatic handling."
          },
          "id": {
            "type": "string",
            "description": "Optional: machine-readable ID for the unlocked item/recipe/zone. Used to cross-reference the item database."
          },
          "description": {
            "type": "string",
            "description": "Human-readable description of what this unlock grants."
          },
          "combatBenefit": {
            "type": "string",
            "description": "Optional: description of how this unlock affects combat. Omit if no combat impact."
          },
          "affectedCombatTypes": {
            "type": "array",
            "items": { "type": "string", "enum": ["Warrior", "Ranger", "Mage", "All"] },
            "description": "Which hero combat types benefit from this unlock. Omit if universal."
          }
        }
      }
    },

    "dependencies": {
      "type": "array",
      "description": "Other skills whose outputs this skill consumes as inputs.",
      "items": {
        "type": "object",
        "required": ["skillId", "reason"],
        "properties": {
          "skillId": {
            "type": "string",
            "description": "id field of the skill this skill depends on."
          },
          "levelGate": {
            "type": "integer",
            "minimum": 1,
            "maximum": 99,
            "description": "Optional: minimum level in the dependency skill before this interaction unlocks."
          },
          "reason": {
            "type": "string",
            "description": "Plain-English explanation of the dependency."
          }
        }
      }
    },

    "masteryReward": {
      "type": "object",
      "description": "Special reward granted at level 99. Always type 'mastery_reward' in unlocks array.",
      "required": ["name", "description"],
      "properties": {
        "name":          { "type": "string" },
        "description":   { "type": "string" },
        "isHeirloom":    { "type": "boolean", "description": "True if the reward is a cosmetic heirloom item." },
        "isPassive":     { "type": "boolean", "description": "True if the reward is a permanent passive bonus." },
        "combatBenefit": { "type": "string" }
      }
    },

    "xpActions": {
      "type": "array",
      "description": "List of actions that award XP in this skill.",
      "items": {
        "type": "object",
        "required": ["name", "xpPerAction", "levelRequired"],
        "properties": {
          "name":          { "type": "string" },
          "xpPerAction":   { "type": "number", "minimum": 0 },
          "levelRequired": { "type": "integer", "minimum": 1, "maximum": 99 },
          "inputItems":    { "type": "array", "items": { "type": "string" } },
          "outputItems":   { "type": "array", "items": { "type": "string" } }
        }
      }
    }
  }
}
```

### Example SkillDefinition instance — Fletching

```json
{
  "id": "fletching",
  "name": "Fletching",
  "category": "Crafting",
  "description": "Heroes craft ranged ammunition and bows from wood and metal. A Ranger's damage output scales directly with arrow tier, making Fletching a critical craft for ranged-focused parties.",
  "icon": "Icons/Skills/fletching",
  "xpCurve": "osrs_standard",
  "primaryResource": {
    "name": "Ranged Ammunition",
    "tiers": [
      { "name": "Bronze Arrows",   "levelRequired": 1,  "xpPerAction": 5.0  },
      { "name": "Iron Arrows",     "levelRequired": 10, "xpPerAction": 15.0 },
      { "name": "Steel Arrows",    "levelRequired": 20, "xpPerAction": 35.0 },
      { "name": "Mithril Arrows",  "levelRequired": 40, "xpPerAction": 50.0 },
      { "name": "Adamant Arrows",  "levelRequired": 60, "xpPerAction": 75.0 },
      { "name": "Runite Arrows",   "levelRequired": 80, "xpPerAction": 100.0 },
      { "name": "Dragon Bolts",    "levelRequired": 80, "xpPerAction": 120.0 }
    ]
  },
  "unlocks": [
    {
      "level": 1,
      "type": "recipe",
      "id": "bronze_arrows",
      "description": "Fletch Bronze Arrows (10 per log + arrowheads)",
      "combatBenefit": "Ranger damage tier 1",
      "affectedCombatTypes": ["Ranger"]
    },
    {
      "level": 10,
      "type": "recipe",
      "id": "willow_shortbow",
      "description": "Craft Willow Shortbow",
      "affectedCombatTypes": ["Ranger"]
    },
    {
      "level": 40,
      "type": "recipe",
      "id": "maple_shortbow",
      "description": "Craft Maple Shortbow; Mithril Arrow tips",
      "affectedCombatTypes": ["Ranger"]
    },
    {
      "level": 99,
      "type": "mastery_reward",
      "id": "crystal_bow",
      "description": "Crystal Bow — heirloom bow that requires no arrows; +15% Ranger crit chance",
      "combatBenefit": "+15% critical hit chance for Ranger heroes",
      "affectedCombatTypes": ["Ranger"]
    }
  ],
  "dependencies": [
    {
      "skillId": "woodcutting",
      "levelGate": 1,
      "reason": "Logs provide bow limbs and arrow shafts."
    },
    {
      "skillId": "smithing",
      "levelGate": 10,
      "reason": "Smithed arrowheads and bolt tips are required for all metal ammunition."
    }
  ],
  "masteryReward": {
    "name": "Crystal Bow",
    "description": "An ancient elven bow that draws from ambient magic instead of arrows. Grants +15% critical hit chance to the wielding Ranger.",
    "isHeirloom": true,
    "isPassive": false,
    "combatBenefit": "+15% critical hit chance for Ranger combat type"
  },
  "xpActions": [
    { "name": "Fletch Arrow Shafts",    "xpPerAction": 5.0,  "levelRequired": 1,  "inputItems": ["logs"],              "outputItems": ["arrow_shafts_15"] },
    { "name": "Attach Bronze Tips",     "xpPerAction": 10.0, "levelRequired": 1,  "inputItems": ["arrow_shafts", "bronze_arrowheads"], "outputItems": ["bronze_arrows_15"] },
    { "name": "Fletch Willow Shortbow", "xpPerAction": 33.3, "levelRequired": 10, "inputItems": ["willow_logs"],        "outputItems": ["willow_shortbow_u"] },
    { "name": "String Willow Shortbow", "xpPerAction": 16.5, "levelRequired": 10, "inputItems": ["willow_shortbow_u", "bow_string"], "outputItems": ["willow_shortbow"] }
  ]
}
```

---

## 8. Resource Interdependency Web

```
╔═══════════════════════════════════════════════════════════════════════════╗
║                    EVETERO RESOURCE DEPENDENCY WEB                       ║
╚═══════════════════════════════════════════════════════════════════════════╝

 ┌─────────────┐    logs      ┌───────────────┐
 │ WOODCUTTING │─────────────►│   FLETCHING   │──► Arrows, Bows
 │  (Gathering)│              │   (Crafting)  │        │
 └─────────────┘              └───────────────┘        │ combat
       │                            ▲                   ▼
       │ magic logs                 │ arrowheads    Ranger DMG
       ▼                            │
 ┌─────────────┐              ┌───────────────┐    weapons
 │   ALCHEMY   │◄─────────────│   SMITHING    │──► armour
 │  (Utility)  │ components   │   (Crafting)  │        │
 └─────────────┘              └───────────────┘        │ combat
       │                            ▲                   ▼
       │ enchant gear               │ ores          Warrior DMG/DEF
       ▼                            │               Ranger tips
    Gear +stats               ┌─────────────┐
                               │   MINING    │──► gems
                               │  (Gathering)│    │
                               └─────────────┘    │
                                                  ▼
 ┌─────────────┐   flax/crops  ┌───────────────┐ jewelry
 │   FARMING   │──────────────►│   CRAFTING    │──► Leather armour
 │  (Gathering)│               │   (Crafting)  │──► Amulets/Rings
 └─────────────┘               └───────────────┘        │
       │                                                 │ combat
       │ herbs                                           ▼
       ▼                                           Ranger armour
 ┌─────────────┐   raw fish    ┌───────────────┐   Mage amulets
 │   FISHING   │──────────────►│    COOKING    │──► Healing food
 │  (Gathering)│               │   (Crafting)  │──► Combat food
 └─────────────┘               └───────────────┘        │
       │                            │                    │ combat
       │ fish oil                   │ ingredients        ▼
       ▼                            ▼               HP restore
 ┌─────────────┐◄──────────────┌───────────────┐   Stat buffs
 │  HERBALISM  │    secondary  │  (Cooking 40+)│
 │  (Support)  │   ingredients └───────────────┘
 └─────────────┘
       │
       │ potions
       ▼
  Combat buffs
  (+10% to +25%
   all types)


 LEGEND
 ──►  Resource flows FROM skill TO skill
 ◄──  Resource flows INTO skill FROM skill
 Italics at edge = resource type transferred
```

### Dependency summary by target skill

| Skill      | Depends On                         | Produces for                    |
|------------|------------------------------------|---------------------------------|
| Woodcutting| —                                  | Fletching, Crafting, Alchemy    |
| Mining     | —                                  | Smithing, Crafting              |
| Fishing    | —                                  | Cooking, Herbalism              |
| Farming    | —                                  | Cooking, Herbalism, Crafting    |
| Smithing   | Mining                             | Fletching (tips), Crafting (—)  |
| Cooking    | Fishing, Farming                   | Herbalism (secondary)           |
| Crafting   | Mining (gems), Farming (flax)      | Alchemy (gem components)        |
| Fletching  | Woodcutting, Smithing              | — (end product)                 |
| Herbalism  | Farming, Fishing, Cooking          | — (end product)                 |
| Alchemy    | Woodcutting, Mining, Crafting      | — (end product: enchanted gear) |

---

## 9. OSRS → Evetero Comparison Table

| OSRS Skill    | OSRS Category  | Evetero Adaptation     | Evetero Category | Change Rationale                                                                  |
|---------------|----------------|------------------------|------------------|-----------------------------------------------------------------------------------|
| Mining        | Gathering      | Mining                 | Gathering        | Kept identical — foundational ore loop                                            |
| Woodcutting   | Gathering      | Woodcutting            | Gathering        | Kept identical — foundational log loop                                            |
| Fishing       | Gathering      | Fishing                | Gathering        | Kept identical — food source loop                                                 |
| Farming       | Gathering      | Farming                | Gathering        | Kept — real-time grow timers are ideal idle mechanic for mobile                   |
| Smithing      | Artisan        | Smithing               | Crafting         | Kept — primary gear progression path                                              |
| Cooking       | Artisan        | Cooking                | Crafting         | Kept — consumables are essential combat layer                                     |
| Crafting      | Artisan        | Crafting               | Crafting         | Kept — covers leather, jewelry; renamed category only                             |
| Fletching     | Artisan        | Fletching              | Crafting         | Kept — critical for Ranger specialisation                                         |
| Herblore      | Artisan        | Herbalism              | Support          | Renamed for clarity; same function; potions renamed for Evetero lore              |
| Firemaking    | Artisan        | **Removed**            | —                | OSRS XP sink with no standalone product; replaced by Alchemy for mobile utility   |
| Construction  | Artisan        | **Absorbed into city builder** | —       | Base-building is already the city-builder loop; redundant as a skill              |
| Runecrafting  | Artisan        | **Absorbed into Alchemy** | Utility       | Rune production collapsed into Alchemy; simplifies economy for mobile             |
| Thieving      | Support        | **Expedition mechanic**| —                | Works better as a one-time expedition event than a grindable skill                |
| Agility       | Support        | **Removed**            | —                | No traversal game in a strategy title; run energy mechanic has no equivalent      |
| Slayer        | Combat         | **Removed**            | —                | Task-based combat is handled by the battle system, not a skill                    |
| Hunter        | Gathering      | **Removed**            | —                | Trap mechanics require real-time input; poor fit for mobile async play            |
| *New*         | —              | Alchemy                | Utility          | New skill combining Runecrafting + High Alchemy + Enchanting into one mobile-friendly utility skill |
| Attack        | Combat         | **Removed**            | —                | Combat handled by hero stat blocks (Warrior/Ranger/Mage fixed types)              |
| Strength      | Combat         | **Removed**            | —                | Same as above                                                                     |
| Defence       | Combat         | **Removed**            | —                | Same as above                                                                     |
| Hitpoints     | Combat         | **Removed**            | —                | Hero maxHP defined in HeroData ScriptableObject                                   |
| Ranged        | Combat         | **Removed**            | —                | Ranger combat type covers this                                                    |
| Magic         | Combat         | **Removed**            | —                | Mage combat type covers this                                                      |
| Prayer        | Combat         | **Removed**            | —                | Bone/altar system too complex for mobile; covered by Herbalism potions            |

### Net result

| OSRS skills | Kept (adapted) | Removed | New additions |
|-------------|----------------|---------|---------------|
| 23          | 9              | 12      | 1 (Alchemy)   |
| —           | —              | —       | **= 10 total** |

---

## 10. XP Curve Reference

The OSRS XP formula (already implemented in `SkillSystem.cs`):

```
total_xp(L) = floor( sum_{n=1}^{L-1} floor(n + 300 × 2^(n/7)) ) / 4
```

### Key level thresholds

| Level | Total XP Required | Notes                                |
|-------|-------------------|--------------------------------------|
| 1     | 0                 | Start                                |
| 2     | 83                |                                      |
| 10    | 1,154             |                                      |
| 20    | 4,470             |                                      |
| 30    | 13,363            |                                      |
| 40    | 37,224            | T3 gear unlocks                      |
| 50    | 101,333           | Breakthrough level 1 (IsBreakthrough)|
| 60    | 273,742           | T4 gear unlocks                      |
| 70    | 737,627           |                                      |
| 75    | 1,210,421         | Breakthrough level 2                 |
| 80    | 2,031,628         | T5 gear unlocks                      |
| 90    | 5,346,332         |                                      |
| 92    | 6,517,253         | ← XP halfway point to 99            |
| 99    | 13,034,431        | Mastery / Breakthrough level 3       |

### Why OSRS curve works for Evetero
- **Front-loaded progression** — early levels feel fast, giving new players quick wins
- **Mid-game meat** (40-80) is the longest slog — this is where the strategy game content lives
- **Level 92 = halfway** — creates an authentic "almost there" feeling that drives long-term retention
- **Breakthrough events at 50/75/99** — natural moments for push notifications and celebration UI
- The breakeven point (92) is a well-known OSRS community meme ("only halfway") — long-time OSRS players will recognise and enjoy this reference

---

*Document version: 1.0 | Evetero GDD version: 0.1 | Date: 2026-03-11*
