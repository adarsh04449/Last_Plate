# Last Plate — New Development Plan

## Direction

Last Plate will remain a fast, illustrated, single-screen catching game. The falling-ingredient
loop is the identity of the game and should not be replaced with separate chopping, cooking, or
plating pages.

The goal is to make that loop deeper and more exciting while keeping the player’s job obvious:

```text
Read one order → Catch ingredients in sequence → Tilt to manage mistakes → Complete the order
```

The game should feel like a chef surviving a busy kitchen, not like the player is learning several
unrelated minigames.

## Current Foundation

The restored prototype already contains:

- Illustrated kitchen background and cartoon chef/ingredient artwork.
- A single active customer order with named recipes.
- Three-ingredient sequence checking.
- Chef movement using A/D, Left/Right, or arrows.
- Three-slot visible plate inventory.
- Rechargeable plate tilt that discards the newest item and avoids catches.
- Carry weight that slows the chef.
- Burnt food hazards and three lives.
- Golden ingredients, combo scoring, and score multipliers.
- A two-minute round with Preparation, Dinner Rush, and Kitchen Chaos phases, lasting 40 seconds each.
- Pause, restart, results rank, and synthesized sound effects.

## Design Rules

These rules prevent the game from becoming confusing again:

1. Keep one active order visible at a time.
2. Keep the player on one gameplay screen during a round.
3. Keep the basic controls limited to movement, tilt, Enter, and Escape.
4. Every new mechanic must be understandable from the HUD in a few seconds.
5. Do not add a second required minigame or station until the catching loop is polished.
6. Mistakes should be recoverable when possible; avoid stacking several punishments at once.
7. Add one feature, test it, then move to the next feature.
8. Burnt food is the only hazard that removes a life for now. Wrong ingredients remain
   recoverable by tilting, and a customer leaving replaces the order without removing a life.

## Feature Roadmap

### Phase 1 — Make the Existing Game Clear

Objective: make the restored game immediately understandable in a video or first playthrough.

- Replace recipe text with three large ingredient icons.
- Highlight the next required ingredient with a bright outline or arrow.
- Show caught ingredients directly on the chef's plate; do not duplicate them in a bottom HUD.
- Add a small “TILT READY” prompt when Space can discard an item.
- Add a clear visual flash when a life is lost.
- Add a short instruction line under the order card:
  `Catch the highlighted ingredient next`.
- Make the title screen explain the complete control scheme in one readable block.

Acceptance test: a new player can explain what to catch, what Space does, and how to complete an
order without reading external instructions.

### Phase 2 — Add Personality to Orders

Objective: make customers and recipes feel different without adding new controls.

- Add 6–8 named recipes using the existing four food types.
- Add customer portraits or silhouettes to the order card.
- Add order modifiers such as:
  - Golden Finish: catch one golden ingredient for bonus points.
  - No Waste: finish without discarding an ingredient.
  - Perfect Sequence: no wrong catches.
  - Rush Order: less patience but a larger reward.
- Show the active modifier directly on the order card.
- Give each customer a short reaction line after completion or failure.

Acceptance test: every order has one readable identity and the player can understand its bonus
before starting it.

### Phase 3 — Improve the Catching Challenge

Objective: create variety while preserving the same core control scheme.

- Add moving delivery lanes so ingredients do not all fall straight down.
- Add occasional ingredient clusters that reward positioning.
- Add a slow-moving kitchen tool or tray that temporarily blocks one small area.
- Add a visible warning before any special hazard appears.
- Keep target-aware spawning so every order remains achievable.
- Make Kitchen Chaos increase patterns and hazard frequency gradually, not randomly unfairly.

Acceptance test: the challenge feels different between phases, but the player always knows why an
ingredient was missed.

### Phase 4 — Make Tilt More Interesting

Objective: turn tilt into a deliberate decision without making it mandatory every few seconds.

