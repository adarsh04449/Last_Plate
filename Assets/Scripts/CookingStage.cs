using UnityEngine;
using UnityEngine.UI;

/// <summary>Runs the raw-to-burnt cooking meter or cleanly skips heat for cold recipes.</summary>
public class CookingStage : MonoBehaviour
{
    public Image cookingFill;
    public Text instructionText;

    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private float progress;
    private bool active;
    private int beginFrame;

    public void Begin(MealRecipe nextRecipe, KitchenFlowManager manager)
    {
        recipe = nextRecipe;
        flow = manager;
        progress = 0f;
        beginFrame = Time.frameCount;
        cookingFill.fillAmount = 0f;
        active = true;
        instructionText.text = recipe.needsCooking
            ? "STOVE\n\nRAW  →  PERFECT  →  BURNT\n\nPress SPACE when the cooking meter reaches the green zone"
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

        progress = Mathf.Min(1f, progress + recipe.cookingSpeed * Time.deltaTime);
        cookingFill.fillAmount = progress;
        cookingFill.color = progress < 0.62f ? new Color(1f, 0.68f, 0.2f) : progress <= 0.84f ? new Color(0.3f, 0.9f, 0.35f) : new Color(0.65f, 0.16f, 0.08f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            int quality = progress >= 0.66f && progress <= 0.84f ? 100 : progress >= 0.5f && progress <= 0.93f ? 70 : 40;
            Finish(quality);
        }
        else if (progress >= 1f) Finish(25);
    }

    private void Finish(int quality)
    {
        active = false;
        flow.CookingComplete(quality);
    }
}
