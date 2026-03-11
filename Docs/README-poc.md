# Evetero — POC Scripts

**Version:** 0.1 (proof-of-concept)  
**Unity version target:** Unity 6  
**Namespace:** `Evetero`  
**Date written:** March 2026

These scripts demonstrate the core data-driven architecture for Evetero.
The theme throughout: **game logic lives in code; game content lives in data.**
Adding heroes, abilities, dialogue, or world tiles means creating ScriptableObject assets — never editing a `.cs` file.

---

## Files at a Glance

| File | Type | Purpose |
|---|---|---|
| `HeroData.cs` | ScriptableObject | Defines a hero (identity, stats, 4 ability slots) |
| `AbilityData.cs` | ScriptableObject | Defines one ability (formula, cooldown, VFX ref) |
| `AbilitySystem.cs` | MonoBehaviour | Executes abilities, tracks cooldowns, resolves formulas |
| `DialogueData.cs` | ScriptableObject | A sequence of dialogue lines (one per scene/convo) |
| `DialogueSystem.cs` | MonoBehaviour | Plays dialogue sequences line-by-line, drives UI |
| `WorldNodeData.cs` | ScriptableObject | Defines a world map tile (type, resource yield, visuals) |

---

## How They Connect

```
HeroData  ──────────────┐
  └─ AbilityData[4] ────┤
                        ▼
                  AbilitySystem
                  (reads data, executes effects,
                   evaluates damage formulas)

DialogueData ───────────▶  DialogueSystem
(lines[])                   (drives UI panel,
                             fires events when done)

WorldNodeData ──────────▶  WorldTile (your future MonoBehaviour)
(nodeType, yieldPerHour)    (reads asset, renders sprite,
                             accumulates resources)
```

---

## Script Details

### HeroData.cs

A `ScriptableObject` that fully describes a hero.

**Key fields:**
- `heroName`, `portrait`, `fullArt`, `lore` — identity & UI
- `combatType` — enum (Warrior, Ranger, Mage, Healer, Rogue, Paladin, Summoner, Warlock)
- `abilities[4]` — drag in 4 `AbilityData` assets
- `baseStats` — HP, attack, magic power, defense, magic defense, speed, crit chance

**To define all 8 heroes:** Create 8 assets, fill inspector fields, done.  
No subclassing. No `HeroMira.cs`. Just data.

---

### AbilityData.cs

A `ScriptableObject` that defines a single ability.

**Key fields:**
- `abilityType` — Attack / Heal / Buff / Debuff
- `targetType` — SingleEnemy / AllEnemies / SingleAlly / AllAllies / Self
- `damageFormula` — string like `"{matk} * 1.2 - {mdef}"` evaluated at runtime
- `cooldownTurns`, `manaCost` — resource costs
- `vfxPrefab`, `sfx` — visual/audio references

**Mira's 4 abilities (create these assets):**

| Slot | Name | Type | Formula |
|---|---|---|---|
| 0 | Frost Bolt | Attack | `{matk} * 1.2 - {mdef}` |
| 1 | Blizzard | Attack (AoE) | `{matk} * 0.9 - {mdef}` |
| 2 | Ice Shield | Buff | _(no formula — status effect)_ |
| 3 | Glacial Pulse | Debuff | `{matk} * 0.5 - {mdef}` |

---

### AbilitySystem.cs

A `MonoBehaviour` that reads from `HeroData` and executes abilities.

**API:**
```csharp
// Execute ability in slot 0 against an enemy:
AbilityResult result = abilitySystem.ExecuteBySlot(0, enemyController);

// Tick cooldowns at end of each turn:
abilitySystem.TickCooldowns();

// Evaluate a formula standalone:
int dmg = AbilitySystem.EvaluateFormula("{matk} * 1.5", caster, target);
```

**AbilityResult** contains `success`, `value` (damage/heal amount), `failReason`.

**How it handles new abilities:** It doesn't need to. `Execute()` reads `AbilityType` and routes accordingly. New ability = new asset.

---

### DialogueData.cs

A `ScriptableObject` holding an ordered array of `DialogueLine` structs.

**DialogueLine fields:**
- `speakerName` — nameplate text
- `speakerPortrait` — portrait sprite
- `lineText` — the line itself
- `autoAdvanceDelay` — 0 = wait for player tap; >0 = auto-advance after N seconds

**Mira's intro scene (3 lines):**

