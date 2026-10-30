# Last Plate

A 2D cooking game made in Unity for a class project. Move the chef, catch the
ingredients shown on the order, and use the plate tilt to get rid of mistakes.

## Running the game

1. Open the project in Unity Hub using Unity 6.6 (6000.6.2f1).
2. Open `Assets/Scenes/Game.unity`.
3. Press Play, then Enter to start.

The scene contains the camera and a setup script. The setup script creates the
chef, kitchen, and UI when the game starts.

## Controls

- A/D or arrow keys: move.
- Space: tilt the plate and discard the newest ingredient.
- Enter: start or restart.
- Escape: pause or resume.
- M: turn background music on or off.

## How it works

Each order needs three ingredients in the shown order. Wrong ingredients take up
space on the plate, but they do not cost a life. Burnt food costs one life.
Holding Space avoids catches until the tilt meter runs out. Release it to recharge.

The round lasts two minutes, with three 40-second stages: Preparation, Dinner
Rush, and Kitchen Chaos. Ingredients get faster and burnt food becomes more
common. Golden ingredients and correct-catch combos give extra points. Each
recipe slot only gives catch points once per order.

Customers have a patience meter. When it runs out, a new order appears without
taking a life. Food already on the plate stays there. If it matches the new
recipe completely, the order is served right away.

The round ends when the timer reaches zero or all three lives are lost.

## Testing and building

Open **Window > General > Test Runner**, select **PlayMode**, and run the tests.
There are 15 tests for catching, scoring, tilt, orders, the UI, pause, restart, and timing.

For a desktop build, open **File > Build Profiles** and choose the target platform.
The Game scene is already in the build list.

## Project notes

- `Assets/Scripts/`: game code.
- `Assets/Resources/`: art and audio.
- `Assets/Tests/PlayMode/`: gameplay tests.
- `DESIGN_CHANGES.txt`: changes from the original one-page design.
- `PLAN.md`: remaining work.

## Credits

The kitchen background, chef poses, customer portraits, and ingredient images
were generated for this project using OpenAI image generation. They are in
`Assets/Resources/Art`. The older `character_ingredient_sheet.png` is kept as a
fallback. The plate and plain UI panels are drawn in code; no purchased art packs
are used.

Voice reactions are by [cicifyre](https://cicifyre.itch.io/), from the
[Free Voice Clips Pack - Bright Female](https://opengameart.org/content/voice-clip-packs-for-visual-novels-and-rpgs).
The background track is [Sherwood](https://opengameart.org/content/sherwood) by
Chris Murphy (zesona). Both packs use the
[CC0 1.0 license](https://creativecommons.org/publicdomain/zero/1.0/).

The source clips were renamed for the game:

```text
01-anime gasp.wav -> chef_damage_01.wav
02-anime gasp.wav -> chef_damage_02.wav
01-okay.wav       -> chef_golden_01.wav
02-okay.wav       -> chef_golden_02.wav
01-thank you.wav  -> chef_complete_01.wav
02-thank you.wav  -> chef_complete_02.wav
sherwood_0.ogg    -> peaceful_kitchen.ogg
```

Voices are imported as mono and play at their original speed and pitch. Each
reaction has two takes to reduce repetition. The music plays quietly and lowers
during speech. Other sound effects are generated in `AudioManager.cs`.
