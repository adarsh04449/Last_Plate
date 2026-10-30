using System.Collections.Generic;
using UnityEngine;

public enum CustomerType { Regular, Impatient, VIP }

public sealed class CustomerTicket
{
    public CustomerType type;
    public string displayName;
    public string recipeName;
    public IngredientType[] ingredients;
    public float patience;
    public float maxPatience;
    public int rewardBonus;
}

public class OrderManager : MonoBehaviour
{
    public int Progress { get; private set; }
    public CustomerTicket CurrentCustomer { get; private set; }
    public IngredientType[] CurrentOrder => CurrentCustomer == null ? fallbackOrder : CurrentCustomer.ingredients;
    public IngredientType NextRequired => CurrentOrder[Mathf.Clamp(Progress, 0, CurrentOrder.Length - 1)];

    private readonly bool[] rewardedSlots = new bool[3];
    private readonly IngredientType[] fallbackOrder = { IngredientType.Tomato, IngredientType.Cheese, IngredientType.Lettuce };
    private readonly IngredientType[][] recipes = {
        new[] { IngredientType.Tomato, IngredientType.Cheese, IngredientType.Lettuce },
        new[] { IngredientType.Mushroom, IngredientType.Cheese, IngredientType.Tomato },
        new[] { IngredientType.Lettuce, IngredientType.Tomato, IngredientType.Mushroom },
        new[] { IngredientType.Cheese, IngredientType.Mushroom, IngredientType.Cheese },
        new[] { IngredientType.Tomato, IngredientType.Lettuce, IngredientType.Tomato }
    };
    private readonly string[] recipeNames = { "Garden Stack", "Mushroom Melt", "Fresh Skillet", "Double Cheese", "Tomato Tower" };
    private int lastRecipe = -1;
    private UIManager ui;
    private AudioManager audioManager;
    private PlateInventory inventory;

    private void Start()
    {
        ui = GetComponent<UIManager>();
        audioManager = GetComponent<AudioManager>();
        inventory = FindAnyObjectByType<PlateInventory>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || CurrentCustomer == null) return;
        CurrentCustomer.patience -= Time.deltaTime;
        if (CurrentCustomer.patience <= 0f) ExpireCustomer();
    }

    public void CreateNewOrder()
    {
        CurrentCustomer = BuildCustomer();
        System.Array.Clear(rewardedSlots, 0, rewardedSlots.Length);
        Progress = 0;
        ui.Refresh();
    }

    public bool ProcessPlateCatch(IReadOnlyList<IngredientType> plate, bool golden)
    {
        if (GameManager.Instance.State != GameState.Playing || CurrentCustomer == null || plate.Count == 0) return false;
        SyncPlate(plate);
        if (Progress == plate.Count)
        {
            int slot = plate.Count - 1;
            bool newlyRewarded = !rewardedSlots[slot];
            if (newlyRewarded)
            {
                // Discarding a correct item should not let the same slot pay out again.
                rewardedSlots[slot] = true;
                GameManager.Instance.RegisterCorrectIngredient(golden);
            }
            else ui.ShowCatchFeedback("GOOD CATCH", false);
            audioManager.PlayCatch(golden && newlyRewarded);
        }
        else
        {
            GameManager.Instance.BreakCombo();
            ui.ShowCatchFeedback("WRONG — TILT TO DISCARD", false);
            audioManager.PlayWrongIngredient();
        }

        if (TryCompleteOrder(plate.Count)) return true;
        ui.Refresh();
        return false;
    }

    public void SyncPlate(IReadOnlyList<IngredientType> plate)
    {
        Progress = 0;
        int count = Mathf.Min(plate.Count, CurrentOrder.Length);
        while (Progress < count && plate[Progress] == CurrentOrder[Progress]) Progress++;
        ui.Refresh();
    }

    private CustomerTicket BuildCustomer()
    {
        int recipe;
        do
        {
            recipe = Random.Range(0, recipes.Length);
        }
        while (recipe == lastRecipe && recipes.Length > 1);
        lastRecipe = recipe;
        DifficultyLevel difficulty = GameManager.Instance == null ? DifficultyLevel.Preparation : GameManager.Instance.Difficulty;
        float roll = Random.value;
        CustomerType type = difficulty == DifficultyLevel.Preparation ? CustomerType.Regular :
            difficulty == DifficultyLevel.DinnerRush && roll < 0.35f ? CustomerType.Impatient :
            difficulty == DifficultyLevel.KitchenChaos && roll < 0.28f ? CustomerType.VIP :
            difficulty == DifficultyLevel.KitchenChaos && roll < 0.62f ? CustomerType.Impatient : CustomerType.Regular;
        float patience = type == CustomerType.Impatient ? 13f : type == CustomerType.VIP ? 24f : 20f;
        string name = type == CustomerType.Impatient ? "Rushed Diner" : type == CustomerType.VIP ? "VIP Critic" : "Hungry Guest";
        return new CustomerTicket {
            type = type, displayName = name, recipeName = recipeNames[recipe],
            ingredients = (IngredientType[])recipes[recipe].Clone(), patience = patience, maxPatience = patience,
            rewardBonus = type == CustomerType.VIP ? 150 : type == CustomerType.Impatient ? 60 : 0
        };
    }

    private bool TryCompleteOrder(int plateCount)
    {
        if (Progress != CurrentOrder.Length || plateCount != CurrentOrder.Length) return false;
        GameManager.Instance.AddCompletedOrder(CurrentCustomer.rewardBonus);
        CreateNewOrder();
        return true;
    }

    private void ExpireCustomer()
    {
        CreateNewOrder();
        // Burnt food is the only life-losing hazard for now; patience only replaces the order.
        ui.ShowCatchFeedback("CUSTOMER LEFT — NEW ORDER", false);
        SyncPlate(inventory.CurrentTypes);
        if (TryCompleteOrder(inventory.Count)) inventory.Clear();
    }
}
