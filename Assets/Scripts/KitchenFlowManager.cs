using UnityEngine;
using UnityEngine.UI;

public enum KitchenPage { Title, Order, Collection, Preparation, Cooking, Serving, OrderResult, SessionResult }

public sealed class MealRecipe
{
    public string name;
    public string customer;
    public IngredientType[] ingredients;
    public bool[] chop;
    public bool needsCooking;
    public float cookingSpeed;
}

/// <summary>Moves one customer order through the six focused cooking pages.</summary>
public class KitchenFlowManager : MonoBehaviour
{
    public GameObject titlePage, orderPage, collectionPage, preparationPage, cookingPage;
    public GameObject servingPage, resultPage, sessionPage, pauseOverlay;
    public Text titleText, orderText, servingText, resultText, sessionText;
    public CollectionStage collectionStage;
    public PreparationStage preparationStage;
    public CookingStage cookingStage;
    public PlatingStage platingStage;

    public KitchenPage Page { get; private set; }
    public MealRecipe CurrentRecipe => recipes[currentRecipeIndex];

    private MealRecipe[] recipes;
    private int currentRecipeIndex;
    private int collectionQuality;
    private int preparationQuality;
    private int cookingQuality;
    private int sessionScore;
    private int sessionStars;
    private bool paused;
    private int pageChangedFrame;
    private bool canServe;

    private void Awake()
    {
        recipes = new[]
        {
            new MealRecipe {
                name = "Garden Salad", customer = "Hungry Guest",
                ingredients = new[] { IngredientType.Tomato, IngredientType.Lettuce, IngredientType.Mushroom },
                chop = new[] { true, true, false }, needsCooking = false, cookingSpeed = 0f
            },
            new MealRecipe {
                name = "Mushroom Melt", customer = "Lunch Regular",
                ingredients = new[] { IngredientType.Mushroom, IngredientType.Cheese, IngredientType.Tomato },
                chop = new[] { true, false, true }, needsCooking = true, cookingSpeed = 0.19f
            },
            new MealRecipe {
                name = "Veggie Skillet", customer = "Food Critic",
                ingredients = new[] { IngredientType.Tomato, IngredientType.Mushroom, IngredientType.Cheese },
                chop = new[] { true, true, false }, needsCooking = true, cookingSpeed = 0.25f
            }
        };
    }

