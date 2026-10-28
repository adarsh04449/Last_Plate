# Last Plate

An original 60-second Unity 6 kitchen-rush arcade game. Catch recipe ingredients in sequence,
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

The plate visibly carries up to three ingredients. Extra weight slows the chef, and wrong food
can be recovered from by tilting instead of losing a life. Burnt food is the only item that removes
a life for now. Tilting consumes energy and recharges when released. One clear customer ticket is
active at a time; later difficulty phases change the customer type and pressure rather than adding
simultaneous orders. Golden ingredients award bonus points.
The HUD uses large recipe icons to highlight the next required ingredient. Caught ingredients
remain visible as a physical stack above the chef's plate instead of being repeated in another HUD.
Damage and completed orders have separate visual flashes and sounds.
Customer types now have distinct cartoon portraits, and the chef visibly reacts to burnt food and
completed orders. Short spoken reactions ("Oh no!", "Nice catch!", and "Order up!") are used only
for important moments, with cooldowns that prevent voice lines from becoming repetitive.
Each falling ingredient now uses an individual high-resolution transparent sprite, and the plate
uses a four-times-larger texture with a defined rim for cleaner scaling and tilting.
The main gameplay tuning values are exposed on `GameManager`, `SpawnManager`, and `PlateController`.

See `DESIGN_CHANGES.txt` for the required comparison with the original one-page design.
See `PLAN.md` for the current development roadmap and the features that are intentionally out of scope.
