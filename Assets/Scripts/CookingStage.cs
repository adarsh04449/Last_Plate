using UnityEngine;
using UnityEngine.UI;

/// <summary>Runs the raw-to-burnt cooking meter or cleanly skips heat for cold recipes.</summary>
public class CookingStage : MonoBehaviour
{
    public Image cookingFill;
    public RectTransform heatMarker;
    public Text instructionText;
    public Text heatStatusText;
    public GameObject[] cookingControls;

    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private float progress;
    private bool active;
    private int beginFrame;
    private float heat;
    private float elapsed;
    private float qualityTotal;

    public void Begin(MealRecipe nextRecipe, KitchenFlowManager manager)
    {
        recipe = nextRecipe;
        flow = manager;
        progress = 0f;
        heat = 0.34f;
        elapsed = 0f;
        qualityTotal = 0f;
        beginFrame = Time.frameCount;
        cookingFill.fillAmount = 0f;
        active = true;
        foreach (GameObject controlObject in cookingControls) controlObject.SetActive(recipe.needsCooking);
        instructionText.text = recipe.needsCooking
            ? "STOVE\n\nCONTROL THE HEAT UNTIL THE MEAL IS COOKED\n\nUse A / D or ARROWS to keep the flame marker in green"
            : "MIXING COUNTER\n\n" + recipe.name.ToUpper() + " DOES NOT NEED HEAT\n\nPress ENTER to mix and continue";
    }

    private void Update()
    {
        if (!active || Time.timeScale == 0f || Time.frameCount <= beginFrame) return;
        if (!recipe.needsCooking)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Finish(100);
            return;
        }

        float control = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) control = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) control = 1f;
        heat = Mathf.Clamp01(heat + (0.075f + control * 0.42f) * Time.deltaTime);
        heatMarker.anchoredPosition = new Vector2(Mathf.Lerp(-250f, 250f, heat), 0f);

        progress = Mathf.Min(1f, progress + recipe.cookingSpeed * Time.deltaTime);
        elapsed += Time.deltaTime;
        int momentQuality = heat >= 0.42f && heat <= 0.62f ? 100 : heat >= 0.28f && heat <= 0.76f ? 65 : 25;
        qualityTotal += momentQuality * Time.deltaTime;
        cookingFill.fillAmount = progress;
        cookingFill.color = Color.Lerp(new Color(1f, 0.68f, 0.2f), new Color(0.85f, 0.28f, 0.08f), progress);
        heatStatusText.text = "COOKING " + Mathf.RoundToInt(progress * 100f) + "%     HEAT: " +
            (momentQuality == 100 ? "PERFECT" : heat < 0.42f ? "TOO LOW" : "TOO HIGH");

        if (progress >= 1f) Finish(Mathf.RoundToInt(qualityTotal / Mathf.Max(0.01f, elapsed)));
    }

    private void Finish(int quality)
    {
        active = false;
        flow.CookingComplete(quality);
    }
}