    private void Start()
    {
        Time.timeScale = 1f;
        ShowTitle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Page != KitchenPage.Title && Page != KitchenPage.SessionResult)
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            if (pauseOverlay != null) pauseOverlay.SetActive(paused);
            return;
        }
        if (paused) return;

        bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        if (!enter || Time.frameCount <= pageChangedFrame) return;

        if (Page == KitchenPage.Title || Page == KitchenPage.SessionResult) StartSession();
        else if (Page == KitchenPage.Order) StartCollection();
        else if (Page == KitchenPage.Serving && canServe) ServeOrder();
        else if (Page == KitchenPage.OrderResult) ContinueAfterResult();
    }

    private void ShowTitle()
    {
        ChangePage(KitchenPage.Title, titlePage);
        titleText.text = "LAST PLATE\n\nA THREE-COURSE KITCHEN STORY\n\nTake one customer at a time through\nCOLLECT  •  PREPARE  •  COOK  •  SERVE\n\nNo global timer. Focus on quality.\n\nPress ENTER to open the kitchen";
    }

    private void StartSession()
    {
        paused = false;
        Time.timeScale = 1f;
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        currentRecipeIndex = 0;
        sessionScore = 0;
        sessionStars = 0;
        ShowOrder();
    }

    private void ShowOrder()
    {
        ChangePage(KitchenPage.Order, orderPage);
        MealRecipe recipe = CurrentRecipe;
        orderText.text = "CUSTOMER " + (currentRecipeIndex + 1) + " OF " + recipes.Length + "\n\n" +
            recipe.customer.ToUpper() + " ORDERS\n" + recipe.name.ToUpper() + "\n\n" +
            IngredientLine(recipe) + "\n\n" + StepLine(recipe) +
            "\n\nTake your time and read the ticket.\nPress ENTER to collect ingredients";
    }

    private void StartCollection()
    {
        ChangePage(KitchenPage.Collection, collectionPage);
        collectionStage.Begin(CurrentRecipe, this);
    }

    public void CollectionComplete(int mistakes)
    {
        collectionQuality = Mathf.Max(25, 100 - mistakes * 15);
        ChangePage(KitchenPage.Preparation, preparationPage);
        preparationStage.Begin(CurrentRecipe, this);
    }

    public void PreparationComplete(int quality)
    {
        preparationQuality = quality;
        ChangePage(KitchenPage.Cooking, cookingPage);
        cookingStage.Begin(CurrentRecipe, this);
    }

    public void CookingComplete(int quality)
    {
        cookingQuality = quality;
        canServe = false;
        ChangePage(KitchenPage.Serving, servingPage);
        platingStage.Begin(CurrentRecipe, this);
    }

    public void PlatingComplete() { canServe = true; }

    private void ServeOrder()
    {
        FindAnyObjectByType<AudioManager>()?.PlayOrderComplete();
        int average = Mathf.RoundToInt((collectionQuality + preparationQuality + cookingQuality) / 3f);
        int stars = average >= 90 ? 3 : average >= 65 ? 2 : 1;
        int points = average * 10;
        sessionScore += points;
        sessionStars += stars;

        string reaction = stars == 3 ? "DELICIOUS!" : stars == 2 ? "NOT BAD!" : "OH NO — KEEP PRACTICING!";
        ChangePage(KitchenPage.OrderResult, resultPage);
        resultText.text = reaction + "\n\n" + Stars(stars) + "\n\n" +
            "COLLECTION  " + collectionQuality + "%\n" +
            "PREPARATION  " + preparationQuality + "%\n" +
            "COOKING  " + cookingQuality + "%\n\n" +
            "ORDER SCORE  " + points + "\n\nPress ENTER to continue";
    }

    private void ContinueAfterResult()
    {
        currentRecipeIndex++;
        if (currentRecipeIndex < recipes.Length) ShowOrder();
        else ShowSessionResult();
    }

    private void ShowSessionResult()
    {
        ChangePage(KitchenPage.SessionResult, sessionPage);
        string rank = sessionStars >= 8 ? "MASTER CHEF" : sessionStars >= 5 ? "SOUS CHEF" : "KITCHEN TRAINEE";
        sessionText.text = "KITCHEN CLOSED\n\n" + rank + "\n\n" + Stars(sessionStars) + "\n\n" +
            "THREE CUSTOMERS SERVED\nTOTAL SCORE  " + sessionScore +
            "\n\nPress ENTER to play another session";
        FindAnyObjectByType<AudioManager>()?.PlayRoundFinished();
    }

    private void SetPage(GameObject active)
    {
        GameObject[] pages = { titlePage, orderPage, collectionPage, preparationPage, cookingPage, servingPage, resultPage, sessionPage };
        foreach (GameObject page in pages) if (page != null) page.SetActive(page == active);
    }

    private void ChangePage(KitchenPage nextPage, GameObject pageObject)
    {
        Page = nextPage;
        pageChangedFrame = Time.frameCount;
        SetPage(pageObject);
    }

    public static string IngredientName(IngredientType type)
    {
        if (type == IngredientType.Tomato) return "TOMATO";
        if (type == IngredientType.Cheese) return "CHEESE";
        if (type == IngredientType.Lettuce) return "LETTUCE";
        return "MUSHROOM";
    }

    private static string IngredientLine(MealRecipe recipe)
    {
        return IngredientName(recipe.ingredients[0]) + "  +  " + IngredientName(recipe.ingredients[1]) + "  +  " + IngredientName(recipe.ingredients[2]);
    }

    private static string StepLine(MealRecipe recipe)
    {
        string chopped = "CHOP ";
        bool first = true;
        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            if (!recipe.chop[i]) continue;
            if (!first) chopped += " + ";
            chopped += IngredientName(recipe.ingredients[i]);
            first = false;
        }
        return chopped + (recipe.needsCooking ? "  →  COOK  →  SERVE" : "  →  MIX  →  SERVE");
    }

    private static string Stars(int count)
    {
        string stars = "";
        for (int i = 0; i < count; i++) stars += "★";
        return stars;
    }
}
