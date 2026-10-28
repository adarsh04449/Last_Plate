using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scores each required chop using one readable timing bar.</summary>
public class PreparationStage : MonoBehaviour
{
    public RectTransform marker;
    public Text instructionText;

    private readonly List<int> chopIndices = new List<int>();
    private readonly List<int> scores = new List<int>();
    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private int current;
    private bool active;
    private float phase;
    private int beginFrame;

    public void Begin(MealRecipe nextRecipe, KitchenFlowManager manager)
    {
        recipe = nextRecipe;
        flow = manager;
        chopIndices.Clear();
        scores.Clear();
        for (int i = 0; i < recipe.chop.Length; i++) if (recipe.chop[i]) chopIndices.Add(i);
        current = 0;
        phase = 0f;
        beginFrame = Time.frameCount;
        active = true;
        RefreshInstruction();
    }

    private void Update()
    {
        if (!active || Time.timeScale == 0f || Time.frameCount <= beginFrame) return;
        phase += Time.deltaTime * 1.25f;
        float normalized = Mathf.PingPong(phase, 1f);
        marker.anchoredPosition = new Vector2(Mathf.Lerp(-250f, 250f, normalized), 0f);

        if (!Input.GetKeyDown(KeyCode.Space)) return;
        float distance = Mathf.Abs(marker.anchoredPosition.x);
        int score = distance <= 62f ? 100 : distance <= 145f ? 70 : 40;
        scores.Add(score);
        FindAnyObjectByType<AudioManager>()?.PlayChop();
        current++;
        phase = 0f;

        if (current >= chopIndices.Count)
        {
            active = false;
            int total = 0;
            foreach (int value in scores) total += value;
            flow.PreparationComplete(scores.Count == 0 ? 100 : Mathf.RoundToInt(total / (float)scores.Count));
        }
        else RefreshInstruction();
    }

    private void RefreshInstruction()
    {
        IngredientType ingredient = recipe.ingredients[chopIndices[current]];
        instructionText.text = "PREPARATION COUNTER\n\nCHOP " + KitchenFlowManager.IngredientName(ingredient) +
            "   (" + (current + 1) + "/" + chopIndices.Count + ")\n\nPress SPACE when the knife marker is inside the green zone";
    }
}
