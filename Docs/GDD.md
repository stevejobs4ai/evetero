# Evetero — Game Design Document
> **Status:** Early Draft (v0.1) — Everything here is a starting point. Names, lore, mechanics, and structure are all subject to change. When making significant changes, bump the version number and note what changed.

---

## Table of Contents
1. [Game Overview](#game-overview)
2. [World & Lore](#world--lore)
3. [Heroes](#heroes)
4. [Core Gameplay Loop](#core-gameplay-loop)
5. [Alliance System](#alliance-system)
6. [Monetization](#monetization)
7. [Tech Stack](#tech-stack)
8. [Art & Asset Direction](#art--asset-direction)

---

## Game Overview

**Name:** Evetero  
**Genre:** Mobile strategy / city builder / RPG hybrid  
**Platforms:** iOS, Android  
**Inspiration:** Last War: Survival (mechanics), League of Legends (hero depth + IP), Runescape (world feel), D&D (lore richness)  
**Core promise:** The deepest hero roster in mobile strategy — characters so rich that players identify with them, buy merchandise, and dress up as them.

**Differentiators from Last War:**
- Fantasy theme (not zombies/military)
- Hero playstyles that meaningfully change how you play
- Rich world lore with actual story
- Heavy cosmetics (Fortnite-level shop)
- Skinnable world themes
- AI-generated personal story arcs (premium feature)
- AI-generated art, voice, music pipeline

---

## World & Lore

> ⚠️ **Draft v0.1** — God names (Vael/Morath), the number of gods, the nature of the curse, and the overall myth are all placeholders. Reece may decide to change fundamentally (e.g. more gods, different conflict, different cause of sleep). Update this section and bump version when lore is revised.

### The World
The world of Evetero is waking up.

For a thousand years it has been asleep — not dead, but frozen. Magic faded to a flicker. Ancient cities sank beneath soil and sea. Great creatures retreated into myth. People adapted, built smaller lives, and forgot what the world used to be.

Then, about a generation ago, it started waking up. Nobody knows why — or rather, nobody *living* knows why. A buried temple surfaced overnight. A river ran backwards for a week. A child was born speaking a language no one had heard in a thousand years.

The old magic is returning. With it: everything that was lost. Some of it beautiful. Some of it dangerous.

### The Sundering (Creation Myth)
Two gods — twins — built the world together.

- **Vael** *(god of life, magic, growth)* shaped the land, creatures, and magic.
- **Morath** *(god of time, order, death)* shaped time, memory, and mortality.

For ages they were balanced. Then Morath grew fearful. He believed Vael's creations — magic, life, chaos, growth — would eventually consume everything. Order would be lost.

So Morath cursed the world: *sleep, until you are ready to be orderly.*

Vael fought back. The curse landed anyway. The world didn't die — it froze.

Morath believed he was being merciful.

Now, a thousand years later, the curse is cracking. Vael is waking the world back up. But Morath has been watching, waiting, building order in the dark — and he's not done.

### The Central Conflict
**Is a messy, waking, magical world worth saving — or should it be put back to sleep?**

Morath isn't evil. He genuinely believes order is preservation. His followers aren't villains — they're afraid of what an uncontrolled world becomes.

This moral ambiguity is intentional. The best antagonists have a point.

### Factions *(placeholder names)*
- **The Wakers** — fight for the waking world, Vael-aligned
- **The Order** — believe the world should return to sleep, Morath-aligned
- **The Unbound** — don't care about the gods; just want power in the chaos

---

## Heroes

> ⚠️ **Draft — hero roster TBD.** Starting with 5 launch heroes. See `/docs/heroes/` for individual hero files.

### Design Philosophy
Heroes are the heart of Evetero. Players should:
- **Identify** with a hero personally
- **Want to own** them (cosmetics, exclusives)
- **Follow** their story arc
- **Cosplay** them at events

Each hero needs:
- Distinctive silhouette (recognizable at a glance)
- Meaningful playstyle difference (not just stat boosts)
- Personal backstory connected to the world lore
- A flaw or wound that drives their arc
- Voice lines with personality (AI-generated)
- Multiple cosmetic skin options

---

## Core Gameplay Loop

*(To be expanded — see GitHub issues for detailed breakdowns)*

- Build & upgrade your city while away (resources cap after X hours)
- Return to collect, train troops, research, expand territory
- Recruit and upgrade heroes
- Join an alliance — cooperate, donate, rally
- Participate in rotating events (every 3-4 hours)
- Arms race: limited-time challenges rotating through game systems
- Treasure dig, radar, convoys — reasons to stay active

---

## Alliance System

*(To be expanded)*

- Alliance chat + activity feed
- Alliance tech donations
- Alliance rankings (internal + world)
- Alliance VS: daily rotating objectives (biggest engagement/spend driver)
- Alliance leader tools: automated strategy messages for events
- Zombie Metropolis equivalent: bot alliance for world PvP events

---

## Monetization

*(To be expanded)*

**Entry funnel:**
- $0.99 starter hero pack
- $1.99 second build queue
- $4.99 third queue
- $19.99 fourth queue

**Ongoing:**
- Battle pass
- Daily $5 deal
- Limited-time packs
- Cosmetics shop (rotating, Fortnite-style)

**Hooks:**
- Earn small amounts of premium currency free → get players hooked
- Every purchase unlocks more value than paid (visible ROI)
- Gem rewards for: email signup, Discord join, app review

---

## Tech Stack

- **Engine:** Unity (iOS + Android)
- **AI Coding:** Cursor + Claude Code
- **Backend:** TBD
- **CI/CD:** GitHub Actions
- **Repo:** https://github.com/stevejobs4ai/evetero

---

## Art & Asset Direction

- **Style:** Stylized fantasy illustration (not realistic, not chibi)
- **Pipeline:** AI-generated concept art → 3D model generation → rigging
- **Voice:** AI-generated per hero (ElevenLabs or equivalent)
- **Music:** AI-generated (Suno/Udio or equivalent)
- **Reference:** League of Legends champion art direction

---

*Last updated: 2026-03-10 | Version: 0.1*
