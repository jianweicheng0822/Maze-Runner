# GALAXY RUNNERS - Game Design Document

## 1. Core Identity

- **Title:** GALAXY RUNNERS
- **Target Platform:** Steam
- **Genre:** Party / Team-building / Couch-co-op multiplayer
- **References:** Fall Guys, Pico Park, Lethal Company
- **Current Phase:** Main Menu & Core UI (early development)
- **Ultimate Goal:** A highly recognizable, chaotic, humorous Steam hit — perfect for online co-op sessions and team-building events.

## 2. Art Style & Visual Direction

- **Player Character:** Space astronauts in spacesuits.
  - Suit color must support **random generation / customization**.
- **Environment:** Low-poly or cartoon-style space mazes with a vibrant, sci-fi aesthetic.
- **UI Tone:** Clean, modern, party-game quality. No placeholder or default-asset look.
- **No:** "Beta Sign-up" buttons, subtitles like "Grab. Run. Score!", profile systems, or Steam invite UIs on the main menu.

## 3. Core Gameplay: Material Scramble

Players race through mazes to **grab, carry, and deliver space materials** for points.

### Material Tiers

| Tier | Examples | Visual FX | Carry Method | Speed Penalty | Score |
|------|----------|-----------|-------------|---------------|-------|
| **Small** | Alien energy cells, crystals, glowing bolts | Faint blue/green particle glow | One-handed grab | None | Low |
| **Medium** | Ancient alien discs, rotating gravity rings, alien flora/tech | Strong purple/yellow glow | Two-handed carry | -20% speed, no jumping | Medium |
| **Large** | Giant alien relic cannons, massive energy cores with glowing runes | Intense glow, visible map-wide | Multi-player cooperative lift strongly encouraged | -80% speed (solo) | Very High (comeback potential) |

### Key Design Principles

- Materials must **never** be boring default shapes (spheres, cubes).
- Large materials create emergent chaos: all players see them on the map, triggering interception and sabotage.
- Multi-player cooperative carrying for large items is a core social mechanic.

## 4. Main Menu Design

### Title
Large, textured "GALAXY RUNNERS" logo (3D or stylized 2D).

### Buttons
1. **Single Player** — Solo practice mode
2. **Multiplayer (Coming Soon)** — Grayed out / disabled state
3. **Settings** — Game settings
4. **Quit** — Exit game

### Background
A showcase of in-game elements:
- Maze structures
- Randomly colored astronauts
- Scattered small/medium/large space materials

## 5. Development Roadmap

- [x] Project setup & initial scenes
- [x] Main menu scene with cartoon UI and 3D background
- [x] Core gameplay systems (8 phases)
- [ ] Refine main menu to match GALAXY RUNNERS branding
- [ ] Astronaut character with customizable suit colors
- [ ] Material system (small / medium / large tiers)
- [ ] Maze generation or level design
- [ ] Single-player practice mode
- [ ] Multiplayer networking (future)

## 6. Technical Notes

- **Engine:** Unity
- **UI System:** UGUI (or UI Toolkit — TBD)
- **Code Language:** C#
- **Architecture:** Keep code extensible for future multiplayer and material-scramble integration.
