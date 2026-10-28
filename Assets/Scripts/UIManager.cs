using UnityEngine;
using UnityEngine.UI;

/// <summary>Formats the arcade HUD and all menu/feedback states.</summary>
public class UIManager : MonoBehaviour
{
    public Text timerText, scoreText, livesText, orderText, comboText, levelText, messageText;
    public Image tiltFill;
    public Text[] customerTexts;
    public Image[] patienceFills;
    public Image[] customerCardImages;
    public GameObject titlePanel, pausePanel, resultsPanel, hud;

    public void Refresh()
    {
        GameManager game = GameManager.Instance;
        OrderManager order = FindAnyObjectByType<OrderManager>();
        PlateController plate = FindAnyObjectByType<PlateController>();
        PlateInventory inventory = FindAnyObjectByType<PlateInventory>();
        if (game == null || order == null) return;

        timerText.text = Mathf.CeilToInt(game.TimeRemaining).ToString("00") + "s";
        timerText.color = game.TimeRemaining <= 10f ? new Color(1f, 0.3f, 0.25f) : Color.white;
        scoreText.text = "SCORE  " + game.Score.ToString("0000");
        string comboLabel = game.Combo >= 2 ? "COMBO " + game.Combo + "  x" + game.Multiplier : "BUILD A COMBO";
        comboText.text = comboLabel + (inventory == null ? "" : "   PLATE " + inventory.Count + "/" + inventory.maxSlots);
        livesText.text = new string('♥', game.Lives) + new string('♡', 3 - game.Lives);
        levelText.text = DifficultyLabel(game.Difficulty);
        if (tiltFill != null && plate != null) tiltFill.fillAmount = plate.EnergyNormalized;

        for (int customerIndex = 0; customerTexts != null && customerIndex < customerTexts.Length; customerIndex++)
        {
            CustomerTicket ticket = order.GetCustomer(customerIndex);
            bool active = ticket != null;
            customerTexts[customerIndex].transform.parent.gameObject.SetActive(active);
            if (!active) continue;
            customerTexts[customerIndex].text = "[" + (customerIndex + 1) + "] " + ticket.displayName.ToUpper() + "\n" +
                ticket.recipeName.ToUpper() + "\n" + RecipeLine(ticket.ingredients);
            patienceFills[customerIndex].fillAmount = ticket.patience / ticket.maxPatience;
            patienceFills[customerIndex].color = ticket.patience < ticket.maxPatience * 0.3f ? new Color(1f, 0.2f, 0.18f) : new Color(0.25f, 0.85f, 0.35f);
            customerCardImages[customerIndex].color = customerIndex == order.SelectedIndex ? new Color(1f, 0.78f, 0.25f, 0.98f) : new Color(0.96f, 0.9f, 0.75f, 0.9f);
        }

        string sequence = "TARGET: " + order.RecipeName.ToUpper() + "   ";
        for (int i = 0; i < order.CurrentOrder.Length; i++)
        {
            string item = ShortName(order.CurrentOrder[i]);
            if (i < order.Progress) sequence += "✓ " + item + "   ";
            else if (i == order.Progress) sequence += "[ " + item + " ]   ";
            else sequence += item + "   ";
        }
        orderText.text = sequence;
    }

    public void ShowTitle()
    {
        SetPanels(titlePanel);
        hud.SetActive(false);
        messageText.color = Color.white;
        messageText.fontSize = 42;
        messageText.text = "LAST PLATE\n\n60-SECOND KITCHEN RUSH\n\nComplete the order before the customer leaves\nHold SPACE to discard and tilt\nGolden food scores bonus points\n\nA / D or ARROWS to move\nENTER to start";
    }

    public void ShowPlaying() { SetPanels(null); hud.SetActive(true); ClearMessage(); }

    public void ShowPaused()
    {
        SetPanels(pausePanel); hud.SetActive(false); messageText.color = Color.white; messageText.fontSize = 44;
        messageText.text = "KITCHEN PAUSED\n\nPress ESCAPE to resume";
    }

    public void ShowResults()
    {
        SetPanels(resultsPanel); hud.SetActive(false);
        GameManager game = GameManager.Instance;
        string rank = game.Score >= 1800 ? "MASTER CHEF" : game.Score >= 900 ? "SOUS CHEF" : "LINE COOK";
        messageText.color = new Color(1f, 0.88f, 0.35f); messageText.fontSize = 42;
        messageText.text = "RUSH COMPLETE\n" + rank + "\n\nSCORE  " + game.Score + "\nORDERS  " + game.CompletedOrders + "\nBEST COMBO  " + game.BestCombo + "\n\nPress ENTER to play again";
        FindAnyObjectByType<AudioManager>().PlayRoundFinished();
    }

    public void ShowCatchFeedback(string text, bool golden)
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        CancelInvoke(nameof(ClearMessage));
        messageText.fontSize = golden ? 38 : 30;
        messageText.color = golden ? new Color(1f, 0.8f, 0.1f) : Color.white;
        messageText.text = text;
        Invoke(nameof(ClearMessage), 0.55f);
    }

    public void ShowOrderComplete(int points)
    {
        CancelInvoke(nameof(ClearMessage));
        messageText.fontSize = 40; messageText.color = new Color(0.35f, 1f, 0.55f);
        messageText.text = "ORDER SERVED!  +" + points;
        Invoke(nameof(ClearMessage), 1f);
    }

    public void ShowDifficulty(DifficultyLevel level)
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        CancelInvoke(nameof(ClearMessage));
        messageText.fontSize = 34; messageText.color = new Color(1f, 0.65f, 0.2f);
        messageText.text = DifficultyLabel(level);
        Invoke(nameof(ClearMessage), 1.2f);
    }

    private void SetPanels(GameObject active)
    {
        titlePanel.SetActive(active == titlePanel); pausePanel.SetActive(active == pausePanel); resultsPanel.SetActive(active == resultsPanel);
    }

    private void ClearMessage() { if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing) messageText.text = ""; }

    private static string DifficultyLabel(DifficultyLevel level)
    {
        if (level == DifficultyLevel.Preparation) return "PREPARATION";
        if (level == DifficultyLevel.DinnerRush) return "DINNER RUSH";
        return "KITCHEN CHAOS";
    }

    private static string ShortName(IngredientType type)
    {
        if (type == IngredientType.Tomato) return "TOMATO";
        if (type == IngredientType.Cheese) return "CHEESE";
        if (type == IngredientType.Lettuce) return "LETTUCE";
        return "MUSHROOM";
    }

    private static string RecipeLine(IngredientType[] ingredients)
    {
        string line = "";
        for (int i = 0; i < ingredients.Length; i++)
        {
            if (i > 0) line += " > ";
            line += ShortName(ingredients[i]);
        }
        return line;
    }
}
