using UnityEngine;
using UnityEngine.UI;

/// <summary>Places the three prepared ingredients onto the final plate one at a time.</summary>
public class PlatingStage : MonoBehaviour
{
    public Text instructionText;
    public Text plateStatusText;
    public Image[] ingredientSlots;
    public Sprite[] ingredientSprites;

    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private int placed;
    private bool active;
    private int beginFrame;

    public void Begin(MealRecipe nextRecipe, KitchenFlowManager manager)
    {
        recipe = nextRecipe;
        flow = manager;
        placed = 0;
        active = true;
        beginFrame = Time.frameCount;
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            ingredientSlots[i].sprite = ingredientSprites[(int)recipe.ingredients[i]];
            ingredientSlots[i].color = new Color(1f, 1f, 1f, 0.12f);
        }
        Refresh();
    }

    private void Update()
    {
        if (!active || Time.timeScale == 0f || Time.frameCount <= beginFrame) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        ingredientSlots[placed].color = Color.white;
        FindAnyObjectByType<AudioManager>()?.PlayCatch(false);
        placed++;
        if (placed >= recipe.ingredients.Length)
        {
            active = false;
            instructionText.text = "PLATING COMPLETE!\n\n" + recipe.name.ToUpper() + " IS READY";
            plateStatusText.text = "Press ENTER to serve the customer";
            flow.PlatingComplete();
        }
        else Refresh();
    }

    private void Refresh()
    {
        instructionText.text = "PLATING COUNTER\n\nPLACE " + KitchenFlowManager.IngredientName(recipe.ingredients[placed]);
        plateStatusText.text = "Press SPACE to add the next ingredient   •   " + placed + "/3 placed";
    }
}