> **Narrator:** "From the frozen peaks of the Icereach Range, she descended..."  
> **Mira:** "You called for a mage. I'm here. Try not to waste my time."  
> **Commander:** "Charming. Welcome to Evetero, Mira."

One asset per scene. Swap the asset, swap the story.

---

### DialogueSystem.cs

A `MonoBehaviour` that drives the dialogue UI.

**API:**
```csharp
// Start a conversation:
dialogueSystem.Play(miraIntroDialogue);

// Listen for completion:
dialogueSystem.OnDialogueComplete += () => { /* unlock next zone */ };

// Player presses Next:
dialogueSystem.Next();

// Player presses Skip:
dialogueSystem.Skip();
```

**Wire in inspector:**
- `dialoguePanel` — root Canvas panel to show/hide
- `speakerNameText` — Text or TMP_Text
- `dialogueLineText` — Text or TMP_Text
- `speakerPortraitImage` — Image component
- `nextButton` / `skipButton` — optional; or call `Next()`/`Skip()` from input actions

---

### WorldNodeData.cs

A `ScriptableObject` defining a world map tile type.

**Key fields:**
- `nodeType` — ResourceNode / Town / Dungeon / AllianceBase
- `resourceType` — Wood / Gold / Stone / Food / Mana / Iron / None
- `yieldPerHour` — passive income rate
- `storageCapacity` — caps resource accumulation
- `recommendedLevel`, `difficulty` — shown in tile info UI
- `tileSprite`, `tileVFXPrefab` — visuals

**Three sample tiles:**

| Asset | Type | Resource | Yield/hr |
|---|---|---|---|
| `Node_TreeTile` | ResourceNode | Wood | 40 |
| `Node_BankTile` | ResourceNode | Gold | 100 |
| `Node_DungeonTile` | Dungeon | None | 0 |

**Helper method:**
```csharp
int accumulated = nodeData.GetAccumulatedYield(tile.lastCollectTime);
```

---

## Quick Setup

1. **Drop scripts into your Unity project** under `Assets/Scripts/Evetero/`
2. **Create ScriptableObject assets:**
   - `Create → Evetero → Hero Data` × 8
   - `Create → Evetero → Ability Data` × 32 (4 per hero)
   - `Create → Evetero → Dialogue Data` × however many scenes
   - `Create → Evetero → World Node Data` × however many tile types
3. **Fill inspector fields** per the comments in each script
4. **Attach MonoBehaviours:**
   - `AbilitySystem` → attach to each hero's prefab (or a CombatManager)
   - `DialogueSystem` → attach to a persistent UI canvas object
5. **Wire UI references** in the DialogueSystem inspector

---

## Next Steps

These POC scripts are a foundation. Here's the natural build order:

### Immediate
- [ ] **CombatManager** — turn-order loop, win/loss detection, calls `AbilitySystem.Execute()`
- [ ] **HeroController** (full version) — replace the stub in AbilitySystem with a real class that handles animations, status effects, and death
- [ ] **StatusEffectSystem** — buffs/debuffs need duration tracking; AbilitySystem already routes to it

### Short-term
- [ ] **WorldTile MonoBehaviour** — reads `WorldNodeData`, renders sprite, tracks `lastCollectTime`, shows accumulation UI
- [ ] **WorldMapController** — manages the grid of `WorldTile` instances, handles player tap → tile info panel
- [ ] **ResourceManager** — singleton tracking player's current Wood/Gold/Stone/Food/Mana

### Medium-term
- [ ] **HeroRoster UI** — reads all `HeroData` assets (via `Resources.LoadAll` or Addressables), displays hero cards
- [ ] **DialogueSystem** → **CutsceneManager** bridge — trigger dialogue from combat events, zone entry, quest completion
- [ ] **AbilityData → Addressables** — swap `vfxPrefab` from direct reference to Addressable key for memory-efficient loading
- [ ] **Save/Load system** — serialize cooldown state, resource accumulation timestamps, hero progression

### Architecture notes
- Keep all `HeroData`, `AbilityData`, `DialogueData`, and `WorldNodeData` assets under `Resources/Data/` or in an Addressables group — never hardcode paths
- The formula parser (`System.Data.DataTable.Compute`) handles MVP. For 100+ abilities, consider a lightweight expression library
- `DialogueSystem` is intentionally UI-system-agnostic (UGUI stubs). Swap `Text` → `TMP_Text` in a single pass when you move to TextMeshPro

---

*These scripts were written as a clean POC — minimal, well-commented, drop-in ready.*  
*They compile standalone (no external dependencies beyond Unity 6 core).*
