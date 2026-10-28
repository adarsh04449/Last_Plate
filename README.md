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
can be recovered from by tilting instead of immediately losing a life. Tilting consumes energy
and recharges when released. Golden ingredients award bonus points.
The main gameplay tuning values are exposed on `GameManager`, `SpawnManager`, and `PlateController`.

See `DESIGN_CHANGES.txt` for the required comparison with the original one-page design.
