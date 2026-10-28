# Last Plate — Multi-Stage Cooking Game Plan

## Direction

Last Plate is no longer a 60-second falling-object survival game. Each customer order moves
through a short sequence of clear, full-screen kitchen pages. The player can read and complete
one task at a time without managing every mechanic simultaneously.

The structure is inspired by the readable station flow used by cooking games such as Papa's
Burgeria, Overcooked, Good Pizza Great Pizza, and Cook, Serve, Delicious, while retaining Last
Plate's own ingredient-collection minigame and recipes.

## Core Loop

```text
Customer Order → Collect → Prepare → Cook → Plate and Serve → Rating
```

There is no global round timer, life counter, burnt-food rain, plate-tilt meter, or customer queue.
A session contains three customers, and the player may take the time needed to understand each
stage.

## Pages

### 1. Customer Order

- Show one customer and one named recipe.
- Display the three required ingredients and cooking steps.
- The timer does not run while reading.
- Enter accepts the order.

### 2. Ingredient Collection

- A/D or Left/Right moves the basket.
- Ingredients fall into the collection area.
- Catch the three ingredients shown on the order card.
- Required ingredients may be collected in any order.
- Wrong catches reduce ingredient quality but do not remove a life.
- The page ends when all required ingredients have been collected.

### 3. Preparation

- Required vegetables are prepared one at a time.
- A marker moves across a timing bar.
- Each prepared ingredient requires three visible cuts.
- Space chops when the marker reaches a green cut mark that changes position.
- Accuracy becomes the preparation-quality score.

### 4. Cooking

- Recipes that require heat use a raw-to-burnt cooking meter.
- A/D or Left/Right controls the heat level while cooking progress advances.
- Keeping the flame marker inside the green zone gives the best cooking score.
- Recipes such as salad skip heat by pressing Enter.

### 5. Plate and Serve

- Place the three prepared ingredients onto the plate one at a time with Space.
- Enter serves only after plating is complete.
- A serving sound and customer reaction provide feedback.

### 6. Rating

- Award one to three stars from collection, preparation, and cooking quality.
- Show separate quality percentages so mistakes are understandable.
- Enter continues to the next customer.
- After three customers, show the session score and total stars.

## Initial Recipes

1. Garden Salad — tomato, lettuce, mushroom; chop only; no cooking.
2. Mushroom Melt — mushroom, cheese, tomato; chop and cook.
3. Veggie Skillet — tomato, mushroom, cheese; chop and cook with a faster meter.

## Presentation

- Keep one Unity scene for reliability.
- Use separate full-screen panels and distinct color/background treatments for every page.
- Reuse the existing chef and ingredient artwork.
- Use short sound cues for collecting, mistakes, chopping, cooking, serving, and results.
- Add recorded human reactions later: “Perfect!”, “Delicious!”, “Not bad,” and “Oh no!”

## Implementation Order

1. Page-based state machine and persistent meal data.
2. Customer order page.
3. Collection minigame adapted from the prototype.
4. Chopping timing minigame.
5. Cooking timing minigame.
6. Serving and star-rating pages.
7. Three-order session progression.
8. Final page-specific artwork, animation, and human voice clips.

## Acceptance Criteria

- The player can complete three customer orders without a global timer.
- Only one gameplay task is active on screen at a time.
- Each page has distinct instructions and presentation.
- Wrong collection catches lower quality without ending the order.
- Chopping and cooking accuracy affect the final rating.
- Salad skips cooking cleanly.
- Enter advances order, serving, rating, and session screens correctly.
- Restarting a session resets all scores and collected ingredients.
