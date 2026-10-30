# Last Plate

An original two-minute Unity 6 kitchen-rush arcade game. Catch recipe ingredients in sequence,
manage limited plate-tilt energy, build score multipliers, and survive escalating kitchen chaos.

## Run it

1. Open this folder in Unity Hub with the Unity 6 2D template/version.
2. Open `Assets/Scenes/Game.unity`.
3. Press Play.

The scene builds its illustrated kitchen, chef, ingredients, gameplay systems, and UI at runtime.

## Controls

- A/D or Left/Right: move
- Tap/Hold Space: discard the newest plate item, then keep the plate tilted to avoid catches
- Enter: start/restart
- Escape: pause/resume
- M: background music on/off (saved between sessions)

The plate visibly carries up to three ingredients. Extra weight slows the chef, and wrong food
can be recovered from by tilting instead of losing a life. Burnt food is the only item that removes
a life for now. Tilting consumes energy and recharges when released. One clear customer ticket is
active at a time; later difficulty phases change the customer type and pressure rather than adding
simultaneous orders. Golden ingredients award bonus points.
The HUD uses large recipe icons to highlight the next required ingredient. Caught ingredients
remain visible as a physical stack above the chef's plate instead of being repeated in another HUD.
Damage and completed orders have separate visual flashes and sounds.
Customer types now have distinct cartoon portraits, and the chef visibly reacts to burnt food and
completed orders. Conversational human recordings by cicifyre provide brief gasps, "Okay", and
"Thank you" reactions. Each event has two takes, played at their original pitch and speed, with
cooldowns and quieter effects during speech. A quiet, relaxing background loop ("Sherwood" by
Chris Murphy) fades in during play, softens further during reactions, and fades out at round end.
Escape pauses all audio along with gameplay. See `AUDIO_CREDITS.txt` for sources and CC0 licenses.
Each falling ingredient now uses an individual high-resolution transparent sprite, and the plate
uses a four-times-larger texture with a defined rim for cleaner scaling and tilting.
The order ticket uses a fixed-color remaining-time meter that visibly shrinks as customer patience
runs out.
The main gameplay tuning values are exposed on `GameManager`, `SpawnManager`, and `PlateController`.
The round lasts 120 seconds, displayed as minutes and seconds. Preparation runs for the first
40 seconds, Dinner Rush for the next 40, and Kitchen Chaos for the final 40.

Holding Space after tilt energy runs out leaves the plate flat; release Space to recharge before
tilting again. The blue meter shrinks with remaining energy. Each recipe slot awards catch points
only once per ticket, so discarding and re-catching it cannot farm points or combo credit. If a new
customer's recipe matches a full plate left from an expired order, that meal is served immediately.

## Test and build

- Open **Window > General > Test Runner**, select **PlayMode**, and run all tests. The 14 gameplay
  tests cover ordered catches, wrong food, duplicate damage, tilt exhaustion/recharge, meter width,
  pause/resume, once-per-slot scoring, replacement orders, restart, difficulty, and round ending.
- `Assets/Scenes/Game.unity` is included in the saved build scene list. Use **File > Build Profiles**
  to select the desktop target and build the game.

The runtime assembly definition allows the tests to reference game code; the test assembly is
excluded from normal player builds. No additional testing packages are required.

See `DESIGN_CHANGES.txt` for the required comparison with the original one-page design.
See `PLAN.md` for the current development roadmap and the features that are intentionally out of scope.
