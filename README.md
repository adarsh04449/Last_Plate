# Last Plate

Last Plate is a multi-stage 2D cooking game made with Unity 6. One customer order moves through
separate collection, preparation, cooking, plating, and rating pages. There is no global timer;
the goal is to make each meal accurately and earn up to three stars.

## Run it

1. Open this folder in Unity Hub using Unity 6.
2. Open `Assets/Scenes/Game.unity`.
3. Press Play.
4. Press Enter on the title screen.

The scene creates all gameplay pages and systems at runtime, so no Inspector setup is required.

## Controls

- Enter: start, accept an order, serve, or continue
- A/D or Left/Right: move the collection basket
- Space: chop and place ingredients onto the plate
- A/D or Left/Right during cooking: control the stove heat
- Escape: pause/resume

## Game flow

```text
Customer Order → Collect Ingredients → Chop → Cook/Mix → Serve → Star Rating
```

A session contains three recipes: Garden Salad, Mushroom Melt, and Veggie Skillet. Wrong
collection catches reduce quality rather than taking a life. Preparation requires three accurate
cuts per chopped ingredient. During cooking, the player controls the heat and tries to keep it in
the green zone until the meal is done. Ingredients are then placed onto the final plate one at a
time before serving. The final rating explains the quality earned in every stage.

See `PLAN.md` for the current design and `DESIGN_CHANGES.txt` for differences from the original
one-page proposal.