- Add a brief “perfect tilt” bonus when a wrong ingredient passes safely.
- Add a small cooldown after tilt so holding Space forever is not optimal.
- Add a visual arc showing the plate’s discard direction.
- Add one rare hazard that can only be avoided by tilting.
- Keep tilt energy visible and generous enough for beginners.

Acceptance test: Space feels useful and responsive, but the player still spends most of the round
catching ingredients normally.

### Phase 5 — Audio and Visual Polish

Objective: make the game feel finished in the final recording.

- Add distinct catch, wrong catch, tilt, golden, order-complete, life-lost, and phase-change sounds.
- Use brief, conversational human reactions for burnt food, special catches, and completed orders.
- Add chef idle, leaning, celebration, and worried animations.
- Add ingredient trails or small spark effects for golden food.
- Add a plate shake on wrong catches and a larger celebration on completed orders.
- Keep a quiet, peaceful background loop below gameplay sounds; fade it during speech and at
  round end. Let players toggle music with M and pause all audio with Escape.

Acceptance test: every important player action has readable visual and audio feedback without sound
effects becoming repetitive or overwhelming.

Current progress: customer portraits, chef ready/worried/celebration poses, distinct action sounds,
and human-recorded disappointed, positive, and victory reactions are implemented. The original
text-to-speech and intense adventurer clips have been replaced with conversational CC0 recordings
by cicifyre. Each event has two takes, and effects become quieter during speech. Reactions finish
before another plays. A quiet "Sherwood" loop by Chris Murphy accompanies gameplay with fades,
speech ducking, and an M-key music toggle. Escape pauses all audio. All five falling-food
assets now use individual high-resolution transparent sprites, and the plate texture has been
upgraded for clean scaling.

### Phase 6 — Replayability and Submission Polish

Objective: make the game easy to grade and easy to replay.

- Add a local high score and best completed-order count.
- Add a short tutorial overlay on the first launch.
- Add a clean results screen showing score, orders, best combo, and special bonuses.
- Add a “What changed from the one-pager” section to `DESIGN_CHANGES.txt`.
- Test keyboard controls, restart, pause, timer end, lives reaching zero, and every difficulty phase.
- Record a video showing the title screen, normal catching, tilt recovery, Kitchen Chaos, and results.

## Testing Checklist

Automated coverage: 14 committed PlayMode tests under `Assets/Tests/PlayMode` exercise the core
gameplay rules. The audit fixes also replaced the hidden extra customer cards with one ticket,
fixed the tilt meter, require Space release for recharge, prevent repeated slot rewards, and
serve matching retained plates when a replacement order arrives. The game scene is included in
the saved build configuration.

- Chef stays inside the screen.
- A/D and arrow keys both work.
- Correct ingredients advance only the intended order slot.
- Wrong ingredients do not create duplicate penalties.
- Tilt prevents catches while active and discards only the intended item.
- Holding Space at zero energy does not reactivate tilt; releasing Space recharges it.
- Re-catching a scored recipe slot does not award more points or combo credit on the same ticket.
- A replacement order matching the full retained plate serves immediately.
- Plate-full feedback is clear.
- Wrong ingredients do not remove lives; they occupy plate space and can be discarded with tilt.
- Burnt food removes exactly one life and cannot remove more than one life per item.
- A customer timing out replaces the order without removing a life.
- Golden ingredients and combo multipliers award the correct score.
- New orders always contain three ingredients.
- The timer ends the round correctly.
- Losing the third life ends the round immediately.
- Escape pauses and resumes without changing the timer unexpectedly.
- Enter starts and restarts correctly.
- Restart resets lives, score, timer, plate contents, combo, and order progress.
- The complete game can be understood from the video without external explanation.

## Explicitly Out of Scope for Now

- Separate collection, chopping, cooking, and serving screens.
- Multiple simultaneous customers.
- A restaurant-management economy.
- Complex inventory or upgrade menus.
- Online features.
- A global redesign that removes the illustrated catching gameplay.
