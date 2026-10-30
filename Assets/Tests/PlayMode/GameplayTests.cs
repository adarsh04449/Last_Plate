using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameplayTests
{
    private GameManager game;
    private OrderManager orders;
    private PlateInventory inventory;
    private PlateController plate;
    private SpawnManager spawner;
    private UIManager ui;

    [UnitySetUp]
    public IEnumerator StartFreshRound()
    {
        Time.timeScale = 1f;
        yield return SceneManager.LoadSceneAsync("Game");
        yield return null;
        game = GameManager.Instance;
        orders = Object.FindAnyObjectByType<OrderManager>();
        inventory = Object.FindAnyObjectByType<PlateInventory>();
        plate = Object.FindAnyObjectByType<PlateController>();
        spawner = Object.FindAnyObjectByType<SpawnManager>();
        ui = Object.FindAnyObjectByType<UIManager>();
        game.StartRound();
        spawner.StopSpawning();
    }

    [TearDown]
    public void RestoreTimeScale() { Time.timeScale = 1f; }

    [Test]
    public void GameSceneIsIncludedInBuild()
    {
        Assert.That(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game.unity"), Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void ThreeCorrectIngredientsServeOneOrderAndClearPlate()
    {
        for (int i = 0; i < 3; i++) Catch(orders.NextRequired);
        Assert.That(game.CompletedOrders, Is.EqualTo(1));
        Assert.That(inventory.Count, Is.Zero);
        Assert.That(orders.Progress, Is.Zero);
        Assert.That(game.Lives, Is.EqualTo(3));
    }

    [Test]
    public void WrongIngredientCanBeDiscardedWithoutLosingLife()
    {
        Catch((IngredientType)(((int)orders.NextRequired + 1) % 4));
        Assert.That(game.Lives, Is.EqualTo(3));
        Assert.That(inventory.Count, Is.EqualTo(1));
        Assert.That(orders.Progress, Is.Zero);
        inventory.DiscardLast();
        Assert.That(inventory.Count, Is.Zero);
        Assert.That(game.Score, Is.Zero);
    }

    [Test]
    public void BurntIngredientCannotRemoveTwoLives()
    {
        FallingIngredient item = MakeItem(IngredientType.BurntFood);
        item.CaughtBy(plate);
        item.CaughtBy(plate);
        Assert.That(game.Lives, Is.EqualTo(2));
    }

    [Test]
    public void HoldingEmptyTiltDoesNotRestartOrDiscardAgain()
    {
        FillWrongPlate();
        for (int frame = 0; frame < 240; frame++) plate.AdvanceTilt(true, 1f / 60f);
        Assert.That(plate.IsTilted, Is.False);
        Assert.That(plate.TiltEnergy, Is.Zero);
        Assert.That(inventory.Count, Is.EqualTo(2), "One hold should discard only the newest item.");

        plate.AdvanceTilt(false, 0.5f);
        Assert.That(plate.TiltEnergy, Is.GreaterThan(0f));
        plate.AdvanceTilt(true, 1f / 60f);
        Assert.That(plate.IsTilted, Is.True);
        Assert.That(inventory.Count, Is.EqualTo(1), "A new hold can discard the next item.");
    }

    [Test]
    public void TiltMeterShrinksWithEnergyAndKeepsItsLeftEdge()
    {
        ui.Refresh();
        RectTransform meter = ui.tiltFill.rectTransform;
        float leftEdge = meter.anchoredPosition.x - meter.rect.width * meter.pivot.x;
        Assert.That(meter.rect.width, Is.EqualTo(205f).Within(0.01f));
        plate.AdvanceTilt(true, plate.maxTiltEnergy / 2f);
        ui.Refresh();
        Assert.That(meter.rect.width, Is.EqualTo(102.5f).Within(0.01f));
        Assert.That(meter.anchoredPosition.x - meter.rect.width * meter.pivot.x, Is.EqualTo(leftEdge).Within(0.01f));
        plate.AdvanceTilt(true, plate.maxTiltEnergy);
        ui.Refresh();
        Assert.That(meter.rect.width, Is.Zero);
        StringAssert.Contains("RELEASE SPACE", ui.tiltPromptText.text);
    }

    [UnityTest]
    public IEnumerator PausingWhileTiltedDoesNotDiscardAgainOnResume()
    {
        FillWrongPlate();
        plate.AdvanceTilt(true, 0.1f);
        Assert.That(inventory.Count, Is.EqualTo(2));
        game.PauseGame();
        yield return null;
        Assert.That(plate.IsTilted, Is.True);
        game.ResumeGame();
        plate.AdvanceTilt(true, 0.1f);
        Assert.That(inventory.Count, Is.EqualTo(2));
    }

    [Test]
    public void RecaughtRecipeSlotDoesNotFarmPointsOrCombo()
    {
        Catch(orders.NextRequired);
        int score = game.Score;
        for (int i = 0; i < 40; i++)
        {
            inventory.DiscardLast();
            Catch(orders.NextRequired);
        }
        Assert.That(game.Score, Is.EqualTo(score));
        Assert.That(game.Combo, Is.EqualTo(1));
        Assert.That(game.CompletedOrders, Is.Zero);
        Catch(orders.NextRequired);
        Catch(orders.NextRequired);
        Assert.That(game.CompletedOrders, Is.EqualTo(1));
    }

    [Test]
    public void GoldenRecatchDoesNotAwardTheSameSlotTwice()
    {
        Catch(orders.NextRequired, true);
        int score = game.Score;
        inventory.DiscardLast();
        Catch(orders.NextRequired, true);
        Assert.That(game.Score, Is.EqualTo(score));
        Assert.That(game.Combo, Is.EqualTo(1));
    }

    [Test]
    public void NewTicketResetsSlotRewards()
    {
        Catch(orders.NextRequired);
        inventory.DiscardLast();
        orders.CreateNewOrder();
        int score = game.Score;
        Catch(orders.NextRequired);
        Assert.That(game.Score, Is.GreaterThan(score));
        Assert.That(game.Combo, Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator MatchingReplacementTicketServesRetainedFullPlate()
    {
        orders.CurrentCustomer.ingredients = new[] { IngredientType.Tomato, IngredientType.Cheese, IngredientType.Lettuce };
        Catch(IngredientType.Mushroom);
        Catch(IngredientType.Cheese);
        Catch(IngredientType.Tomato);
        Assert.That(game.CompletedOrders, Is.Zero);
        // Use a repeatable sequence of replacements until Mushroom Melt arrives.
        Random.InitState(12345);
        for (int i = 0; i < 100 && game.CompletedOrders == 0; i++)
        {
            orders.CurrentCustomer.patience = 0f;
            yield return null;
        }
        Assert.That(game.CompletedOrders, Is.EqualTo(1));
        Assert.That(inventory.Count, Is.Zero);
        Assert.That(orders.Progress, Is.Zero);
        Assert.That(game.Lives, Is.EqualTo(3));
    }

    [UnityTest]
    public IEnumerator PauseFreezesTimerAndResumeContinuesIt()
    {
        game.PauseGame();
        float remaining = game.TimeRemaining;
        yield return new WaitForSecondsRealtime(0.1f);
        Assert.That(game.TimeRemaining, Is.EqualTo(remaining));
        game.ResumeGame();
        yield return null;
        Assert.That(game.TimeRemaining, Is.LessThan(remaining));
    }

    [Test]
    public void NewOrderUpdatesTicketAndPatienceMeter()
    {
        string oldRecipe = orders.CurrentCustomer.recipeName;
        orders.CreateNewOrder();
        CustomerTicket ticket = orders.CurrentCustomer;
        Assert.That(ticket.recipeName, Is.Not.EqualTo(oldRecipe));
        StringAssert.Contains(ticket.recipeName.ToUpper(), ui.customerText.text);
        Assert.That(ui.customerPortrait.sprite, Is.SameAs(ui.customerSprites[(int)ticket.type]));

        ticket.patience = ticket.maxPatience / 2f;
        ui.Refresh();
        Assert.That(ui.patienceFill.rectTransform.rect.width, Is.EqualTo(150f).Within(0.01f));
    }

    [Test]
    public void ThirdBurntCatchEndsRoundAndRestartResetsState()
    {
        Catch(orders.NextRequired);
        plate.AdvanceTilt(true, 0.5f);
        plate.AdvanceTilt(false, 0f);
        for (int i = 0; i < 3; i++) MakeItem(IngredientType.BurntFood).CaughtBy(plate);
        Assert.That(game.State, Is.EqualTo(GameState.Results));
        Assert.That(game.Lives, Is.Zero);
        game.StartRound();
        spawner.StopSpawning();
        Assert.That(game.State, Is.EqualTo(GameState.Playing));
        Assert.That(game.TimeRemaining, Is.EqualTo(120f));
        Assert.That(game.Lives, Is.EqualTo(3));
        Assert.That(game.Score, Is.Zero);
        Assert.That(game.Combo, Is.Zero);
        Assert.That(inventory.Count, Is.Zero);
        Assert.That(orders.Progress, Is.Zero);
        Assert.That(plate.IsTilted, Is.False);
        Assert.That(plate.TiltEnergy, Is.EqualTo(plate.maxTiltEnergy));
    }

    [UnityTest, Timeout(10000)]
    public IEnumerator DifficultyAdvancesInThirdsAndTimerEndsRound()
    {
        // Shorten only this test; it exercises the same phase calculation as a 120-second round.
        game.roundDuration = 3f;
        game.StartRound();
        spawner.StopSpawning();
        Assert.That(game.Difficulty, Is.EqualTo(DifficultyLevel.Preparation));
        yield return new WaitUntil(() => game.Difficulty != DifficultyLevel.Preparation);
        Assert.That(game.Difficulty, Is.EqualTo(DifficultyLevel.DinnerRush));
        yield return new WaitUntil(() => game.Difficulty != DifficultyLevel.DinnerRush);
        Assert.That(game.Difficulty, Is.EqualTo(DifficultyLevel.KitchenChaos));
        yield return new WaitUntil(() => game.State == GameState.Results);
        Assert.That(game.TimeRemaining, Is.Zero);
    }

    private void FillWrongPlate()
    {
        orders.CurrentCustomer.ingredients = new[] { IngredientType.Tomato, IngredientType.Cheese, IngredientType.Lettuce };
        for (int i = 0; i < 3; i++) Catch(IngredientType.Mushroom);
    }

    private FallingIngredient MakeItem(IngredientType type, bool golden = false)
    {
        GameObject obj = new GameObject("Test " + type);
        obj.transform.SetParent(spawner.ingredientContainer);
        obj.AddComponent<SpriteRenderer>().sprite = type == IngredientType.BurntFood ? spawner.burntSprite : spawner.ingredientSprites[(int)type];
        FallingIngredient item = obj.AddComponent<FallingIngredient>();
        item.ingredientType = type;
        item.isGolden = golden;
        return item;
    }

    private void Catch(IngredientType type, bool golden = false)
    {
        FallingIngredient item = MakeItem(type, golden);
        inventory.TryCatch(item);
        Object.Destroy(item.gameObject);
    }
}
