using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scores each required chop using one readable timing bar.</summary>
public class PreparationStage : MonoBehaviour
{
    public RectTransform marker;
    public RectTransform perfectZone;
    public Text instructionText;
    public Text cutProgressText;

    private readonly List<int> chopIndices = new List<int>();
    private readonly List<int> scores = new List<int>();
    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private int current;
    private int cut;
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
        cut = 0;
        phase = 0f;
        beginFrame = Time.frameCount;
        active = true;
        MovePerfectZone();
        RefreshInstruction();
    }

    private void Update()
    {
        if (!active || Time.timeScale == 0f || Time.frameCount <= beginFrame) return;
        phase += Time.deltaTime * 1.25f;
        float normalized = Mathf.PingPong(phase, 1f);
        marker.anchoredPosition = new Vector2(Mathf.Lerp(-250f, 250f, normalized), 0f);

        if (!Input.GetKeyDown(KeyCode.Space)) return;
        float distance = Mathf.Abs(marker.anchoredPosition.x - perfectZone.anchoredPosition.x);
        int score = distance <= 55f ? 100 : distance <= 125f ? 70 : 40;
        scores.Add(score);
        FindAnyObjectByType<AudioManager>()?.PlayChop();
        cut++;
        phase = 0f;

        if (cut >= 3)
        {
            cut = 0;
            current++;
            if (current >= chopIndices.Count)
            {
                active = false;
                int total = 0;
                foreach (int value in scores) total += value;
                flow.PreparationComplete(scores.Count == 0 ? 100 : Mathf.RoundToInt(total / (float)scores.Count));
                return;
            }
        }
        MovePerfectZone();
        RefreshInstruction();
    }

    private void RefreshInstruction()
    {
        IngredientType ingredient = recipe.ingredients[chopIndices[current]];
        instructionText.text = "PREPARATION COUNTER\n\nCHOP " + KitchenFlowManager.IngredientName(ingredient) +
            "   (ingredient " + (current + 1) + "/" + chopIndices.Count + ")\n\nPress SPACE when the knife reaches the green cut mark";
        cutProgressText.text = "CUTS: " + FilledCuts(cut) + "   Move carefully—the cut mark changes position";
    }

    private void MovePerfectZone()
    {
        perfectZone.anchoredPosition = new Vector2(Random.Range(-165f, 165f), 0f);
    }

    private static string FilledCuts(int count)
    {
        string value = "";
        for (int i = 0; i < 3; i++) value += i < count ? "■ " : "□ ";
        return value;
    }
}
