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
    public int chopIndex;
}

/// <summary>Owns the single active customer ticket and validates the plate against it.</summary>
public class OrderManager : MonoBehaviour
{
    public int Progress { get; private set; }
    public bool IsPlateReady { get; private set; }
    public int SelectedIndex { get; private set; }
    public int CustomerCount => customers.Count;
    public IngredientType[] CurrentOrder => customers.Count == 0 ? fallbackOrder : customers[SelectedIndex].ingredients;
    public string RecipeName => customers.Count == 0 ? "Garden Stack" : customers[SelectedIndex].recipeName;
    public IngredientType NextRequired => CurrentOrder[Mathf.Clamp(Progress, 0, CurrentOrder.Length - 1)];
    public int ChopIndex => customers.Count == 0 ? 0 : customers[SelectedIndex].chopIndex;

    private readonly List<CustomerTicket> customers = new List<CustomerTicket>();
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

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        // One ticket stays active so the catch, chop, and serve route remains readable.
        EnsureCustomerCount(1);

        for (int i = customers.Count - 1; i >= 0; i--)
        {
            customers[i].patience -= Time.deltaTime;
            if (customers[i].patience <= 0f) ExpireCustomer(i);
        }
    }

    public CustomerTicket GetCustomer(int index) => index >= 0 && index < customers.Count ? customers[index] : null;

    public void CreateNewOrder()
    {
        customers.Clear();
        SelectedIndex = 0;
        Progress = 0;
        IsPlateReady = false;
        EnsureCustomerCount(1);
        FindAnyObjectByType<UIManager>()?.Refresh();
    }

    public bool ProcessPlateCatch(IReadOnlyList<IngredientType> plate, IReadOnlyList<bool> prepared, bool golden)
    {
        if (GameManager.Instance.State != GameState.Playing || customers.Count == 0) return false;
        SyncPlate(plate, prepared);
        if (Progress == plate.Count)
        {
            GameManager.Instance.RegisterCorrectIngredient(golden);
            FindAnyObjectByType<AudioManager>().PlayCatch(golden);
        }
        else
        {
            GameManager.Instance.BreakCombo();
            FindAnyObjectByType<UIManager>().ShowCatchFeedback("WRONG — TILT TO DISCARD", false);
            FindAnyObjectByType<AudioManager>().PlayWrongIngredient();
        }

        if (Progress >= CurrentOrder.Length && plate.Count == CurrentOrder.Length)
        {
            FindAnyObjectByType<UIManager>().ShowCatchFeedback(IsPlateReady ? "ORDER READY — TAKE IT RIGHT" : "NEEDS CHOPPING — GO LEFT", IsPlateReady);
            FindAnyObjectByType<UIManager>().Refresh();
            return IsPlateReady;
        }
        FindAnyObjectByType<UIManager>().Refresh();
        return false;
    }

    public void SyncPlate(IReadOnlyList<IngredientType> plate, IReadOnlyList<bool> prepared)
    {
        Progress = 0;
        int count = Mathf.Min(plate.Count, CurrentOrder.Length);
        while (Progress < count && plate[Progress] == CurrentOrder[Progress]) Progress++;
        bool chopped = ChopIndex < prepared.Count && prepared[ChopIndex];
        IsPlateReady = Progress == CurrentOrder.Length && plate.Count == CurrentOrder.Length && chopped;
        FindAnyObjectByType<UIManager>()?.Refresh();
    }

    public bool RequiresChop(int index) => index == ChopIndex;

    /// <summary>Scores a finished plate only when it reaches the serving window.</summary>
    public bool TryServe(IReadOnlyList<IngredientType> plate, IReadOnlyList<bool> prepared)
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || customers.Count == 0)
            return false;

        if (!IsPlateReady || plate.Count != CurrentOrder.Length || ChopIndex >= prepared.Count || !prepared[ChopIndex])
        {
            FindAnyObjectByType<UIManager>()?.ShowCatchFeedback("FINISH THE ORDER FIRST", false);
            return false;
        }

        for (int i = 0; i < CurrentOrder.Length; i++)
        {
            if (plate[i] != CurrentOrder[i])
            {
                IsPlateReady = false;
                FindAnyObjectByType<UIManager>()?.ShowCatchFeedback("THAT IS NOT THIS ORDER", false);
                return false;
            }
        }

        int bonus = customers[SelectedIndex].rewardBonus;
        IsPlateReady = false;
        GameManager.Instance.AddCompletedOrder(bonus);
        ReplaceCustomer(SelectedIndex);
        return true;
    }

    private void EnsureCustomerCount(int target)
    {
        while (customers.Count < target) customers.Add(BuildCustomer());
        SelectedIndex = Mathf.Clamp(SelectedIndex, 0, customers.Count - 1);
    }

    private CustomerTicket BuildCustomer()
    {
        int recipe;
        do recipe = Random.Range(0, recipes.Length); while (recipe == lastRecipe && recipes.Length > 1);
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
            rewardBonus = type == CustomerType.VIP ? 150 : type == CustomerType.Impatient ? 60 : 0,
            chopIndex = PickChopIndex(recipes[recipe])
        };
    }

    private static int PickChopIndex(IngredientType[] recipe)
    {
        List<int> choices = new List<int>();
        for (int i = 0; i < recipe.Length; i++)
            if (recipe[i] != IngredientType.Cheese) choices.Add(i);
        return choices.Count == 0 ? 0 : choices[Random.Range(0, choices.Count)];
    }

    private void ReplaceCustomer(int index)
    {
        customers[index] = BuildCustomer();
        Progress = 0;
        IsPlateReady = false;
        FindAnyObjectByType<UIManager>().Refresh();
    }

    private void ExpireCustomer(int index)
    {
        bool selectedExpired = index == SelectedIndex;
        customers[index] = BuildCustomer();
        if (selectedExpired) IsPlateReady = false;
        GameManager.Instance.LoseLife("CUSTOMER LEFT!");
        FindAnyObjectByType<AudioManager>().PlayLifeLost();
        if (selectedExpired)
        {
            PlateInventory inventory = FindAnyObjectByType<PlateInventory>();
            SyncPlate(inventory == null ? new IngredientType[0] : inventory.CurrentTypes,
                inventory == null ? new bool[0] : inventory.CurrentPrepared);
        }
    }

}
