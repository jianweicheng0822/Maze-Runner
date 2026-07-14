# Stay in the Shadows, Keep Your Light

## Concept

A 2D dark maze survival game. Dodge traps, creatures, and obstacles to escape each floor of the maze alive.

## Story

Bobo is a small creature born with an innate glow — a living light source in a world of darkness. One day, Bobo falls into an ancient maze deep underground. The maze is alive with creatures that thrive in the dark. They sense Bobo's light and hunger to absorb it, to extinguish the only brightness in their world.

Bobo must navigate the twisting corridors, avoid deadly traps, and escape floor by floor — all while his light draws danger closer. The deeper the maze goes, the darker it gets, and the harder it becomes to survive.

**Tagline:** *Stay in the shadows, keep your light.*

## Inspiration

A dark basement. One glowing screen. A world opens up.

## Core Loop

The tension cycle that drives gameplay:

1. Moving slowly keeps Bobo hidden but his light **dims** — darkness closes in, fear builds
2. Sprinting makes Bobo's light **flare up** — visibility increases but Glare Spirits speed up, drawn to the brightness
3. The player constantly balances stealth vs speed, darkness vs exposure
4. A fading trail of light marks where Bobo has been — traces in the void

## Core Mechanics

- **Fog of War**: Bobo's glow illuminates a small area (point light) and a directional flashlight cone. Everything beyond is darkness.
- **Dynamic Light**: Bobo's light radius responds to movement — dims when still, flares when sprinting. This is the central risk/reward mechanic.
- **Sprint** (Left Shift): Move faster but glow brighter, attracting danger.
- **Light Trail**: Fading glow footprints show where Bobo has been. Fades over ~10 seconds.
- **Maze Generation**: Procedural DFS-generated mazes that grow larger and more complex with each level.
- **Health System**: Bobo has limited health. Death resets progress to level 1.
- **Progressive Difficulty**: Each level increases maze size, number of traps/creatures, and reduces Bobo's base light radius.
- **Map** (M key): View explored areas. Only tiles Bobo has visited are revealed.

## Creatures & Traps

### 1. Glare Spirits (探照灯精灵) — "Your core enemy" [IMPLEMENTED]

- **Look**: A dark eyeball with a dim cone-shaped gaze. Invisible in darkness — only revealed by Bobo's light
- **Behavior**: Patrol corridors autonomously (walk forward, turn at intersections, occasionally stop and stare). They are blind creatures that sense light
- **Contact**: When Bobo bumps into a spirit (can't see it in the dark), the spirit latches on and starts **chasing** through the maze, draining Bobo's light/HP continuously
- **Chase**: Spirit uses BFS pathfinding to follow Bobo. Faster than walking speed but slower than sprint. Drains light while close
- **Escape**: Bobo must outrun the spirit. If distance > 8 tiles, spirit gives up and resumes patrol
- **Light = Life**: HP represents light energy. As HP drops, Bobo's visible radius shrinks. HP = 0 → light goes out → death

### 2. Shadow Tendrils (阴影触手) — "Static zone hazard" [PLANNED]

- **Look**: Writhing dark ink-like patches on the floor
- **Behavior**: "Light devourers" — fixed floor zones that drain Bobo's light energy on contact
- **Gameplay**: Forces player to choose between sprinting through (less drain time) or finding a detour. Good for dead ends and chokepoints. Drain effect = shrink light radius temporarily

### 3. Echo Mimics (回声捕手) — "Anti-sprint punishment" [PLANNED]

- **Look**: Wall tiles that subtly pulse/throb, disguised as normal bricks
- **Behavior**: Sensitive to sound. Triggered when Bobo sprints nearby
- **Gameplay**: When triggered, they lash out a tendril to block the path. Creates hesitation: "The faster I run, the more traps I trigger." Pairs with Glare Spirits for double-bind situations

## Design Principles

- No safe strategy — every choice has tradeoffs
- The maze should feel alive and hostile
- Sound and light are both tools and threats
- Difficulty comes from decision-making, not unfair mechanics

## Future Plans

- Player classes/professions with unique abilities (e.g., healing skills)
- UI art and visual polish (Google Flow / Gemini generated)
- Level cap or endless mode decision
- Sound design: negative pressure SFX for Shadow Tendrils, heartbeat when low light

## Art Direction

- Dark, atmospheric tone
- Warm golden glow for Bobo contrasting against cold, dark maze
- Glare Spirits: orange/amber eye with semi-transparent beam cone
- Shadow Tendrils: dark writhing ink patches
- Echo Mimics: pulsing wall tiles, subtle visual tells
- Procedural visuals (code-generated sprites, no external assets yet)
