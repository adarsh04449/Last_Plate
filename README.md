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

## Project notes

- `Assets/Scripts/`: game code.
- `Assets/Resources/`: art and audio.
- `Assets/Tests/PlayMode/`: gameplay tests.
- `DESIGN_CHANGES.txt`: changes from the original one-page design.
