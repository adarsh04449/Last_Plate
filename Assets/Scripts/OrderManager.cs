using UnityEngine;

/// <summary>Chooses named recipes and validates catches in strict sequence.</summary>
public class OrderManager : MonoBehaviour
{
    public IngredientType[] CurrentOrder { get; private set; } = new IngredientType[3];
    public int Progress { get; private set; }
    public string RecipeName { get; private set; } = "Garden Stack";
    public IngredientType NextRequired => CurrentOrder[Mathf.Clamp(Progress, 0, 2)];

    private readonly IngredientType[][] recipes = {
        new[] { IngredientType.Tomato, IngredientType.Cheese, IngredientType.Lettuce },
        new[] { IngredientType.Mushroom, IngredientType.Cheese, IngredientType.Tomato },
        new[] { IngredientType.Lettuce, IngredientType.Tomato, IngredientType.Mushroom },
        new[] { IngredientType.Cheese, IngredientType.Mushroom, IngredientType.Cheese },
        new[] { IngredientType.Tomato, IngredientType.Lettuce, IngredientType.Tomato }
    };
    private readonly string[] recipeNames = { "Garden Stack", "Mushroom Melt", "Fresh Skillet", "Double Cheese", "Tomato Tower" };
    private int lastRecipe = -1;

    public void CreateNewOrder()
    {
        int choice;
        do choice = Random.Range(0, recipes.Length); while (choice == lastRecipe && recipes.Length > 1);
        lastRecipe = choice;
        recipes[choice].CopyTo(CurrentOrder, 0);
        RecipeName = recipeNames[choice];
        Progress = 0;
        FindAnyObjectByType<UIManager>()?.Refresh();
    }

    public void ProcessIngredient(IngredientType ingredient, bool golden)
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if (ingredient == NextRequired)
        {
            Progress++;
            GameManager.Instance.RegisterCorrectIngredient(golden);
            FindAnyObjectByType<AudioManager>().PlayCatch(golden);
            if (Progress >= CurrentOrder.Length)
            {
                GameManager.Instance.AddCompletedOrder();
                CreateNewOrder();
            }
        }
        else
        {
            Progress = 0;
            GameManager.Instance.LoseLife();
            FindAnyObjectByType<AudioManager>().PlayLifeLost();
        }
        FindAnyObjectByType<UIManager>().Refresh();
    }
}
